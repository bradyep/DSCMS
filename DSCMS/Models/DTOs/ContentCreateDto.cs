using System.ComponentModel.DataAnnotations;

namespace DSCMS.Models.DTOs;

/// <summary>
/// DTO for creating new content
/// </summary>
public class ContentCreateDto
{
    public string? BodySource { get; set; }

    [Required]
    public int BodySourceTypeId { get; set; }

    [Required]
    public int ContentTypeId { get; set; }

    [Required]
    public string? CreatedBy { get; set; }

    [Required]
    public string? LastUpdatedBy { get; set; }

    [Required]
    public int TemplateId { get; set; }

    public string? Title { get; set; }

    [Required]
    [RegularExpression(@"^[a-z0-9-]+$")]
    [StringLength(100)]
    public string UrlToDisplay { get; set; } = string.Empty;
}
