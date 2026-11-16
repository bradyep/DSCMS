using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DSCMS.Models
{
  /// <summary>
  /// One admin upload (a single file or an extracted .zip) living in its own folder under the static files root.
  /// </summary>
  public class StaticUpload
  {
    public int StaticUploadId { get; set; }

    /// <summary>
    /// Normalized folder path relative to the static files root, e.g. "games/shapeshifter".
    /// </summary>
    [Required, StringLength(200)]
    public string FolderPath { get; set; } = string.Empty;

    [Required, StringLength(260)]
    public string OriginalFileName { get; set; } = string.Empty;

    public bool IsArchive { get; set; }

    /// <summary>
    /// Path (relative to FolderPath) of the main file to link to, if there is an obvious one.
    /// </summary>
    [StringLength(500)]
    public string? EntryPoint { get; set; }

    public long TotalSizeBytes { get; set; }
    public int FileCount { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public DateTime UploadedAt { get; set; }
    public DateTime? LastReplacedAt { get; set; }

    public string? UploadedBy { get; set; }
    [ForeignKey("UploadedBy")]
    public ApplicationUser? UploadedByUser { get; set; }

    public List<StaticUploadFile> Files { get; set; } = new List<StaticUploadFile>();
  }
}
