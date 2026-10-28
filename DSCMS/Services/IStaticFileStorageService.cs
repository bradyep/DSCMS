using Microsoft.AspNetCore.StaticFiles;

namespace DSCMS.Services
{
  /// <summary>
  /// Stores admin-uploaded static files (single files or extracted .zip archives) on disk.
  /// Uploads are first staged outside the served folder, then committed into place.
  /// </summary>
  public interface IStaticFileStorageService
  {
    /// <summary>
    /// Absolute path of the folder that is served at <see cref="StaticFileStorageService.RequestPath"/>.
    /// </summary>
    string RootPath { get; }

    StaticFileSettings Settings { get; }

    FileExtensionContentTypeProvider ContentTypeProvider { get; }

    /// <summary>
    /// Turns user input into a safe folder path like "games/shapeshifter", or null if it can't be made valid.
    /// </summary>
    string? NormalizeFolderPath(string? folderPath);

    bool FolderExistsOnDisk(string folderPath);

    /// <summary>
    /// Writes the upload into a private staging folder. Throws <see cref="StaticUploadException"/> for bad input.
    /// </summary>
    Task<StagedUpload> StageAsync(Stream content, string originalFileName, bool extractArchive, CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves a staged upload into its public folder, replacing anything already there.
    /// </summary>
    void Commit(StagedUpload staged, string folderPath);

    void Discard(StagedUpload staged);

    void DeleteFolder(string folderPath);

    /// <summary>
    /// Root-relative URL for a folder (with trailing slash) or for a file within it.
    /// </summary>
    string BuildUrl(string folderPath, string? relativePath = null);
  }

  public class StagedUpload
  {
    public string StagingPath { get; set; } = string.Empty;
    public bool IsArchive { get; set; }
    public string? EntryPoint { get; set; }
    public List<StagedFile> Files { get; set; } = new();
    public long TotalSizeBytes => Files.Sum(f => f.SizeBytes);
  }

  public class StagedFile
  {
    public string RelativePath { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string Sha256 { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
  }

  /// <summary>
  /// An upload was rejected because of its content (invalid archive, too large, unsafe paths, ...).
  /// </summary>
  public class StaticUploadException : Exception
  {
    public StaticUploadException(string message, Exception? innerException = null)
      : base(message, innerException)
    {
    }
  }
}
