using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using DSCMS.Data;
using DSCMS.Models;
using DSCMS.Models.DTOs;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DSCMS.Services
{
  public class BackupService : IBackupService
  {
    private const string SnapshotPrefix = "pre-restore-";
    private const long MaxManifestBytes = 64L * 1024 * 1024;
    private const int MaxListedProblems = 20;

    // Tables every DSCMS database has, whatever its migration level
    private static readonly string[] RequiredTables = { "__EFMigrationsHistory", "ContentTypes", "Contents", "Layouts", "Templates", "AspNetUsers" };

    // Identity tables, children before parents. On restore these are kept from the target site.
    private static readonly string[] IdentityTables =
    {
      "AspNetUserTokens", "AspNetUserLogins", "AspNetUserClaims", "AspNetUserRoles", "AspNetRoleClaims", "AspNetUsers", "AspNetRoles"
    };

    // (table, column) pairs that reference AspNetUsers.Id
    private static readonly (string Table, string Column)[] UserReferences =
    {
      ("Contents", "CreatedBy"), ("Contents", "LastUpdatedBy"), ("StaticUploads", "UploadedBy")
    };

    // Already compressed; deflating them again only costs time
    private static readonly HashSet<string> StoredExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
      ".zip", ".gz", ".7z", ".rar", ".png", ".jpg", ".jpeg", ".gif", ".webp", ".mp3", ".mp4", ".ogg", ".webm",
      ".woff", ".woff2", ".pdf", ".docx", ".xlsx", ".pptx", ".odt", ".swf", ".unityweb", ".br"
    };

    private static readonly Regex SnapshotNamePattern = new("^[A-Za-z0-9][A-Za-z0-9._-]*\\.zip$", RegexOptions.Compiled);

    private static readonly JsonSerializerOptions ManifestJsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private readonly IStaticFileStorageService _storage;
    private readonly MaintenanceState _maintenance;
    private readonly ILogger<BackupService> _logger;
    private readonly string _liveConnectionString;
    private readonly string _environmentName;
    private readonly string _snapshotRoot;

    private readonly object _stagedLock = new();
    private StagedRestore? _staged;

    public BackupSettings Settings { get; }

    public BackupService(IConfigurationService configurationService, IWebHostEnvironment environment, IStaticFileStorageService storage,
      MaintenanceState maintenance, ILogger<BackupService> logger)
      : this(configurationService.GetBackupSettings(), environment.ContentRootPath, configurationService.GetDatabaseSettings().ConnectionString,
          environment.EnvironmentName, storage, maintenance, logger)
    {
    }

    public BackupService(BackupSettings settings, string contentRootPath, string liveConnectionString, string environmentName,
      IStaticFileStorageService storage, MaintenanceState maintenance, ILogger<BackupService> logger)
    {
      Settings = settings;
      _storage = storage;
      _maintenance = maintenance;
      _logger = logger;
      _liveConnectionString = liveConnectionString;
      _environmentName = environmentName;

      var snapshotPath = Path.IsPathRooted(settings.StoragePath)
        ? settings.StoragePath
        : Path.Combine(contentRootPath, settings.StoragePath);
      _snapshotRoot = Path.GetFullPath(snapshotPath);
    }

    // -------------------------------------------------------------------------
    // Status

    public Task<BackupStatusDto> GetStatusAsync(CancellationToken cancellationToken = default)
    {
      var status = new BackupStatusDto { IsBusy = _maintenance.IsBusy, Snapshots = ListSnapshots().ToList() };

      using (var connection = OpenConnection(_liveConnectionString))
      {
        status.Counts = ReadCounts(connection);
        status.Migrations = ReadMigrations(connection);

        // Sizes only; hashing every file on each page load would be slow
        var files = EnumerateStaticFiles().ToDictionary(f => f.RelativePath, f => (f.SizeBytes, (string?)null), StringComparer.Ordinal);
        status.Counts.StaticFiles = files.Count;
        status.Counts.StaticBytes = files.Values.Sum(f => f.SizeBytes);

        var (problems, untracked) = CrossCheck(ReadStaticFileRows(connection), files);
        status.Warnings.AddRange(problems);
        status.Warnings.AddRange(untracked);
      }

      return Task.FromResult(status);
    }

    // -------------------------------------------------------------------------
    // Backup

    public async Task<FileStream> CreateBackupAsync(CancellationToken cancellationToken = default)
    {
      using var lease = _maintenance.TryEnter() ?? throw new BackupBusyException();

      var path = Path.Combine(_storage.StagingRootPath, $"download-{Guid.NewGuid():N}.zip");
      try
      {
        await CreateBackupFileAsync(path, cancellationToken);
        return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete, 81920,
          FileOptions.Asynchronous | FileOptions.DeleteOnClose);
      }
      catch
      {
        TryDeleteFile(path);
        throw;
      }
    }

    /// <summary>
    /// Writes a complete backup archive to <paramref name="destination"/>. The caller must hold the maintenance gate
    /// so that uploads can't change the files while they're being copied.
    /// </summary>
    private async Task<BackupManifest> CreateBackupFileAsync(string destination, CancellationToken cancellationToken)
    {
      var workDir = CreateWorkDir("backup");
      try
      {
        var manifest = new BackupManifest
        {
          FormatVersion = BackupManifest.CurrentFormatVersion,
          AppVersion = GetAppVersion(),
          CreatedAtUtc = DateTime.UtcNow,
          SourceMachine = Environment.MachineName,
          SourceEnvironment = _environmentName
        };

        // Copy through SQLite's backup API: copying the file itself could catch it mid-write (WAL)
        var dbPath = Path.Combine(workDir, BackupManifest.DatabaseEntryName);
        using (var live = OpenConnection(_liveConnectionString))
        using (var snapshot = OpenConnection(TempConnectionString(dbPath)))
        {
          live.BackupDatabase(snapshot);
        }

        List<StaticFileRow> rows;
        using (var snapshot = OpenConnection(TempConnectionString(dbPath)))
        {
          var integrity = RunIntegrityCheck(snapshot);
          if (integrity != null)
          {
            throw new BackupException($"The site's database failed its integrity check, so no backup was made: {integrity}");
          }

          manifest.Migrations = ReadMigrations(snapshot);
          rows = ReadStaticFileRows(snapshot);
        }

        await using (var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
        await using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        {
          manifest.Database = await AddFileToArchiveAsync(archive, BackupManifest.DatabaseEntryName, dbPath, cancellationToken);
          manifest.Database.Path = BackupManifest.DatabaseEntryName;

          foreach (var file in EnumerateStaticFiles())
          {
            var entry = await AddFileToArchiveAsync(archive, BackupManifest.StaticFolderPrefix + file.RelativePath, file.FullPath, cancellationToken);
            entry.Path = file.RelativePath;
            manifest.StaticFiles.Add(entry);
          }

          var files = manifest.StaticFiles.ToDictionary(f => f.Path, f => (f.SizeBytes, (string?)f.Sha256), StringComparer.Ordinal);
          var (problems, untracked) = CrossCheck(rows, files);
          manifest.Warnings.AddRange(problems);
          manifest.Warnings.AddRange(untracked);

          var manifestEntry = archive.CreateEntry(BackupManifest.FileName, CompressionLevel.Optimal);
          await using var manifestStream = manifestEntry.Open();
          await JsonSerializer.SerializeAsync(manifestStream, manifest, ManifestJsonOptions, cancellationToken);
        }

        _logger.LogInformation("Created backup {Destination}: {FileCount} static files, {WarningCount} warnings",
          destination, manifest.StaticFiles.Count, manifest.Warnings.Count);
        return manifest;
      }
      finally
      {
        TryDeleteDirectory(workDir);
      }
    }

    private static async Task<BackupManifestFile> AddFileToArchiveAsync(ZipArchive archive, string entryName, string sourcePath, CancellationToken cancellationToken)
    {
      var level = StoredExtensions.Contains(Path.GetExtension(entryName)) ? CompressionLevel.NoCompression : CompressionLevel.Optimal;
      var entry = archive.CreateEntry(entryName, level);
      entry.LastWriteTime = File.GetLastWriteTime(sourcePath);

      await using var source = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
      await using var target = entry.Open();
      var (size, sha256) = await SafeFileIO.CopyAndHashAsync(source, target, long.MaxValue, () => new InvalidOperationException(), cancellationToken);
      return new BackupManifestFile { Path = entryName, SizeBytes = size, Sha256 = sha256 };
    }

    // -------------------------------------------------------------------------
    // Restore: stage + verify

    public async Task<RestorePreviewDto> StageSnapshotAsync(string snapshotName, CancellationToken cancellationToken = default)
    {
      var path = GetSnapshotPath(snapshotName) ?? throw new BackupException($"The snapshot '{snapshotName}' does not exist.");
      await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
      return await StageRestoreAsync(stream, snapshotName, cancellationToken);
    }

    public async Task<RestorePreviewDto> StageRestoreAsync(Stream archiveStream, string fileName, CancellationToken cancellationToken = default)
    {
      if (_maintenance.IsBusy)
      {
        throw new BackupBusyException();
      }

      ReplaceStaged(null);

      var workDir = CreateWorkDir("restore");
      var preview = new RestorePreviewDto { FileName = Path.GetFileName(fileName) };
      try
      {
        await ExtractAndVerifyArchiveAsync(archiveStream, workDir, preview, cancellationToken);
        if (preview.CanRestore)
        {
          VerifyDatabase(workDir, preview);
        }
      }
      catch
      {
        TryDeleteDirectory(workDir);
        throw;
      }

      if (!preview.CanRestore)
      {
        _logger.LogWarning("Backup {FileName} cannot be restored: {Errors}", preview.FileName, string.Join(" | ", preview.Errors));
        TryDeleteDirectory(workDir);
        return preview;
      }

      preview.Token = Guid.NewGuid().ToString("N");
      ReplaceStaged(new StagedRestore(preview.Token, workDir, preview));

      _logger.LogInformation("Staged restore of {FileName} with token {Token}", preview.FileName, preview.Token);
      return preview;
    }

    /// <summary>
    /// Checks the archive's structure and hashes, extracting dscms.db and static/ into the work folder.
    /// Anything that means "this isn't an intact DSCMS backup" throws; nothing here is recoverable by the user.
    /// </summary>
    private async Task ExtractAndVerifyArchiveAsync(Stream archiveStream, string workDir, RestorePreviewDto preview, CancellationToken cancellationToken)
    {
      ZipArchive archive;
      try
      {
        archive = new ZipArchive(archiveStream, ZipArchiveMode.Read, leaveOpen: true);
      }
      catch (InvalidDataException ex)
      {
        throw new BackupException("The file is not a valid .zip archive.", ex);
      }

      await using (archive)
      {
        var entries = archive.Entries.Where(e => !e.FullName.EndsWith('/')).ToList();
        if (entries.Count > Settings.MaxArchiveEntries + 2)
        {
          throw new BackupException($"The archive contains {entries.Count} files; the maximum is {Settings.MaxArchiveEntries}.");
        }

        // Paths are restored exactly as named, so refuse anything that could escape the target folders,
        // or that would collide on a case-insensitive file system
        var byName = new Dictionary<string, ZipArchiveEntry>(StringComparer.Ordinal);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in entries)
        {
          var name = entry.FullName;
          if (name.Contains('\\') || name.Contains(':') || name.StartsWith('/') || !SafeFileIO.IsCleanRelativePath(name))
          {
            throw new BackupException($"The archive entry '{name}' has an invalid path.");
          }
          if (!seen.Add(name))
          {
            throw new BackupException($"The archive contains '{name}' more than once (names are compared ignoring case).");
          }
          byName[name] = entry;
        }

        if (!byName.TryGetValue(BackupManifest.FileName, out var manifestEntry))
        {
          throw new BackupException($"This is not a DSCMS backup: it has no {BackupManifest.FileName}.");
        }

        var manifest = await ReadManifestAsync(manifestEntry, cancellationToken);
        if (manifest.FormatVersion > BackupManifest.CurrentFormatVersion)
        {
          throw new BackupException($"This backup was created by a newer version of DSCMS (backup format {manifest.FormatVersion}; this site reads format {BackupManifest.CurrentFormatVersion}). Update this site first.");
        }
        if (manifest.FormatVersion != BackupManifest.CurrentFormatVersion)
        {
          throw new BackupException($"Backup format {manifest.FormatVersion} is not supported.");
        }

        preview.FormatVersion = manifest.FormatVersion;
        preview.AppVersion = manifest.AppVersion;
        preview.CreatedAtUtc = DateTime.SpecifyKind(manifest.CreatedAtUtc, DateTimeKind.Utc);
        preview.SourceMachine = manifest.SourceMachine;
        preview.SourceEnvironment = manifest.SourceEnvironment;

        if (!byName.TryGetValue(BackupManifest.DatabaseEntryName, out var dbEntry))
        {
          throw new BackupException($"The backup does not contain {BackupManifest.DatabaseEntryName}.");
        }

        // The archive must hold exactly what the manifest lists: nothing missing, nothing extra
        var archiveStatic = byName.Keys
          .Where(n => n != BackupManifest.FileName && n != BackupManifest.DatabaseEntryName)
          .ToList();
        var unexpected = archiveStatic.Where(n => !n.StartsWith(BackupManifest.StaticFolderPrefix, StringComparison.Ordinal)).ToList();
        if (unexpected.Count > 0)
        {
          throw new BackupException($"The backup contains unexpected entries: {Summarize(unexpected)}");
        }

        var listed = manifest.StaticFiles.ToDictionary(f => BackupManifest.StaticFolderPrefix + f.Path, StringComparer.Ordinal);
        var notListed = archiveStatic.Where(n => !listed.ContainsKey(n)).ToList();
        var notPresent = listed.Keys.Where(n => !byName.ContainsKey(n)).ToList();
        if (notListed.Count > 0 || notPresent.Count > 0)
        {
          throw new BackupException("The backup's contents don't match its manifest. "
            + (notPresent.Count > 0 ? $"Missing: {Summarize(notPresent)}. " : "")
            + (notListed.Count > 0 ? $"Not in manifest: {Summarize(notListed)}." : ""));
        }

        var remainingBytes = Settings.MaxExtractedBytes;

        var dbPath = Path.Combine(workDir, BackupManifest.DatabaseEntryName);
        remainingBytes -= await ExtractVerifiedAsync(dbEntry, dbPath, manifest.Database, remainingBytes, cancellationToken);

        // Restored byte-for-byte: a .zip that was uploaded as a file stays a .zip, it's never extracted here
        var staticRoot = Path.Combine(workDir, "static");
        Directory.CreateDirectory(staticRoot);
        foreach (var (entryName, expected) in listed)
        {
          var destination = SafeFileIO.ResolveInside(staticRoot, expected.Path)
            ?? throw new BackupException($"The archive entry '{entryName}' points outside the static files folder.");
          Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
          remainingBytes -= await ExtractVerifiedAsync(byName[entryName], destination, expected, remainingBytes, cancellationToken);
        }

        preview.Counts.StaticFiles = manifest.StaticFiles.Count;
        preview.Counts.StaticBytes = manifest.StaticFiles.Sum(f => f.SizeBytes);

        // Kept for the database cross-check
        File.WriteAllText(Path.Combine(workDir, BackupManifest.FileName), JsonSerializer.Serialize(manifest, ManifestJsonOptions));
      }
    }

    private static async Task<BackupManifest> ReadManifestAsync(ZipArchiveEntry entry, CancellationToken cancellationToken)
    {
      if (entry.Length > MaxManifestBytes)
      {
        throw new BackupException($"{BackupManifest.FileName} is too large.");
      }

      try
      {
        await using var stream = entry.Open();
        return await JsonSerializer.DeserializeAsync<BackupManifest>(stream, ManifestJsonOptions, cancellationToken)
          ?? throw new BackupException($"{BackupManifest.FileName} is empty.");
      }
      catch (JsonException ex)
      {
        throw new BackupException($"{BackupManifest.FileName} is not valid: {ex.Message}", ex);
      }
    }

    private async Task<long> ExtractVerifiedAsync(ZipArchiveEntry entry, string destination, BackupManifestFile expected, long remainingBytes, CancellationToken cancellationToken)
    {
      await using var source = entry.Open();
      var (size, sha256) = await SafeFileIO.WriteFileAsync(source, destination, remainingBytes,
        () => new BackupException("The backup's extracted contents exceed the maximum allowed size."), cancellationToken);

      if (size != expected.SizeBytes || !string.Equals(sha256, expected.Sha256, StringComparison.OrdinalIgnoreCase))
      {
        throw new BackupException($"'{entry.FullName}' doesn't match the checksum in the manifest. The backup is damaged or was modified.");
      }

      return size;
    }

    /// <summary>
    /// Checks the extracted database, upgrades it to this app's schema if needed, and cross-checks it against the static files.
    /// Problems are reported on the preview rather than thrown.
    /// </summary>
    private void VerifyDatabase(string workDir, RestorePreviewDto preview)
    {
      var dbPath = Path.Combine(workDir, BackupManifest.DatabaseEntryName);
      if (!HasSqliteHeader(dbPath))
      {
        preview.Errors.Add($"{BackupManifest.DatabaseEntryName} is not a SQLite database.");
        return;
      }

      List<string> backupMigrations;
      using (var connection = OpenConnection(TempConnectionString(dbPath)))
      {
        var integrity = RunIntegrityCheck(connection);
        if (integrity != null)
        {
          preview.Errors.Add($"The backup's database failed its integrity check: {integrity}");
          return;
        }

        var missingTables = RequiredTables.Where(t => !TableExists(connection, t)).ToList();
        if (missingTables.Count > 0)
        {
          preview.Errors.Add($"The backup's database is not a DSCMS database (missing tables: {string.Join(", ", missingTables)}).");
          return;
        }

        backupMigrations = ReadMigrations(connection);
      }

      // Migration IDs start with a timestamp, so unknown ones sorting after this app's latest come from newer code.
      // Older unknown ones are history from earlier incarnations of DSCMS (e.g. 2016's "initial") and are harmless.
      var appMigrations = GetAppMigrations();
      var latestAppMigration = appMigrations.LastOrDefault() ?? string.Empty;
      var unknown = backupMigrations
        .Except(appMigrations, StringComparer.Ordinal)
        .Where(m => string.CompareOrdinal(m, latestAppMigration) > 0)
        .ToList();
      if (unknown.Count > 0)
      {
        preview.Errors.Add("The backup's database is from a newer version of DSCMS than this site is running "
          + $"(unknown migrations: {Summarize(unknown)}). Deploy the newer code to this site first, then restore.");
        return;
      }

      preview.PendingMigrations = appMigrations.Except(backupMigrations, StringComparer.Ordinal).ToList();
      if (preview.PendingMigrations.Count > 0)
      {
        try
        {
          MigrateDatabase(dbPath);
        }
        catch (Exception ex)
        {
          _logger.LogWarning(ex, "Could not migrate staged backup database");
          preview.Errors.Add($"The backup's database could not be upgraded to this site's schema: {ex.Message}");
          return;
        }
      }

      var manifest = JsonSerializer.Deserialize<BackupManifest>(File.ReadAllText(Path.Combine(workDir, BackupManifest.FileName)), ManifestJsonOptions)!;
      var files = manifest.StaticFiles.ToDictionary(f => f.Path, f => (f.SizeBytes, (string?)f.Sha256), StringComparer.Ordinal);

      using (var connection = OpenConnection(TempConnectionString(dbPath)))
      {
        var counts = ReadCounts(connection);
        counts.StaticFiles = preview.Counts.StaticFiles;
        counts.StaticBytes = preview.Counts.StaticBytes;
        preview.Counts = counts;

        // Files the database points at must all be there; extra files on disk are only worth a mention
        var (problems, untracked) = CrossCheck(ReadStaticFileRows(connection), files);
        preview.Errors.AddRange(problems);
        preview.Warnings.AddRange(untracked);

        if (TableExists(connection, "LayoutValues"))
        {
          using var command = connection.CreateCommand();
          command.CommandText = "SELECT \"Key\", \"Value\" FROM \"LayoutValues\" WHERE \"Value\" LIKE '%localhost%' OR \"Value\" LIKE '%127.0.0.1%'";
          using var reader = command.ExecuteReader();
          while (reader.Read())
          {
            preview.Warnings.Add($"Layout value '{reader.GetString(0)}' points at a local address: {reader.GetString(1)}");
          }
        }
      }

      if (preview.Counts.Users > 0)
      {
        preview.Warnings.Add($"The backup contains {preview.Counts.Users} user account(s). They will not be restored: this site keeps its own users, "
          + "and content owned by accounts that don't exist here will be assigned to you.");
      }
    }

    // -------------------------------------------------------------------------
    // Restore: commit

    public void DiscardStagedRestore(string token)
    {
      lock (_stagedLock)
      {
        if (_staged?.Token == token)
        {
          TryDeleteDirectory(_staged.WorkDir);
          _staged = null;
        }
      }
    }

    public async Task<RestoreResultDto> CommitRestoreAsync(string token, string? restoringUserId, CancellationToken cancellationToken = default)
    {
      using var lease = _maintenance.TryEnter() ?? throw new BackupBusyException();

      StagedRestore staged;
      lock (_stagedLock)
      {
        if (_staged == null || _staged.Token != token)
        {
          throw new BackupException("This restore has expired or was replaced by another upload. Please upload the backup again.");
        }
        staged = _staged;
        _staged = null;
      }

      try
      {
        // Taken while the site is still serving, so the downtime is only the swap itself
        var snapshotName = await CreateSnapshotAsync(cancellationToken);

        var stagedDb = Path.Combine(staged.WorkDir, BackupManifest.DatabaseEntryName);
        try
        {
          KeepTargetUsers(stagedDb, restoringUserId);
        }
        catch (SqliteException ex)
        {
          throw new BackupException($"This site's user accounts could not be carried over, so nothing was changed: {ex.Message}", ex);
        }

        lease.EnterRestoringMode();
        _logger.LogWarning("Restoring site from {FileName} (pre-restore snapshot {SnapshotName})", staged.Preview.FileName, snapshotName);

        var previousStatic = SwapStaticFolder(Path.Combine(staged.WorkDir, "static"));
        try
        {
          using (var source = OpenConnection(TempConnectionString(stagedDb)))
          using (var live = OpenConnection(_liveConnectionString))
          {
            source.BackupDatabase(live);
          }
          SqliteConnection.ClearAllPools();
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Database restore failed; putting the previous static files back");
          UndoStaticSwap(previousStatic);
          throw new BackupException($"The database could not be restored, so nothing was changed: {ex.Message}", ex);
        }

        if (previousStatic != null)
        {
          TryDeleteDirectory(previousStatic);
        }

        RestoreResultDto result;
        using (var live = OpenConnection(_liveConnectionString))
        {
          result = new RestoreResultDto { SnapshotName = snapshotName, Counts = ReadCounts(live) };
        }

        _logger.LogWarning("Restore of {FileName} completed", staged.Preview.FileName);
        return result;
      }
      finally
      {
        TryDeleteDirectory(staged.WorkDir);
      }
    }

    /// <summary>
    /// Replaces the staged database's accounts with the live site's, and points content owned by
    /// accounts that no longer exist at the user doing the restore.
    /// </summary>
    private void KeepTargetUsers(string stagedDbPath, string? restoringUserId)
    {
      using var connection = OpenConnection(TempConnectionString(stagedDbPath));
      Execute(connection, "PRAGMA foreign_keys = OFF");

      using (var attach = connection.CreateCommand())
      {
        attach.CommandText = "ATTACH DATABASE @path AS live";
        attach.Parameters.AddWithValue("@path", GetLiveDatabasePath());
        attach.ExecuteNonQuery();
      }

      // Deferred, so only the staged database is locked for writing; the live one is just read
      using (var transaction = connection.BeginTransaction(deferred: true))
      {
        foreach (var table in IdentityTables)
        {
          Execute(connection, $"DELETE FROM main.\"{table}\"");
        }

        foreach (var table in IdentityTables.Reverse())
        {
          if (!TableExists(connection, table, "live"))
          {
            throw new BackupException($"This site's database has no {table} table, so its users can't be kept.");
          }

          var columns = string.Join(", ", GetColumns(connection, table).Select(c => $"\"{c}\""));
          Execute(connection, $"INSERT INTO main.\"{table}\" ({columns}) SELECT {columns} FROM live.\"{table}\"");
        }

        foreach (var (table, column) in UserReferences)
        {
          if (!TableExists(connection, table))
          {
            continue;
          }

          using var command = connection.CreateCommand();
          command.CommandText = $"UPDATE main.\"{table}\" SET \"{column}\" = @user "
            + $"WHERE \"{column}\" IS NOT NULL AND \"{column}\" NOT IN (SELECT \"Id\" FROM main.\"AspNetUsers\")";
          command.Parameters.AddWithValue("@user", (object?)restoringUserId ?? DBNull.Value);
          var updated = command.ExecuteNonQuery();
          if (updated > 0)
          {
            _logger.LogInformation("Reassigned {Count} {Table}.{Column} values to the restoring user", updated, table, column);
          }
        }

        transaction.Commit();
      }

      Execute(connection, "DETACH DATABASE live");

      using (var check = connection.CreateCommand())
      {
        check.CommandText = "PRAGMA foreign_key_check";
        using var reader = check.ExecuteReader();
        if (reader.Read())
        {
          throw new BackupException($"The backup's database has broken references (table {reader.GetString(0)}), so it was not restored.");
        }
      }
    }

    /// <summary>
    /// Moves the live static folder aside and the staged one into its place. Returns where the old folder went.
    /// Both live beside each other on the same volume, so these are renames, not copies.
    /// </summary>
    private string? SwapStaticFolder(string stagedStatic)
    {
      Directory.CreateDirectory(stagedStatic);

      string? previous = null;
      if (Directory.Exists(_storage.RootPath))
      {
        previous = Path.Combine(_storage.StagingRootPath, Guid.NewGuid().ToString("N") + "-previous");
        Directory.Move(_storage.RootPath, previous);
      }

      try
      {
        Directory.Move(stagedStatic, _storage.RootPath);
      }
      catch
      {
        if (previous != null)
        {
          Directory.Move(previous, _storage.RootPath);
        }
        throw;
      }

      return previous;
    }

    private void UndoStaticSwap(string? previous)
    {
      if (previous == null)
      {
        return;
      }

      TryDeleteDirectory(_storage.RootPath);
      Directory.Move(previous, _storage.RootPath);
    }

    // -------------------------------------------------------------------------
    // Snapshots

    public IReadOnlyList<BackupSnapshotDto> ListSnapshots()
    {
      if (!Directory.Exists(_snapshotRoot))
      {
        return Array.Empty<BackupSnapshotDto>();
      }

      return new DirectoryInfo(_snapshotRoot)
        .EnumerateFiles("*.zip")
        .Where(f => SnapshotNamePattern.IsMatch(f.Name))
        .OrderByDescending(f => f.LastWriteTimeUtc)
        .Select(f => new BackupSnapshotDto { Name = f.Name, SizeBytes = f.Length, CreatedAtUtc = f.LastWriteTimeUtc })
        .ToList();
    }

    public string? GetSnapshotPath(string snapshotName)
    {
      if (string.IsNullOrEmpty(snapshotName) || !SnapshotNamePattern.IsMatch(snapshotName))
      {
        return null;
      }

      var path = Path.Combine(_snapshotRoot, snapshotName);
      return File.Exists(path) ? path : null;
    }

    private async Task<string> CreateSnapshotAsync(CancellationToken cancellationToken)
    {
      Directory.CreateDirectory(_snapshotRoot);

      var baseName = $"{SnapshotPrefix}{DateTime.UtcNow:yyyyMMdd-HHmmss}";
      var name = baseName + ".zip";
      for (var i = 1; File.Exists(Path.Combine(_snapshotRoot, name)); i++)
      {
        name = $"{baseName}-{i}.zip";
      }

      var path = Path.Combine(_snapshotRoot, name);
      try
      {
        await CreateBackupFileAsync(path, cancellationToken);
      }
      catch
      {
        TryDeleteFile(path);
        throw;
      }

      // Only automatic snapshots are pruned; anything else dropped in the folder is left alone
      var keep = Math.Max(1, Settings.SnapshotsToKeep);
      foreach (var old in ListSnapshots().Where(s => s.Name.StartsWith(SnapshotPrefix, StringComparison.Ordinal)).Skip(keep))
      {
        TryDeleteFile(Path.Combine(_snapshotRoot, old.Name));
      }

      return name;
    }

    // -------------------------------------------------------------------------
    // Helpers

    private void ReplaceStaged(StagedRestore? staged)
    {
      lock (_stagedLock)
      {
        if (_staged != null)
        {
          TryDeleteDirectory(_staged.WorkDir);
        }
        _staged = staged;
      }
    }

    private string CreateWorkDir(string purpose)
    {
      var path = Path.Combine(_storage.StagingRootPath, $"{purpose}-{Guid.NewGuid():N}");
      Directory.CreateDirectory(path);
      return path;
    }

    private IEnumerable<(string RelativePath, string FullPath, long SizeBytes)> EnumerateStaticFiles()
    {
      if (!Directory.Exists(_storage.RootPath))
      {
        return Enumerable.Empty<(string, string, long)>();
      }

      return new DirectoryInfo(_storage.RootPath)
        .EnumerateFiles("*", SearchOption.AllDirectories)
        .Select(f => (Path.GetRelativePath(_storage.RootPath, f.FullName).Replace(Path.DirectorySeparatorChar, '/'), f.FullName, f.Length))
        .OrderBy(f => f.Item1, StringComparer.Ordinal)
        .ToList();
    }

    /// <summary>
    /// Compares the database's StaticUploadFiles rows with a set of files (relative path -> size and, if known, hash).
    /// Returns files the database expects but are missing or different, and files nothing in the database refers to.
    /// </summary>
    internal static (List<string> Problems, List<string> Untracked) CrossCheck(
      IReadOnlyCollection<StaticFileRow> rows, IReadOnlyDictionary<string, (long SizeBytes, string? Sha256)> files)
    {
      var missing = new List<string>();
      var changed = new List<string>();
      foreach (var row in rows)
      {
        if (!files.TryGetValue(row.Path, out var file))
        {
          missing.Add(row.Path);
        }
        else if (file.SizeBytes != row.SizeBytes || (file.Sha256 != null && !string.Equals(file.Sha256, row.Sha256, StringComparison.OrdinalIgnoreCase)))
        {
          changed.Add(row.Path);
        }
      }

      var problems = new List<string>();
      if (missing.Count > 0)
      {
        problems.Add($"{missing.Count} file(s) recorded in the database are missing: {Summarize(missing)}");
      }
      if (changed.Count > 0)
      {
        problems.Add($"{changed.Count} file(s) differ from what the database recorded: {Summarize(changed)}");
      }

      var tracked = new HashSet<string>(rows.Select(r => r.Path), StringComparer.Ordinal);
      var untracked = files.Keys
        .Where(p => !tracked.Contains(p))
        .GroupBy(p => p.Contains('/') ? p.Substring(0, p.IndexOf('/') + 1) : p, StringComparer.Ordinal)
        .OrderBy(g => g.Key, StringComparer.Ordinal)
        .Select(g => $"{g.Count()} file(s) in '{g.Key}' are not part of any static upload (they are still backed up and restored).")
        .ToList();

      return (problems, untracked);
    }

    private static string Summarize(IReadOnlyCollection<string> items)
    {
      var listed = string.Join(", ", items.Take(MaxListedProblems));
      return items.Count > MaxListedProblems ? $"{listed}, and {items.Count - MaxListedProblems} more" : listed;
    }

    private static SqliteConnection OpenConnection(string connectionString)
    {
      var connection = new SqliteConnection(connectionString);
      connection.Open();
      return connection;
    }

    // Temp databases are deleted afterwards, so they must not linger in the connection pool holding the file open
    private static string TempConnectionString(string path)
    {
      return new SqliteConnectionStringBuilder { DataSource = path, Pooling = false }.ToString();
    }

    private string GetLiveDatabasePath()
    {
      return Path.GetFullPath(new SqliteConnectionStringBuilder(_liveConnectionString).DataSource);
    }

    private DbContextOptions<ApplicationDbContext> ContextOptions(string connectionString)
    {
      return new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connectionString).Options;
    }

    private List<string> GetAppMigrations()
    {
      using var context = new ApplicationDbContext(ContextOptions(_liveConnectionString));
      return context.Database.GetMigrations().ToList();
    }

    private void MigrateDatabase(string path)
    {
      using var context = new ApplicationDbContext(ContextOptions(TempConnectionString(path)));
      context.Database.Migrate();
    }

    private static bool HasSqliteHeader(string path)
    {
      var expected = Encoding.ASCII.GetBytes("SQLite format 3\0");
      var header = new byte[expected.Length];
      using var stream = File.OpenRead(path);
      return stream.Read(header, 0, header.Length) == header.Length && header.SequenceEqual(expected);
    }

    /// <summary>
    /// Returns null if the database is intact, otherwise SQLite's description of the problem.
    /// </summary>
    private static string? RunIntegrityCheck(SqliteConnection connection)
    {
      using var command = connection.CreateCommand();
      command.CommandText = "PRAGMA integrity_check";
      var result = command.ExecuteScalar() as string;
      return result == "ok" ? null : result ?? "unknown error";
    }

    private static bool TableExists(SqliteConnection connection, string table, string schema = "main")
    {
      using var command = connection.CreateCommand();
      command.CommandText = $"SELECT COUNT(*) FROM \"{schema}\".sqlite_master WHERE type = 'table' AND name = @name";
      command.Parameters.AddWithValue("@name", table);
      return Convert.ToInt64(command.ExecuteScalar()) > 0;
    }

    private static List<string> GetColumns(SqliteConnection connection, string table)
    {
      using var command = connection.CreateCommand();
      command.CommandText = $"SELECT name FROM pragma_table_info('{table}', 'main')";
      using var reader = command.ExecuteReader();
      var columns = new List<string>();
      while (reader.Read())
      {
        columns.Add(reader.GetString(0));
      }
      return columns;
    }

    private static void Execute(SqliteConnection connection, string sql)
    {
      using var command = connection.CreateCommand();
      command.CommandText = sql;
      command.ExecuteNonQuery();
    }

    private static int Count(SqliteConnection connection, string table)
    {
      if (!TableExists(connection, table))
      {
        return 0;
      }

      using var command = connection.CreateCommand();
      command.CommandText = $"SELECT COUNT(*) FROM \"{table}\"";
      return Convert.ToInt32(command.ExecuteScalar());
    }

    private static SiteCountsDto ReadCounts(SqliteConnection connection)
    {
      return new SiteCountsDto
      {
        ContentTypes = Count(connection, "ContentTypes"),
        Contents = Count(connection, "Contents"),
        Layouts = Count(connection, "Layouts"),
        Templates = Count(connection, "Templates"),
        StaticUploads = Count(connection, "StaticUploads"),
        Users = Count(connection, "AspNetUsers")
      };
    }

    private static List<string> ReadMigrations(SqliteConnection connection)
    {
      var migrations = new List<string>();
      if (!TableExists(connection, "__EFMigrationsHistory"))
      {
        return migrations;
      }

      using var command = connection.CreateCommand();
      command.CommandText = "SELECT \"MigrationId\" FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\"";
      using var reader = command.ExecuteReader();
      while (reader.Read())
      {
        migrations.Add(reader.GetString(0));
      }
      return migrations;
    }

    private static List<StaticFileRow> ReadStaticFileRows(SqliteConnection connection)
    {
      var rows = new List<StaticFileRow>();
      if (!TableExists(connection, "StaticUploads") || !TableExists(connection, "StaticUploadFiles"))
      {
        return rows;
      }

      using var command = connection.CreateCommand();
      command.CommandText = "SELECT u.\"FolderPath\", f.\"RelativePath\", f.\"SizeBytes\", f.\"Sha256\" "
        + "FROM \"StaticUploadFiles\" f JOIN \"StaticUploads\" u ON u.\"StaticUploadId\" = f.\"StaticUploadId\"";
      using var reader = command.ExecuteReader();
      while (reader.Read())
      {
        rows.Add(new StaticFileRow($"{reader.GetString(0)}/{reader.GetString(1)}", reader.GetInt64(2), reader.GetString(3)));
      }
      return rows;
    }

    private static string GetAppVersion()
    {
      return typeof(BackupService).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";
    }

    private void TryDeleteDirectory(string path)
    {
      try
      {
        if (Directory.Exists(path))
        {
          Directory.Delete(path, recursive: true);
        }
      }
      catch (Exception ex)
      {
        _logger.LogWarning(ex, "Could not delete temporary folder {Path}", path);
      }
    }

    private void TryDeleteFile(string path)
    {
      try
      {
        File.Delete(path);
      }
      catch (Exception ex)
      {
        _logger.LogWarning(ex, "Could not delete file {Path}", path);
      }
    }

    internal record StaticFileRow(string Path, long SizeBytes, string Sha256);

    private record StagedRestore(string Token, string WorkDir, RestorePreviewDto Preview);
  }
}
