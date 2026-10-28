using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;

namespace DSCMS.Services
{
  public class StaticFileStorageService : IStaticFileStorageService
  {
    /// <summary>
    /// URL prefix uploads are served under. Also reserved as a ContentType name.
    /// </summary>
    public const string RequestPath = "/static";

    private const int MaxFolderDepth = 5;
    private const int MaxFolderPathLength = 200;
    private const int CopyBufferSize = 81920;

    private static readonly Regex InvalidFolderChars = new("[^a-z0-9._-]+", RegexOptions.Compiled);
    private static readonly Regex InvalidFileNameChars = new("[^A-Za-z0-9._-]+", RegexOptions.Compiled);
    private static readonly Regex RepeatedDashes = new("-{2,}", RegexOptions.Compiled);

    private readonly string _stagingRoot;
    private readonly ILogger<StaticFileStorageService> _logger;

    public string RootPath { get; }
    public StaticFileSettings Settings { get; }
    public FileExtensionContentTypeProvider ContentTypeProvider { get; }

    public StaticFileStorageService(IConfigurationService configurationService, IWebHostEnvironment environment, ILogger<StaticFileStorageService> logger)
      : this(configurationService.GetStaticFileSettings(), environment.ContentRootPath, logger)
    {
    }

    public StaticFileStorageService(StaticFileSettings settings, string contentRootPath, ILogger<StaticFileStorageService> logger)
    {
      _logger = logger;
      Settings = settings;

      var storagePath = Path.IsPathRooted(settings.StoragePath)
        ? settings.StoragePath
        : Path.Combine(contentRootPath, settings.StoragePath);
      RootPath = Path.GetFullPath(storagePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

      // Staging sits beside (not inside) the served folder so half-written uploads are never public,
      // and so Directory.Move stays on the same volume.
      _stagingRoot = RootPath + "-staging";

      Directory.CreateDirectory(RootPath);
      Directory.CreateDirectory(_stagingRoot);
      CleanStagingFolder();

      ContentTypeProvider = CreateContentTypeProvider();
      _logger.LogInformation("Static uploads stored in {RootPath}", RootPath);
    }

    public static FileExtensionContentTypeProvider CreateContentTypeProvider()
    {
      var provider = new FileExtensionContentTypeProvider();
      provider.Mappings[".odt"] = "application/vnd.oasis.opendocument.text";
      provider.Mappings[".wasm"] = "application/wasm";
      return provider;
    }

    public string? NormalizeFolderPath(string? folderPath)
    {
      if (string.IsNullOrWhiteSpace(folderPath))
      {
        return null;
      }

      var segments = folderPath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
      if (segments.Length == 0 || segments.Length > MaxFolderDepth)
      {
        return null;
      }

      var normalized = new List<string>();
      foreach (var segment in segments)
      {
        var cleaned = InvalidFolderChars.Replace(segment.ToLowerInvariant(), "-");
        cleaned = RepeatedDashes.Replace(cleaned, "-").Trim('-', '.');
        if (cleaned.Length == 0)
        {
          return null;
        }
        normalized.Add(cleaned);
      }

      var result = string.Join('/', normalized);
      return result.Length <= MaxFolderPathLength ? result : null;
    }

    public bool FolderExistsOnDisk(string folderPath)
    {
      return Directory.Exists(GetFolderFullPath(folderPath));
    }

    public async Task<StagedUpload> StageAsync(Stream content, string originalFileName, bool extractArchive, CancellationToken cancellationToken = default)
    {
      var staged = new StagedUpload
      {
        StagingPath = Path.Combine(_stagingRoot, Guid.NewGuid().ToString("N"))
      };
      Directory.CreateDirectory(staged.StagingPath);

      try
      {
        if (extractArchive && originalFileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
          staged.IsArchive = true;
          await ExtractArchiveAsync(content, staged, cancellationToken);
        }
        else
        {
          await SaveSingleFileAsync(content, originalFileName, staged, cancellationToken);
        }

        if (staged.Files.Count == 0)
        {
          throw new StaticUploadException("The upload does not contain any files.");
        }

        _logger.LogDebug("Staged upload {FileName}: {FileCount} files, {TotalBytes} bytes",
          originalFileName, staged.Files.Count, staged.TotalSizeBytes);
        return staged;
      }
      catch (InvalidDataException ex)
      {
        Discard(staged);
        throw new StaticUploadException("The file is not a valid .zip archive.", ex);
      }
      catch
      {
        Discard(staged);
        throw;
      }
    }

    public void Commit(StagedUpload staged, string folderPath)
    {
      var target = GetFolderFullPath(folderPath);
      Directory.CreateDirectory(Path.GetDirectoryName(target)!);

      string? previous = null;
      if (Directory.Exists(target))
      {
        previous = Path.Combine(_stagingRoot, Guid.NewGuid().ToString("N") + "-previous");
        Directory.Move(target, previous);
      }

      try
      {
        Directory.Move(staged.StagingPath, target);
      }
      catch
      {
        if (previous != null)
        {
          Directory.Move(previous, target);
        }
        throw;
      }

      if (previous != null)
      {
        TryDeleteDirectory(previous);
      }

      _logger.LogInformation("Committed static upload to {FolderPath}", folderPath);
    }

    public void Discard(StagedUpload staged)
    {
      TryDeleteDirectory(staged.StagingPath);
    }

    public void DeleteFolder(string folderPath)
    {
      var target = GetFolderFullPath(folderPath);
      if (Directory.Exists(target))
      {
        Directory.Delete(target, recursive: true);
      }

      // Remove parent folders left empty by nested paths like "games/shapeshifter"
      var parent = Path.GetDirectoryName(target);
      while (parent != null
        && parent.Length > RootPath.Length
        && Directory.Exists(parent)
        && !Directory.EnumerateFileSystemEntries(parent).Any())
      {
        Directory.Delete(parent);
        parent = Path.GetDirectoryName(parent);
      }

      _logger.LogInformation("Deleted static upload folder {FolderPath}", folderPath);
    }

    public string BuildUrl(string folderPath, string? relativePath = null)
    {
      var folderUrl = $"{RequestPath}/{EscapePath(folderPath)}/";
      return relativePath == null ? folderUrl : folderUrl + EscapePath(relativePath);
    }

    // -------------------------------------------------------------------------

    private async Task SaveSingleFileAsync(Stream content, string originalFileName, StagedUpload staged, CancellationToken cancellationToken)
    {
      var fileName = SanitizeFileName(originalFileName);
      var destination = Path.Combine(staged.StagingPath, fileName);

      var (size, sha256) = await WriteFileAsync(content, destination, Settings.MaxUploadBytes,
        "The file exceeds the maximum upload size.", cancellationToken);

      staged.Files.Add(CreateStagedFile(fileName, size, sha256));
      staged.EntryPoint = fileName;
    }

    private async Task ExtractArchiveAsync(Stream content, StagedUpload staged, CancellationToken cancellationToken)
    {
      using var archive = new ZipArchive(content, ZipArchiveMode.Read, leaveOpen: true);

      var entries = archive.Entries
        .Where(e => !e.FullName.EndsWith('/') && !e.FullName.EndsWith('\\'))
        .Select(e => (Entry: e, Path: e.FullName.Replace('\\', '/').TrimStart('/')))
        .Where(x => !IsJunkEntry(x.Path))
        .ToList();

      if (entries.Count > Settings.MaxArchiveEntries)
      {
        throw new StaticUploadException($"The archive contains {entries.Count} files; the maximum is {Settings.MaxArchiveEntries}.");
      }

      // A zip of "shapeshifter/" should land in the chosen folder, not in folder/shapeshifter/
      var commonRoot = GetCommonRootFolder(entries.Select(x => x.Path));
      var stagingPrefix = Path.GetFullPath(staged.StagingPath) + Path.DirectorySeparatorChar;
      var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      var remainingBytes = Settings.MaxExtractedBytes;

      foreach (var (entry, path) in entries)
      {
        var relativePath = commonRoot == null ? path : path.Substring(commonRoot.Length + 1);

        if (relativePath.Split('/').Any(s => s.Length == 0 || s == "." || s == ".."))
        {
          throw new StaticUploadException($"The archive entry '{entry.FullName}' has an invalid path.");
        }

        // Guard against "zip slip": entries must never resolve outside the staging folder
        var destination = Path.GetFullPath(Path.Combine(staged.StagingPath, relativePath));
        if (!destination.StartsWith(stagingPrefix, StringComparison.Ordinal))
        {
          throw new StaticUploadException($"The archive entry '{entry.FullName}' points outside the upload folder.");
        }

        if (!seenPaths.Add(relativePath))
        {
          throw new StaticUploadException($"The archive contains '{relativePath}' more than once.");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);

        // Count bytes actually written rather than trusting the entry's declared length
        await using var entryStream = entry.Open();
        var (size, sha256) = await WriteFileAsync(entryStream, destination, remainingBytes,
          "The archive's extracted contents exceed the maximum allowed size.", cancellationToken);
        remainingBytes -= size;

        staged.Files.Add(CreateStagedFile(relativePath, size, sha256));
      }

      staged.EntryPoint = FindEntryPoint(staged.Files);
    }

    private static async Task<(long Size, string Sha256)> WriteFileAsync(Stream source, string destination, long maxBytes, string tooLargeMessage, CancellationToken cancellationToken)
    {
      using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
      var buffer = new byte[CopyBufferSize];
      long total = 0;

      await using (var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None, CopyBufferSize, useAsync: true))
      {
        int read;
        while ((read = await source.ReadAsync(buffer, cancellationToken)) > 0)
        {
          total += read;
          if (total > maxBytes)
          {
            throw new StaticUploadException(tooLargeMessage);
          }

          hash.AppendData(buffer, 0, read);
          await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }
      }

      return (total, Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant());
    }

    private StagedFile CreateStagedFile(string relativePath, long size, string sha256)
    {
      if (!ContentTypeProvider.TryGetContentType(relativePath, out var contentType))
      {
        contentType = "application/octet-stream";
      }

      return new StagedFile
      {
        RelativePath = relativePath,
        SizeBytes = size,
        Sha256 = sha256,
        ContentType = contentType
      };
    }

    private string GetFolderFullPath(string folderPath)
    {
      if (NormalizeFolderPath(folderPath) != folderPath)
      {
        throw new ArgumentException($"'{folderPath}' is not a normalized folder path.", nameof(folderPath));
      }

      return Path.GetFullPath(Path.Combine(RootPath, folderPath.Replace('/', Path.DirectorySeparatorChar)));
    }

    private static string SanitizeFileName(string originalFileName)
    {
      var fileName = Path.GetFileName(originalFileName.Replace('\\', '/'));
      fileName = InvalidFileNameChars.Replace(fileName, "-");
      fileName = RepeatedDashes.Replace(fileName, "-").Trim('-', '.');
      return fileName.Length == 0 ? "file" : fileName;
    }

    private static bool IsJunkEntry(string path)
    {
      var fileName = path.Split('/').Last();
      return path.StartsWith("__MACOSX/", StringComparison.Ordinal)
        || fileName.Equals(".DS_Store", StringComparison.Ordinal)
        || fileName.Equals("Thumbs.db", StringComparison.OrdinalIgnoreCase);
    }

    private static string? GetCommonRootFolder(IEnumerable<string> paths)
    {
      string? root = null;
      foreach (var path in paths)
      {
        var slash = path.IndexOf('/');
        if (slash <= 0)
        {
          return null;
        }

        var first = path.Substring(0, slash);
        if (root == null)
        {
          root = first;
        }
        else if (!string.Equals(root, first, StringComparison.Ordinal))
        {
          return null;
        }
      }
      return root;
    }

    private static string? FindEntryPoint(List<StagedFile> files)
    {
      var rootHtmlFiles = files
        .Select(f => f.RelativePath)
        .Where(p => !p.Contains('/') && (p.EndsWith(".html", StringComparison.OrdinalIgnoreCase) || p.EndsWith(".htm", StringComparison.OrdinalIgnoreCase)))
        .ToList();

      var index = rootHtmlFiles.FirstOrDefault(p => p.Equals("index.html", StringComparison.OrdinalIgnoreCase) || p.Equals("index.htm", StringComparison.OrdinalIgnoreCase));
      if (index != null)
      {
        return index;
      }

      return rootHtmlFiles.Count == 1 ? rootHtmlFiles[0] : null;
    }

    private static string EscapePath(string path)
    {
      return string.Join('/', path.Split('/').Select(Uri.EscapeDataString));
    }

    private void CleanStagingFolder()
    {
      // Nothing is in flight at startup, so anything left here is from an interrupted upload
      foreach (var leftover in Directory.EnumerateDirectories(_stagingRoot))
      {
        TryDeleteDirectory(leftover);
      }
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
  }
}
