namespace DSCMS.Models.DTOs;

/// <summary>
/// Row counts for a site database, shown for the live site and for a backup about to be restored.
/// </summary>
public class SiteCountsDto
{
  public int ContentTypes { get; set; }
  public int Contents { get; set; }
  public int Layouts { get; set; }
  public int Templates { get; set; }
  public int StaticUploads { get; set; }
  public int Users { get; set; }
  public int StaticFiles { get; set; }
  public long StaticBytes { get; set; }
}

/// <summary>
/// The live site's state for the Backup &amp; Restore page.
/// </summary>
public class BackupStatusDto
{
  public SiteCountsDto Counts { get; set; } = new();
  public List<string> Migrations { get; set; } = new();
  public List<string> Warnings { get; set; } = new();
  public List<BackupSnapshotDto> Snapshots { get; set; } = new();
  public bool IsBusy { get; set; }
}

public class BackupSnapshotDto
{
  public string Name { get; set; } = string.Empty;
  public long SizeBytes { get; set; }
  public DateTime CreatedAtUtc { get; set; }
}

/// <summary>
/// Result of checking an uploaded backup. Nothing has been changed yet; restoring requires committing <see cref="Token"/>.
/// </summary>
public class RestorePreviewDto
{
  /// <summary>
  /// Set only when <see cref="CanRestore"/> is true.
  /// </summary>
  public string? Token { get; set; }
  public bool CanRestore => Errors.Count == 0;

  public string FileName { get; set; } = string.Empty;
  public int FormatVersion { get; set; }
  public string AppVersion { get; set; } = string.Empty;
  public DateTime CreatedAtUtc { get; set; }
  public string SourceMachine { get; set; } = string.Empty;
  public string SourceEnvironment { get; set; } = string.Empty;

  public SiteCountsDto Counts { get; set; } = new();

  /// <summary>
  /// Migrations the backup is missing; they will be applied during the restore.
  /// </summary>
  public List<string> PendingMigrations { get; set; } = new();

  public List<string> Errors { get; set; } = new();
  public List<string> Warnings { get; set; } = new();
}

public class RestoreResultDto
{
  /// <summary>
  /// Name of the snapshot of the site taken just before the restore, for undoing it.
  /// </summary>
  public string SnapshotName { get; set; } = string.Empty;
  public SiteCountsDto Counts { get; set; } = new();
}
