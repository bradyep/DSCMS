using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DSCMS.Models
{
  /// <summary>
  /// One physical file belonging to a StaticUpload.
  /// </summary>
  public class StaticUploadFile
  {
    public int StaticUploadFileId { get; set; }

    /// <summary>
    /// Path relative to the owning upload's folder, using '/' separators.
    /// </summary>
    [Required, StringLength(500)]
    public string RelativePath { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    [Required, StringLength(64)]
    public string Sha256 { get; set; } = string.Empty;

    [StringLength(200)]
    public string? ContentType { get; set; }

    public int StaticUploadId { get; set; }
    [ForeignKey("StaticUploadId")]
    public StaticUpload? StaticUpload { get; set; }
  }
}
