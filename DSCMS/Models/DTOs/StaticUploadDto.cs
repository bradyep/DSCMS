using System.ComponentModel.DataAnnotations;

namespace DSCMS.Models.DTOs;

/// <summary>
/// DTO for static upload list display (Index page)
/// </summary>
public class StaticUploadListDto
{
  public int StaticUploadId { get; set; }
  public string FolderPath { get; set; } = string.Empty;
  public string OriginalFileName { get; set; } = string.Empty;
  public bool IsArchive { get; set; }
  public string? EntryPoint { get; set; }
  public int FileCount { get; set; }
  public long TotalSizeBytes { get; set; }
  public string? Description { get; set; }
  public DateTime UploadedAt { get; set; }
  public DateTime? LastReplacedAt { get; set; }
  public string? UploadedByUserName { get; set; }
  public string FolderUrl { get; set; } = string.Empty;
  public string? EntryUrl { get; set; }
}

/// <summary>
/// DTO for detailed static upload view, including every file
/// </summary>
public class StaticUploadDetailDto : StaticUploadListDto
{
  public List<StaticUploadFileDto> Files { get; set; } = new();
}

public class StaticUploadFileDto
{
  public string RelativePath { get; set; } = string.Empty;
  public long SizeBytes { get; set; }
  public string Sha256 { get; set; } = string.Empty;
  public string? ContentType { get; set; }
  public string Url { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating a static upload's metadata (files are replaced via a separate upload)
/// </summary>
public class StaticUploadUpdateDto
{
  [StringLength(500)]
  public string? Description { get; set; }
}
