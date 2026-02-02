using System.ComponentModel.DataAnnotations;

namespace DSCMS.Models
{
  /// <summary>
  /// Defines the source type for content rendering (e.g., Razor file, Markdown, HTML, etc.)
  /// </summary>
  public class SourceType
  {
    [Key]
    public int SourceTypeId { get; set; }

    [Required]
    [StringLength(100)]
    public string Description { get; set; } = string.Empty;
  }
}
