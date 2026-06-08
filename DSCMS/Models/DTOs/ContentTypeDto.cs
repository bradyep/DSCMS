using System.ComponentModel.DataAnnotations;

namespace DSCMS.Models.DTOs;

/// <summary>
/// DTO for content type list display (Index page)
/// </summary>
public class ContentTypeListDto
{
  public int ContentTypeId { get; set; }
  public string Name { get; set; } = string.Empty;
  public string Title { get; set; } = string.Empty;
  public string? Description { get; set; }
  public int ItemsPerPage { get; set; }
  public string? MultipleContentsTemplateName { get; set; }
}

/// <summary>
/// DTO for detailed content type view
/// </summary>
public class ContentTypeDetailDto
{
  public int ContentTypeId { get; set; }
  public string Name { get; set; } = string.Empty;
  public string Title { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public int ItemsPerPage { get; set; }
  public int MultipleContentsTemplateId { get; set; }
  public int? DefaultSingleContentTemplateId { get; set; }
  public bool IsDefaultContentType { get; set; }
  public string? MultipleContentsTemplateName { get; set; }
  public string? DefaultSingleContentTemplateName { get; set; }
}

/// <summary>
/// DTO for creating new content type
/// </summary>
public class ContentTypeCreateDto
{
  [Required]
  public string Name { get; set; } = string.Empty;

  [Required]
  public string Title { get; set; } = string.Empty;

  public string Description { get; set; } = string.Empty;

  [Required]
  public int ItemsPerPage { get; set; }

  [Required]
  public int MultipleContentsTemplateId { get; set; }

  public int? DefaultSingleContentTemplateId { get; set; }

  public bool IsDefaultContentType { get; set; }
}

/// <summary>
/// DTO for updating existing content type
/// </summary>
public class ContentTypeUpdateDto
{
  [Required]
  public string Name { get; set; } = string.Empty;

  [Required]
  public string Title { get; set; } = string.Empty;

  public string Description { get; set; } = string.Empty;

  [Required]
  public int ItemsPerPage { get; set; }

  [Required]
  public int MultipleContentsTemplateId { get; set; }

  public int? DefaultSingleContentTemplateId { get; set; }

  public bool IsDefaultContentType { get; set; }
}

/// <summary>
/// DTO containing form options for content type creation/editing
/// </summary>
public class ContentTypeFormOptionsDto
{
  public List<LookupItemDto> MultipleContentsTemplates { get; set; } = new();
  public List<LookupItemDto> SingleContentTemplates { get; set; } = new();
}
