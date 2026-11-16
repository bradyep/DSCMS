namespace DSCMS.Models
{
  /// <summary>
  /// manifest.json at the root of a backup archive. Describes the archive's layout and lists every file with its hash,
  /// so a restore can verify it received exactly what was backed up.
  /// </summary>
  public class BackupManifest
  {
    /// <summary>
    /// Version of the archive layout itself (not the app or the DB schema). Bump only when the layout changes.
    /// </summary>
    public const int CurrentFormatVersion = 1;

    public const string FileName = "manifest.json";
    public const string DatabaseEntryName = "dscms.db";
    public const string StaticFolderPrefix = "static/";

    public int FormatVersion { get; set; }

    /// <summary>
    /// Informational only: the DSCMS build that created the backup.
    /// </summary>
    public string AppVersion { get; set; } = "unknown";

    public DateTime CreatedAtUtc { get; set; }
    public string SourceMachine { get; set; } = string.Empty;
    public string SourceEnvironment { get; set; } = string.Empty;

    /// <summary>
    /// EF migration IDs applied to the backed-up database, oldest first.
    /// </summary>
    public List<string> Migrations { get; set; } = new();

    public BackupManifestFile Database { get; set; } = new();

    /// <summary>
    /// Every file under the static uploads folder. Paths are relative to that folder and use '/' separators.
    /// </summary>
    public List<BackupManifestFile> StaticFiles { get; set; } = new();

    /// <summary>
    /// Problems noticed while backing up (e.g. files on disk the database doesn't know about).
    /// </summary>
    public List<string> Warnings { get; set; } = new();
  }

  public class BackupManifestFile
  {
    public string Path { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string Sha256 { get; set; } = string.Empty;
  }
}
