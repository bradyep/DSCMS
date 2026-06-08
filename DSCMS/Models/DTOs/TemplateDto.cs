using System.ComponentModel.DataAnnotations;

namespace DSCMS.Models.DTOs;

/// <summary>
/// DTO for template list display (Index page)
/// </summary>
public class TemplateListDto
{
  public int TemplateId { get; set; }
  public string? Name { get; set; }
  public string? TemplateSource { get; set; }
  public string? LayoutName { get; set; }
  public int IsForMultipleContents { get; set; }
}

/// <summary>
/// DTO for detailed template view
/// </summary>
public class TemplateDetailDto
{
  public int TemplateId { get; set; }
  public string? Name { get; set; }
  public string? TemplateSource { get; set; }
  public int SourceTypeId { get; set; }
  public int IsForMultipleContents { get; set; }
  public int? LayoutId { get; set; }
  public string? LayoutName { get; set; }
}

/// <summary>
/// DTO for creating new template
/// </summary>
public class TemplateCreateDto
{
  public string? Name { get; set; }
  public string? TemplateSource { get; set; }

  [Required]
  public int SourceTypeId { get; set; }

  [Required]
  public int IsForMultipleContents { get; set; }

  public int? LayoutId { get; set; }
}

/// <summary>
/// DTO for updating existing template
/// </summary>
public class TemplateUpdateDto
{
  public string? Name { get; set; }
  public string? TemplateSource { get; set; }

  [Required]
  public int SourceTypeId { get; set; }

  [Required]
  public int IsForMultipleContents { get; set; }

  public int? LayoutId { get; set; }
}

/// <summary>
/// DTO containing form options for template creation/editing
/// </summary>
public class TemplateFormOptionsDto
{
  public List<LookupItemDto> Layouts { get; set; } = new();
}
