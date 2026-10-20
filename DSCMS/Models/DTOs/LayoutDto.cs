using System.ComponentModel.DataAnnotations;

namespace DSCMS.Models.DTOs;

/// <summary>
/// DTO for layout list display (Index page)
/// </summary>
public class LayoutListDto
{
  public int LayoutId { get; set; }
  public string Name { get; set; } = string.Empty;
  public string? LayoutSource { get; set; }
  public string? SourceTypeName { get; set; }
}

/// <summary>
/// DTO for detailed layout view
/// </summary>
public class LayoutDetailDto
{
  public int LayoutId { get; set; }
  public string Name { get; set; } = string.Empty;
  public string LayoutSource { get; set; } = string.Empty;
  public int SourceTypeId { get; set; }
  public string? SourceTypeName { get; set; }
  public List<LayoutValueDto> LayoutValues { get; set; } = new();
}

/// <summary>
/// DTO for creating new layout
/// </summary>
public class LayoutCreateDto
{
  [Required]
  public string Name { get; set; } = string.Empty;

  [Required]
  public string LayoutSource { get; set; } = string.Empty;

  [Required]
  public int SourceTypeId { get; set; }

  public List<LayoutValueDto>? LayoutValues { get; set; }
}

/// <summary>
/// DTO for updating existing layout
/// </summary>
public class LayoutUpdateDto
{
  [Required]
  public string Name { get; set; } = string.Empty;

  [Required]
  public string LayoutSource { get; set; } = string.Empty;

  [Required]
  public int SourceTypeId { get; set; }

  public List<LayoutValueDto>? LayoutValues { get; set; }
}

/// <summary>
/// DTO for a single Layout key/value pair
/// </summary>
public class LayoutValueDto
{
  public int LayoutValueId { get; set; }

  [Required]
  public string Key { get; set; } = string.Empty;

  public string? Value { get; set; }
}
