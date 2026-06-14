namespace DSCMS.Models.DTOs;

/// <summary>
/// DTO for content list display (Index page)
/// </summary>
public class ContentListDto
{
    public int ContentId { get; set; }
    public string? Title { get; set; }
    public string? ContentTypeName { get; set; }
    public string? UrlToDisplay { get; set; }
    public bool BodySourceHasContent { get; set; }
    public DateTime LastUpdatedDate { get; set; }
}
