using DSCMS.Models.DTOs;

namespace DSCMS.Services
{
  /// <summary>
  /// Backs up and restores the whole site: the SQLite database plus every file in the static uploads folder,
  /// packed into one .zip with a manifest.json of hashes.
  /// </summary>
  public interface IBackupService
  {
    BackupSettings Settings { get; }

    /// <summary>
    /// Counts and consistency warnings for the live site, plus the saved snapshots.
    /// </summary>
    Task<BackupStatusDto> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes a backup to a temporary file and returns it opened for reading; the file is deleted when the stream is closed.
    /// Throws <see cref="BackupBusyException"/> if another backup or restore is running.
    /// </summary>
    Task<FileStream> CreateBackupAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts and verifies an uploaded backup without changing the site. If it can be restored,
    /// the preview carries a token for <see cref="CommitRestoreAsync"/>. Only one restore is staged at a time.
    /// Throws <see cref="BackupException"/> if the file isn't a usable DSCMS backup at all.
    /// </summary>
    Task<RestorePreviewDto> StageRestoreAsync(Stream archive, string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Like <see cref="StageRestoreAsync"/>, for a snapshot saved on the server.
    /// </summary>
    Task<RestorePreviewDto> StageSnapshotAsync(string snapshotName, CancellationToken cancellationToken = default);

    void DiscardStagedRestore(string token);

    /// <summary>
    /// Snapshots the current site, then replaces its database and static files with the staged backup.
    /// The site's existing user accounts are kept.
    /// </summary>
    Task<RestoreResultDto> CommitRestoreAsync(string token, string? restoringUserId, CancellationToken cancellationToken = default);

    IReadOnlyList<BackupSnapshotDto> ListSnapshots();

    /// <summary>
    /// Full path of a saved snapshot, or null if the name is invalid or doesn't exist.
    /// </summary>
    string? GetSnapshotPath(string snapshotName);
  }

  /// <summary>
  /// A backup or restore was rejected (invalid archive, failed verification, expired token, ...).
  /// </summary>
  public class BackupException : Exception
  {
    public BackupException(string message, Exception? innerException = null)
      : base(message, innerException)
    {
    }
  }

  public class BackupBusyException : BackupException
  {
    public BackupBusyException()
      : base("Another backup or restore is already running. Please try again in a moment.")
    {
    }
  }
}
