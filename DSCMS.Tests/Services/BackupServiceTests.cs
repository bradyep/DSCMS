using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DSCMS.Data;
using DSCMS.Models;
using DSCMS.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DSCMS.Tests.Services
{
  /// <summary>
  /// Exercises real SQLite databases and a real static folder in a throwaway temp folder.
  /// </summary>
  public class BackupServiceTests : IDisposable
  {
    private const string LiveUserId = "live-admin";

    private readonly string _tempRoot;
    private readonly string _liveConnectionString;
    private readonly StaticFileStorageService _storage;
    private readonly MaintenanceState _maintenance = new();
    private readonly BackupSettings _settings;
    private readonly BackupService _service;

    public BackupServiceTests()
    {
      _tempRoot = Path.Combine(Path.GetTempPath(), "dscms-backup-tests-" + Guid.NewGuid().ToString("N"));
      Directory.CreateDirectory(_tempRoot);

      _liveConnectionString = new SqliteConnectionStringBuilder { DataSource = Path.Combine(_tempRoot, "dscms.db"), Pooling = false }.ToString();
      using (var context = CreateContext())
      {
        context.Database.Migrate();
      }

      _storage = new StaticFileStorageService(
        new StaticFileSettings { StoragePath = "static" }, _tempRoot, NullLogger<StaticFileStorageService>.Instance);
      _settings = new BackupSettings { StoragePath = "backups", SnapshotsToKeep = 2 };
      _service = new BackupService(_settings, _tempRoot, _liveConnectionString, "Testing", _storage, _maintenance, NullLogger<BackupService>.Instance);

      SeedSite();
    }

    public void Dispose()
    {
      SqliteConnection.ClearAllPools();
      if (Directory.Exists(_tempRoot))
      {
        Directory.Delete(_tempRoot, recursive: true);
      }
    }

    // -------------------------------------------------------------------------
    // Helpers

    private ApplicationDbContext CreateContext()
    {
      return new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_liveConnectionString).Options);
    }

    private void SeedSite()
    {
      using var context = CreateContext();
      context.Users.Add(new ApplicationUser { Id = LiveUserId, UserName = "admin", NormalizedUserName = "ADMIN" });
      var template = new Template { Name = "list", SourceTypeId = 1 };
      var contentType = new ContentType { Name = "blog", Title = "Blog", Description = "", ItemsPerPage = 5, MultipleContentsTemplate = template };
      context.ContentTypes.Add(contentType);
      context.Contents.Add(new Content { UrlToDisplay = "first-post", Title = "First", BodySourceTypeId = 4, ContentType = contentType, Template = template, CreatedBy = LiveUserId });
      context.SaveChanges();
    }

    private static byte[] MakeZipBytes(params (string Name, string Content)[] entries)
    {
      using var stream = new MemoryStream();
      using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
      {
        foreach (var (name, content) in entries)
        {
          using var writer = new StreamWriter(archive.CreateEntry(name).Open());
          writer.Write(content);
        }
      }
      return stream.ToArray();
    }

    private static string Sha256(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    /// <summary>
    /// Writes files into the static folder and records them as one upload, like StaticUploadsController does.
    /// </summary>
    private void AddUpload(string folderPath, params (string RelativePath, byte[] Bytes)[] files)
    {
      foreach (var (relativePath, bytes) in files)
      {
        var path = Path.Combine(_storage.RootPath, folderPath, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, bytes);
      }

      using var context = CreateContext();
      context.Set<StaticUpload>().Add(new StaticUpload
      {
        FolderPath = folderPath,
        OriginalFileName = folderPath + ".zip",
        IsArchive = true,
        FileCount = files.Length,
        TotalSizeBytes = files.Sum(f => f.Bytes.Length),
        UploadedAt = DateTime.UtcNow,
        UploadedBy = LiveUserId,
        Files = files.Select(f => new StaticUploadFile { RelativePath = f.RelativePath, SizeBytes = f.Bytes.Length, Sha256 = Sha256(f.Bytes) }).ToList()
      });
      context.SaveChanges();
    }

    private async Task<byte[]> CreateBackupBytesAsync()
    {
      await using var backup = await _service.CreateBackupAsync();
      using var copy = new MemoryStream();
      await backup.CopyToAsync(copy);
      return copy.ToArray();
    }

    /// <summary>
    /// Copies a backup archive, letting the caller replace (return new bytes) or drop (return null) entries, or add new ones.
    /// </summary>
    private static byte[] RewriteZip(byte[] source, Func<string, byte[], byte[]?> transform, params (string Name, byte[] Bytes)[] additions)
    {
      using var output = new MemoryStream();
      using (var input = new ZipArchive(new MemoryStream(source), ZipArchiveMode.Read))
      using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
      {
        foreach (var entry in input.Entries)
        {
          using var data = new MemoryStream();
          using (var entryStream = entry.Open())
          {
            entryStream.CopyTo(data);
          }

          var bytes = transform(entry.FullName, data.ToArray());
          if (bytes != null)
          {
            using var target = archive.CreateEntry(entry.FullName).Open();
            target.Write(bytes);
          }
        }

        foreach (var (name, bytes) in additions)
        {
          using var target = archive.CreateEntry(name).Open();
          target.Write(bytes);
        }
      }
      return output.ToArray();
    }

    private static byte[] RewriteManifest(byte[] backup, Action<BackupManifest> change)
    {
      var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
      return RewriteZip(backup, (name, bytes) =>
      {
        if (name != BackupManifest.FileName)
        {
          return bytes;
        }
        var manifest = JsonSerializer.Deserialize<BackupManifest>(bytes, options)!;
        change(manifest);
        return JsonSerializer.SerializeToUtf8Bytes(manifest, options);
      });
    }

    private Task<Models.DTOs.RestorePreviewDto> StageAsync(byte[] backup) => _service.StageRestoreAsync(new MemoryStream(backup), "backup.zip");

    private void ExecuteOnLive(string sql)
    {
      using var connection = new SqliteConnection(_liveConnectionString);
      connection.Open();
      using var command = connection.CreateCommand();
      command.CommandText = sql;
      command.ExecuteNonQuery();
    }

    private List<string> ContentUrls()
    {
      using var context = CreateContext();
      return context.Contents.Select(c => c.UrlToDisplay).OrderBy(u => u).ToList();
    }

    // -------------------------------------------------------------------------
    // Backup

    [Fact]
    public async Task CreateBackup_ContainsManifestDatabaseAndStaticFiles()
    {
      AddUpload("games/foo", ("index.html", Encoding.UTF8.GetBytes("<html></html>")));

      var backup = await CreateBackupBytesAsync();

      using var archive = new ZipArchive(new MemoryStream(backup), ZipArchiveMode.Read);
      var names = archive.Entries.Select(e => e.FullName).OrderBy(n => n).ToList();
      Assert.Equal(new[] { "dscms.db", "manifest.json", "static/games/foo/index.html" }, names);

      using var manifestStream = archive.GetEntry(BackupManifest.FileName)!.Open();
      var manifest = JsonSerializer.Deserialize<BackupManifest>(manifestStream, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
      Assert.Equal(BackupManifest.CurrentFormatVersion, manifest.FormatVersion);
      Assert.Equal("Testing", manifest.SourceEnvironment);
      Assert.NotEmpty(manifest.Migrations);
      Assert.Equal(64, manifest.Database.Sha256.Length);
      var file = Assert.Single(manifest.StaticFiles);
      Assert.Equal("games/foo/index.html", file.Path);
      Assert.Empty(manifest.Warnings);
    }

    [Fact]
    public async Task CreateBackup_WhileBusy_Throws()
    {
      using var lease = _maintenance.TryEnter();

      await Assert.ThrowsAsync<BackupBusyException>(() => _service.CreateBackupAsync());
    }

    [Fact]
    public async Task CreateBackup_DeletesTemporaryFileWhenClosed()
    {
      await CreateBackupBytesAsync();

      Assert.Empty(Directory.GetFiles(_storage.StagingRootPath));
      Assert.Empty(Directory.GetDirectories(_storage.StagingRootPath));
    }

    [Fact]
    public async Task GetStatus_ReportsCountsAndUntrackedFiles()
    {
      AddUpload("resume", ("resume.pdf", new byte[] { 1, 2, 3 }));
      Directory.CreateDirectory(Path.Combine(_storage.RootPath, "ruffle"));
      File.WriteAllText(Path.Combine(_storage.RootPath, "ruffle", "ruffle.js"), "x");

      var status = await _service.GetStatusAsync();

      Assert.Equal(1, status.Counts.Contents);
      Assert.Equal(1, status.Counts.StaticUploads);
      Assert.Equal(2, status.Counts.StaticFiles);
      Assert.Contains(status.Warnings, w => w.Contains("'ruffle/'"));
    }

    // -------------------------------------------------------------------------
    // Round trip

    [Fact]
    public async Task Restore_RoundTrip_PutsDatabaseAndFilesBack()
    {
      // A .zip uploaded to be served as-is must come back byte-for-byte, not extracted
      var nestedZip = MakeZipBytes(("inner.txt", "inside"));
      AddUpload("games/foo", ("index.html", Encoding.UTF8.GetBytes("<html></html>")), ("bundle.zip", nestedZip));
      var backup = await CreateBackupBytesAsync();

      // Change the site after the backup
      ExecuteOnLive("UPDATE \"Contents\" SET \"UrlToDisplay\" = 'changed-post'");
      Directory.Delete(Path.Combine(_storage.RootPath, "games"), recursive: true);
      File.WriteAllText(Path.Combine(_storage.RootPath, "added-later.txt"), "new");

      var preview = await StageAsync(backup);
      Assert.True(preview.CanRestore, string.Join(" | ", preview.Errors));
      Assert.NotNull(preview.Token);
      Assert.Equal(1, preview.Counts.Contents);
      Assert.Empty(preview.PendingMigrations);

      var result = await _service.CommitRestoreAsync(preview.Token!, LiveUserId);

      Assert.Equal(new[] { "first-post" }, ContentUrls());
      Assert.Equal(nestedZip, File.ReadAllBytes(Path.Combine(_storage.RootPath, "games", "foo", "bundle.zip")));
      Assert.False(File.Exists(Path.Combine(_storage.RootPath, "games", "foo", "inner.txt")));
      Assert.False(File.Exists(Path.Combine(_storage.RootPath, "added-later.txt")));

      // The pre-restore snapshot holds the changed site
      Assert.StartsWith("pre-restore-", result.SnapshotName);
      Assert.NotNull(_service.GetSnapshotPath(result.SnapshotName));
      Assert.Empty(Directory.GetDirectories(_storage.StagingRootPath));
    }

    [Fact]
    public async Task Restore_FromSnapshot_UndoesRestore()
    {
      var backup = await CreateBackupBytesAsync();
      ExecuteOnLive("UPDATE \"Contents\" SET \"UrlToDisplay\" = 'changed-post'");

      var preview = await StageAsync(backup);
      var result = await _service.CommitRestoreAsync(preview.Token!, LiveUserId);
      Assert.Equal(new[] { "first-post" }, ContentUrls());

      var undo = await _service.StageSnapshotAsync(result.SnapshotName);
      Assert.True(undo.CanRestore, string.Join(" | ", undo.Errors));
      await _service.CommitRestoreAsync(undo.Token!, LiveUserId);

      Assert.Equal(new[] { "changed-post" }, ContentUrls());
    }

    [Fact]
    public async Task Restore_PrunesOldSnapshots()
    {
      var backup = await CreateBackupBytesAsync();

      for (var i = 0; i < 3; i++)
      {
        var preview = await StageAsync(backup);
        await _service.CommitRestoreAsync(preview.Token!, LiveUserId);
      }

      Assert.Equal(_settings.SnapshotsToKeep, _service.ListSnapshots().Count);
    }

    [Fact]
    public async Task Restore_KeepsTargetUsersAndReassignsOrphanedContent()
    {
      // The backup comes from a site whose only user is "dev-user"
      ExecuteOnLive("INSERT INTO \"AspNetUsers\" (\"Id\", \"UserName\", \"AccessFailedCount\", \"EmailConfirmed\", \"LockoutEnabled\", \"PhoneNumberConfirmed\", \"TwoFactorEnabled\") "
        + "VALUES ('dev-user', 'dev', 0, 0, 0, 0, 0)");
      ExecuteOnLive("UPDATE \"Contents\" SET \"CreatedBy\" = 'dev-user', \"LastUpdatedBy\" = 'dev-user'");
      ExecuteOnLive($"DELETE FROM \"AspNetUsers\" WHERE \"Id\" = '{LiveUserId}'");
      var backup = await CreateBackupBytesAsync();

      // The target site has a different user
      ExecuteOnLive("UPDATE \"Contents\" SET \"CreatedBy\" = NULL, \"LastUpdatedBy\" = NULL");
      ExecuteOnLive("DELETE FROM \"AspNetUsers\"");
      ExecuteOnLive("INSERT INTO \"AspNetUsers\" (\"Id\", \"UserName\", \"AccessFailedCount\", \"EmailConfirmed\", \"LockoutEnabled\", \"PhoneNumberConfirmed\", \"TwoFactorEnabled\") "
        + "VALUES ('prod-user', 'prod', 0, 0, 0, 0, 0)");

      var preview = await StageAsync(backup);
      Assert.Contains(preview.Warnings, w => w.Contains("user account"));
      await _service.CommitRestoreAsync(preview.Token!, "prod-user");

      using var context = CreateContext();
      Assert.Equal(new[] { "prod-user" }, context.Users.Select(u => u.Id).ToList());
      var content = Assert.Single(context.Contents.ToList());
      Assert.Equal("prod-user", content.CreatedBy);
      Assert.Equal("prod-user", content.LastUpdatedBy);
    }

    [Fact]
    public async Task Restore_WhenDatabaseCopyFails_PutsStaticFilesBack()
    {
      AddUpload("site", ("index.html", Encoding.UTF8.GetBytes("before")));
      var backup = await CreateBackupBytesAsync();
      File.WriteAllText(Path.Combine(_storage.RootPath, "site", "index.html"), "after");
      ExecuteOnLive("UPDATE \"StaticUploadFiles\" SET \"SizeBytes\" = 5, \"Sha256\" = 'x'");

      // Another connection holding the write lock lets the snapshot read the live database (WAL)
      // but makes the copy into it fail. A short timeout keeps the test from waiting out SQLite's default.
      var service = new BackupService(_settings, _tempRoot, _liveConnectionString + ";Default Timeout=1", "Testing",
        _storage, _maintenance, NullLogger<BackupService>.Instance);
      var preview = await service.StageRestoreAsync(new MemoryStream(backup), "backup.zip");
      Assert.True(preview.CanRestore, string.Join(" | ", preview.Errors));

      using var blocker = new SqliteConnection(_liveConnectionString);
      blocker.Open();
      using (var begin = blocker.CreateCommand())
      {
        begin.CommandText = "BEGIN IMMEDIATE";
        begin.ExecuteNonQuery();
      }

      var ex = await Assert.ThrowsAsync<BackupException>(() => service.CommitRestoreAsync(preview.Token!, LiveUserId));

      Assert.Contains("database could not be restored", ex.Message);
      Assert.Equal("after", File.ReadAllText(Path.Combine(_storage.RootPath, "site", "index.html")));
      Assert.False(_maintenance.IsBusy);
    }

    // -------------------------------------------------------------------------
    // Verification

    [Fact]
    public async Task Stage_RejectsFileThatIsNotAZip()
    {
      var ex = await Assert.ThrowsAsync<BackupException>(() => StageAsync(Encoding.UTF8.GetBytes("not a zip")));
      Assert.Contains("not a valid .zip", ex.Message);
    }

    [Fact]
    public async Task Stage_RejectsArchiveWithoutManifest()
    {
      var backup = RewriteZip(await CreateBackupBytesAsync(), (name, bytes) => name == BackupManifest.FileName ? null : bytes);

      var ex = await Assert.ThrowsAsync<BackupException>(() => StageAsync(backup));
      Assert.Contains("manifest.json", ex.Message);
    }

    [Fact]
    public async Task Stage_RejectsArchiveWithoutDatabase()
    {
      var backup = RewriteZip(await CreateBackupBytesAsync(), (name, bytes) => name == BackupManifest.DatabaseEntryName ? null : bytes);

      var ex = await Assert.ThrowsAsync<BackupException>(() => StageAsync(backup));
      Assert.Contains("dscms.db", ex.Message);
    }

    [Fact]
    public async Task Stage_RejectsNewerFormatVersion()
    {
      var backup = RewriteManifest(await CreateBackupBytesAsync(), m => m.FormatVersion = BackupManifest.CurrentFormatVersion + 1);

      var ex = await Assert.ThrowsAsync<BackupException>(() => StageAsync(backup));
      Assert.Contains("newer version", ex.Message);
    }

    [Fact]
    public async Task Stage_RejectsTamperedFile()
    {
      AddUpload("site", ("index.html", Encoding.UTF8.GetBytes("original")));
      var backup = RewriteZip(await CreateBackupBytesAsync(),
        (name, bytes) => name == "static/site/index.html" ? Encoding.UTF8.GetBytes("tampered") : bytes);

      var ex = await Assert.ThrowsAsync<BackupException>(() => StageAsync(backup));
      Assert.Contains("checksum", ex.Message);
    }

    [Theory]
    [InlineData("static/../evil.txt")]
    [InlineData("../evil.txt")]
    [InlineData("/etc/evil.txt")]
    [InlineData("static\\evil.txt")]
    public async Task Stage_RejectsUnsafeEntryPaths(string entryName)
    {
      var backup = RewriteZip(await CreateBackupBytesAsync(), (name, bytes) => bytes, (entryName, new byte[] { 1 }));

      var ex = await Assert.ThrowsAsync<BackupException>(() => StageAsync(backup));
      Assert.Contains("invalid path", ex.Message);
    }

    [Fact]
    public async Task Stage_RejectsEntriesNotInManifest()
    {
      var backup = RewriteZip(await CreateBackupBytesAsync(), (name, bytes) => bytes, ("static/sneaky.js", new byte[] { 1 }));

      var ex = await Assert.ThrowsAsync<BackupException>(() => StageAsync(backup));
      Assert.Contains("don't match its manifest", ex.Message);
    }

    [Fact]
    public async Task Stage_BlocksWhenDatabaseReferencesMissingFile()
    {
      AddUpload("site", ("index.html", Encoding.UTF8.GetBytes("hi")), ("app.js", Encoding.UTF8.GetBytes("js")));
      File.Delete(Path.Combine(_storage.RootPath, "site", "app.js"));

      var backup = await CreateBackupBytesAsync();
      var preview = await StageAsync(backup);

      Assert.False(preview.CanRestore);
      Assert.Null(preview.Token);
      Assert.Contains(preview.Errors, e => e.Contains("missing") && e.Contains("site/app.js"));
    }

    [Fact]
    public async Task Stage_WarnsAboutUntrackedFiles()
    {
      AddUpload("site", ("index.html", Encoding.UTF8.GetBytes("hi")));
      File.WriteAllText(Path.Combine(_storage.RootPath, "stray.txt"), "stray");

      var preview = await StageAsync(await CreateBackupBytesAsync());

      Assert.True(preview.CanRestore, string.Join(" | ", preview.Errors));
      Assert.Contains(preview.Warnings, w => w.Contains("stray.txt"));
    }

    [Fact]
    public async Task Stage_RefusesBackupFromNewerSchema()
    {
      ExecuteOnLive("INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('99991231000000_FromTheFuture', '10.0.0')");
      var backup = await CreateBackupBytesAsync();

      var preview = await StageAsync(backup);

      Assert.False(preview.CanRestore);
      Assert.Contains(preview.Errors, e => e.Contains("99991231000000_FromTheFuture"));
    }

    [Fact]
    public async Task Stage_IgnoresLegacyMigrationsOlderThanTheApp()
    {
      // Real DSCMS databases still carry history rows from the original 2016 app
      ExecuteOnLive("INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('00000000000000_CreateIdentitySchema', '1.0.0')");
      ExecuteOnLive("INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('20160730002021_initial', '1.0.0')");

      var preview = await StageAsync(await CreateBackupBytesAsync());

      Assert.True(preview.CanRestore, string.Join(" | ", preview.Errors));
      Assert.Empty(preview.PendingMigrations);
    }

    [Fact]
    public async Task Stage_WarnsAboutLocalhostLayoutValues()
    {
      using (var context = CreateContext())
      {
        var layout = new Layout { Name = "main", SourceTypeId = 1 };
        layout.LayoutValues.Add(new LayoutValue { Key = "siteUrl", Value = "http://localhost:5000" });
        context.Layouts.Add(layout);
        context.SaveChanges();
      }

      var preview = await StageAsync(await CreateBackupBytesAsync());

      Assert.Contains(preview.Warnings, w => w.Contains("siteUrl"));
    }

    [Fact]
    public async Task Commit_WithUnknownToken_Throws()
    {
      await Assert.ThrowsAsync<BackupException>(() => _service.CommitRestoreAsync("nope", LiveUserId));
    }

    [Fact]
    public async Task Stage_ReplacesPreviouslyStagedRestore()
    {
      var backup = await CreateBackupBytesAsync();
      var first = await StageAsync(backup);
      var second = await StageAsync(backup);

      await Assert.ThrowsAsync<BackupException>(() => _service.CommitRestoreAsync(first.Token!, LiveUserId));
      await _service.CommitRestoreAsync(second.Token!, LiveUserId);
    }

    [Theory]
    [InlineData("../dscms.db")]
    [InlineData("..")]
    [InlineData("sub/dir.zip")]
    [InlineData("missing.zip")]
    public void GetSnapshotPath_RejectsInvalidOrUnknownNames(string name)
    {
      Assert.Null(_service.GetSnapshotPath(name));
    }
  }
}
