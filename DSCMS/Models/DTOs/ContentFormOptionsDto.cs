namespace DSCMS.Models.DTOs;

/// <summary>
/// DTO containing all dropdown/select options for content forms
/// </summary>
public class ContentFormOptionsDto
{
    public List<LookupItemDto> ContentTypes { get; set; } = new();
    public List<LookupItemDto> Users { get; set; } = new();
    public List<LookupItemDto> Templates { get; set; } = new();
    public List<LookupItemDto> SourceTypes { get; set; } = new();
    public Dictionary<int, int> DefaultTemplateLookup { get; set; } = new();
}

/// <summary>
/// Generic lookup item for dropdowns
/// </summary>
public class LookupItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
