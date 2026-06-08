namespace DSCMS.Models.DTOs;

/// <summary>
/// DTO for detailed content view including field items
/// </summary>
public class ContentDetailDto
{
    public int ContentId { get; set; }
    public string? BodySource { get; set; }
    public int BodySourceTypeId { get; set; }
    public int ContentTypeId { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreationDate { get; set; }
    public string? LastUpdatedBy { get; set; }
    public DateTime LastUpdatedDate { get; set; }
    public int TemplateId { get; set; }
    public string? Title { get; set; }
    public string? UrlToDisplay { get; set; }
    public List<ContentTypeFieldItemDto>? ContentTypeFieldItems { get; set; }
}

/// <summary>
/// DTO for content type field items
/// </summary>
public class ContentTypeFieldItemDto
{
    public int ContentTypeFieldItemId { get; set; }
    public string? FieldName { get; set; }
    public string? Value { get; set; }
}
