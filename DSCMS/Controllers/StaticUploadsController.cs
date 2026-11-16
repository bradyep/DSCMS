using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DSCMS.Filters;
using DSCMS.Models;
using DSCMS.Models.DTOs;
using DSCMS.Repositories.Interfaces;
using DSCMS.Services;

namespace DSCMS.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class StaticUploadsController : ControllerBase
{
  private const int MaxDescriptionLength = 500;

  private readonly IStaticUploadRepository _staticUploadRepository;
  private readonly IStaticFileStorageService _storage;
  private readonly ILogger<StaticUploadsController> _logger;

  public StaticUploadsController(IStaticUploadRepository staticUploadRepository, IStaticFileStorageService storage, ILogger<StaticUploadsController> logger)
  {
    _staticUploadRepository = staticUploadRepository;
    _storage = storage;
    _logger = logger;
  }

  // GET: api/staticuploads
  [HttpGet]
  public async Task<ActionResult<IEnumerable<StaticUploadListDto>>> GetAll()
  {
    _logger.LogDebug("API GetAll static uploads requested");

    var uploads = await _staticUploadRepository.GetAllWithUploaderAsync();
    var dtos = uploads.Select(MapSummary<StaticUploadListDto>).ToList();

    _logger.LogInformation("Returning {UploadCount} static uploads", dtos.Count);
    return Ok(dtos);
  }

  // GET: api/staticuploads/{id}
  [HttpGet("{id}")]
  public async Task<ActionResult<StaticUploadDetailDto>> GetById(int id)
  {
    _logger.LogDebug("API GetById requested for static upload id: {StaticUploadId}", id);

    var upload = await _staticUploadRepository.GetByIdWithFilesAsync(id);
    if (upload == null)
    {
      _logger.LogWarning("Static upload not found with id: {StaticUploadId}", id);
      return NotFound();
    }

    return Ok(MapDetail(upload));
  }

  // POST: api/staticuploads (multipart/form-data)
  [HttpPost]
  [StaticUploadSizeLimit]
  public async Task<ActionResult<StaticUploadDetailDto>> Create(
    IFormFile? file,
    [FromForm] string? folderPath,
    [FromForm] string? description,
    [FromForm] bool extract = true,
    CancellationToken cancellationToken = default)
  {
    _logger.LogDebug("API Create static upload requested: {FileName} -> {FolderPath}", file?.FileName, folderPath);

    if (file == null || file.Length == 0)
    {
      return BadRequest(new { error = "Please choose a file to upload." });
    }

    if (description?.Length > MaxDescriptionLength)
    {
      return BadRequest(new { error = $"Description must be {MaxDescriptionLength} characters or fewer." });
    }

    var requestedFolder = string.IsNullOrWhiteSpace(folderPath) ? Path.GetFileNameWithoutExtension(file.FileName) : folderPath;
    var normalizedFolder = _storage.NormalizeFolderPath(requestedFolder);
    if (normalizedFolder == null)
    {
      return BadRequest(new { error = $"'{requestedFolder}' is not a valid folder path. Use letters, numbers, '.', '-' or '_', separated by '/'." });
    }

    var overlapping = FindOverlappingFolder(await _staticUploadRepository.GetAllFolderPathsAsync(), normalizedFolder);
    if (overlapping != null)
    {
      _logger.LogWarning("Static upload folder {FolderPath} overlaps existing upload {ExistingFolderPath}", normalizedFolder, overlapping);
      return Conflict(new { error = overlapping == normalizedFolder
        ? $"The folder '{normalizedFolder}' is already used by another upload. Replace that upload instead, or choose a different folder."
        : $"The folder '{normalizedFolder}' overlaps the existing upload '{overlapping}'. Uploads can't be nested inside each other." });
    }

    if (_storage.FolderExistsOnDisk(normalizedFolder))
    {
      _logger.LogWarning("Static upload folder {FolderPath} already exists on disk without a database record", normalizedFolder);
      return Conflict(new { error = $"The folder '{normalizedFolder}' already exists on disk. Choose a different folder." });
    }

    StagedUpload staged;
    try
    {
      await using var stream = file.OpenReadStream();
      staged = await _storage.StageAsync(stream, file.FileName, extract, cancellationToken);
    }
    catch (StaticUploadException ex)
    {
      _logger.LogWarning("Static upload {FileName} rejected: {Reason}", file.FileName, ex.Message);
      return BadRequest(new { error = ex.Message });
    }

    _storage.Commit(staged, normalizedFolder);

    var upload = new StaticUpload
    {
      FolderPath = normalizedFolder,
      OriginalFileName = Path.GetFileName(file.FileName),
      Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
      UploadedAt = DateTime.UtcNow,
      UploadedBy = User.FindFirstValue(ClaimTypes.NameIdentifier)
    };
    ApplyStagedFiles(upload, staged);

    try
    {
      await _staticUploadRepository.AddAsync(upload);
    }
    catch
    {
      // Don't leave files on disk that the database doesn't know about
      _storage.DeleteFolder(normalizedFolder);
      throw;
    }

    _logger.LogInformation("Created static upload {StaticUploadId} at {FolderPath} with {FileCount} files",
      upload.StaticUploadId, upload.FolderPath, upload.FileCount);

    return CreatedAtAction(nameof(GetById), new { id = upload.StaticUploadId }, MapDetail(upload));
  }

  // PUT: api/staticuploads/{id}/file (multipart/form-data) - replaces the contents, keeping the folder and URLs
  [HttpPut("{id}/file")]
  [StaticUploadSizeLimit]
  public async Task<ActionResult<StaticUploadDetailDto>> ReplaceFiles(
    int id,
    IFormFile? file,
    [FromForm] bool extract = true,
    CancellationToken cancellationToken = default)
  {
    _logger.LogDebug("API ReplaceFiles requested for static upload id: {StaticUploadId}", id);

    if (file == null || file.Length == 0)
    {
      return BadRequest(new { error = "Please choose a file to upload." });
    }

    var upload = await _staticUploadRepository.GetByIdWithFilesAsync(id);
    if (upload == null)
    {
      _logger.LogWarning("Static upload not found for replace with id: {StaticUploadId}", id);
      return NotFound();
    }

    StagedUpload staged;
    try
    {
      await using var stream = file.OpenReadStream();
      staged = await _storage.StageAsync(stream, file.FileName, extract, cancellationToken);
    }
    catch (StaticUploadException ex)
    {
      _logger.LogWarning("Replacement for static upload {StaticUploadId} rejected: {Reason}", id, ex.Message);
      return BadRequest(new { error = ex.Message });
    }

    _storage.Commit(staged, upload.FolderPath);

    upload.OriginalFileName = Path.GetFileName(file.FileName);
    upload.LastReplacedAt = DateTime.UtcNow;
    var files = ApplyStagedFiles(upload, staged);

    try
    {
      await _staticUploadRepository.ReplaceFilesAsync(upload, files);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Files for static upload {StaticUploadId} were replaced on disk but the database update failed", id);
      throw;
    }

    _logger.LogInformation("Replaced files for static upload {StaticUploadId} at {FolderPath} ({FileCount} files)",
      upload.StaticUploadId, upload.FolderPath, upload.FileCount);

    return Ok(MapDetail(upload));
  }

  // PUT: api/staticuploads/{id}
  [HttpPut("{id}")]
  public async Task<ActionResult<StaticUploadDetailDto>> Update(int id, [FromBody] StaticUploadUpdateDto dto)
  {
    _logger.LogDebug("API Update requested for static upload id: {StaticUploadId}", id);

    if (!ModelState.IsValid)
    {
      _logger.LogWarning("Model state invalid for static upload update: {StaticUploadId}", id);
      return BadRequest(ModelState);
    }

    var upload = await _staticUploadRepository.GetByIdWithFilesAsync(id);
    if (upload == null)
    {
      _logger.LogWarning("Static upload not found for update with id: {StaticUploadId}", id);
      return NotFound();
    }

    upload.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
    await _staticUploadRepository.UpdateAsync(upload);

    _logger.LogInformation("Updated static upload {StaticUploadId}", id);
    return Ok(MapDetail(upload));
  }

  // DELETE: api/staticuploads/{id}
  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(int id)
  {
    _logger.LogDebug("API Delete requested for static upload id: {StaticUploadId}", id);

    var upload = await _staticUploadRepository.GetByIdAsync(id);
    if (upload == null)
    {
      _logger.LogWarning("Static upload not found for deletion with id: {StaticUploadId}", id);
      return NotFound();
    }

    await _staticUploadRepository.DeleteAsync(upload);

    try
    {
      _storage.DeleteFolder(upload.FolderPath);
    }
    catch (IOException ex)
    {
      _logger.LogWarning(ex, "Static upload {StaticUploadId} deleted but its folder {FolderPath} could not be removed", id, upload.FolderPath);
    }

    _logger.LogInformation("Deleted static upload {StaticUploadId} at {FolderPath}", id, upload.FolderPath);
    return NoContent();
  }

  // -------------------------------------------------------------------------

  /// <summary>
  /// Returns the existing folder that equals, contains, or sits inside the requested one.
  /// </summary>
  private static string? FindOverlappingFolder(IEnumerable<string> existingFolders, string folderPath)
  {
    return existingFolders.FirstOrDefault(existing =>
      string.Equals(existing, folderPath, StringComparison.OrdinalIgnoreCase)
      || folderPath.StartsWith(existing + "/", StringComparison.OrdinalIgnoreCase)
      || existing.StartsWith(folderPath + "/", StringComparison.OrdinalIgnoreCase));
  }

  private static List<StaticUploadFile> ApplyStagedFiles(StaticUpload upload, StagedUpload staged)
  {
    upload.IsArchive = staged.IsArchive;
    upload.EntryPoint = staged.EntryPoint;
    upload.FileCount = staged.Files.Count;
    upload.TotalSizeBytes = staged.TotalSizeBytes;
    upload.Files = staged.Files.Select(f => new StaticUploadFile
    {
      RelativePath = f.RelativePath,
      SizeBytes = f.SizeBytes,
      Sha256 = f.Sha256,
      ContentType = f.ContentType
    }).ToList();
    return upload.Files;
  }

  private T MapSummary<T>(StaticUpload upload) where T : StaticUploadListDto, new()
  {
    return new T
    {
      StaticUploadId = upload.StaticUploadId,
      FolderPath = upload.FolderPath,
      OriginalFileName = upload.OriginalFileName,
      IsArchive = upload.IsArchive,
      EntryPoint = upload.EntryPoint,
      FileCount = upload.FileCount,
      TotalSizeBytes = upload.TotalSizeBytes,
      Description = upload.Description,
      UploadedAt = AsUtc(upload.UploadedAt),
      LastReplacedAt = upload.LastReplacedAt.HasValue ? AsUtc(upload.LastReplacedAt.Value) : null,
      UploadedByUserName = upload.UploadedByUser?.DisplayName ?? upload.UploadedByUser?.UserName,
      FolderUrl = _storage.BuildUrl(upload.FolderPath),
      EntryUrl = upload.EntryPoint == null ? null : _storage.BuildUrl(upload.FolderPath, upload.EntryPoint)
    };
  }

  // SQLite doesn't keep DateTimeKind; times are always stored as UTC, so mark them as such for the client
  private static DateTime AsUtc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);

  private StaticUploadDetailDto MapDetail(StaticUpload upload)
  {
    var dto = MapSummary<StaticUploadDetailDto>(upload);
    dto.Files = upload.Files
      .OrderBy(f => f.RelativePath, StringComparer.OrdinalIgnoreCase)
      .Select(f => new StaticUploadFileDto
      {
        RelativePath = f.RelativePath,
        SizeBytes = f.SizeBytes,
        Sha256 = f.Sha256,
        ContentType = f.ContentType,
        Url = _storage.BuildUrl(upload.FolderPath, f.RelativePath)
      })
      .ToList();
    return dto;
  }
}
