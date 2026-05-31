using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DSCMS.Models;
using DSCMS.Models.DTOs;
using System.Security.Claims;
using DSCMS.Repositories.Interfaces;

namespace DSCMS.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ContentsController : ControllerBase
{
  private readonly IContentRepository _contentRepository;
  private readonly IContentTypeRepository _contentTypeRepository;
  private readonly ITemplateRepository _templateRepository;
  private readonly ISourceTypeRepository _sourceTypeRepository;
  private readonly ILogger<ContentsController> _logger;

  public ContentsController(
    IContentRepository contentRepository,
    IContentTypeRepository contentTypeRepository,
    ITemplateRepository templateRepository,
    ISourceTypeRepository sourceTypeRepository,
    ILogger<ContentsController> logger)
  {
    _contentRepository = contentRepository;
    _contentTypeRepository = contentTypeRepository;
    _templateRepository = templateRepository;
    _sourceTypeRepository = sourceTypeRepository;
    _logger = logger;
  }

  // GET: api/contents
  [HttpGet]
  public async Task<ActionResult<IEnumerable<ContentListDto>>> GetAll([FromQuery] string? contentType)
  {
    _logger.LogDebug("API GetAll contents requested with contentType filter: {ContentType}", contentType);

    var contents = await _contentRepository.GetAllWithDetailsAsync(contentType);

    var dtos = contents.Select(c => new ContentListDto
    {
      ContentId = c.ContentId,
      Title = c.Title,
      ContentTypeName = c.ContentType?.Name,
      UrlToDisplay = c.UrlToDisplay,
      BodySourceHasContent = !string.IsNullOrWhiteSpace(c.BodySource),
      LastUpdatedDate = c.LastUpdatedDate
    }).ToList();

    _logger.LogInformation("Returning {ContentCount} contents for type '{ContentType}'", dtos.Count, contentType ?? "all");
    return Ok(dtos);
  }

  // GET: api/contents/{id}
  [HttpGet("{id}")]
  public async Task<ActionResult<ContentDetailDto>> GetById(int id)
  {
    _logger.LogDebug("API GetById requested for id: {ContentId}", id);

    var content = await _contentRepository.GetByIdWithFieldItemsAsync(id);
    if (content == null)
    {
      _logger.LogWarning("Content not found with id: {ContentId}", id);
      return NotFound();
    }

    var dto = new ContentDetailDto
    {
      ContentId = content.ContentId,
      BodySource = content.BodySource,
      BodySourceTypeId = content.BodySourceTypeId,
      ContentTypeId = content.ContentTypeId,
      CreatedBy = content.CreatedBy,
      CreationDate = content.CreationDate,
      LastUpdatedBy = content.LastUpdatedBy,
      LastUpdatedDate = content.LastUpdatedDate,
      TemplateId = content.TemplateId,
      Title = content.Title,
      UrlToDisplay = content.UrlToDisplay,
      ContentTypeFieldItems = content.ContentTypeFieldItems?.Select(fi => new ContentTypeFieldItemDto
      {
        ContentTypeFieldItemId = fi.ContentTypeFieldItemId,
        FieldName = fi.ContentTypeField?.Name,
        Value = fi.Value
      }).ToList()
    };

    _logger.LogDebug("Found content: {ContentId} - {ContentTitle}", content.ContentId, content.Title);
    return Ok(dto);
  }

  // POST: api/contents
  [HttpPost]
  public async Task<ActionResult<ContentDetailDto>> Create([FromBody] ContentCreateDto dto)
  {
    _logger.LogDebug("API Create content requested for title: {ContentTitle}", dto.Title);

    if (!ModelState.IsValid)
    {
      _logger.LogWarning("Model state invalid for content creation: {ContentTitle}", dto.Title);
      return BadRequest(ModelState);
    }

    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

    var content = new Content
    {
      BodySource = dto.BodySource,
      BodySourceTypeId = dto.BodySourceTypeId,
      ContentTypeId = dto.ContentTypeId,
      CreatedBy = userId,
      CreationDate = DateTime.Now,
      LastUpdatedBy = userId,
      LastUpdatedDate = DateTime.Now,
      TemplateId = dto.TemplateId,
      Title = dto.Title,
      UrlToDisplay = dto.UrlToDisplay
    };

    await _contentRepository.AddAsync(content);
    _logger.LogInformation("Created new content: {ContentId} - {ContentTitle}", content.ContentId, content.Title);

    var resultDto = new ContentDetailDto
    {
      ContentId = content.ContentId,
      BodySource = content.BodySource,
      BodySourceTypeId = content.BodySourceTypeId,
      ContentTypeId = content.ContentTypeId,
      CreatedBy = content.CreatedBy,
      CreationDate = content.CreationDate,
      LastUpdatedBy = content.LastUpdatedBy,
      LastUpdatedDate = content.LastUpdatedDate,
      TemplateId = content.TemplateId,
      Title = content.Title,
      UrlToDisplay = content.UrlToDisplay,
      ContentTypeFieldItems = new List<ContentTypeFieldItemDto>()
    };

    return CreatedAtAction(nameof(GetById), new { id = content.ContentId }, resultDto);
  }

  // PUT: api/contents/{id}
  [HttpPut("{id}")]
  public async Task<ActionResult<ContentDetailDto>> Update(int id, [FromBody] ContentUpdateDto dto)
  {
    _logger.LogDebug("API Update content requested for: {ContentId} - {ContentTitle}", id, dto.Title);

    if (!ModelState.IsValid)
    {
      _logger.LogWarning("Model state invalid for content update: {ContentId}", id);
      return BadRequest(ModelState);
    }

    var content = await _contentRepository.GetByIdAsync(id);
    if (content == null)
    {
      _logger.LogWarning("Content not found for update with id: {ContentId}", id);
      return NotFound();
    }

    content.BodySource = dto.BodySource;
    content.BodySourceTypeId = dto.BodySourceTypeId;
    content.ContentTypeId = dto.ContentTypeId;
    content.LastUpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
    content.LastUpdatedDate = DateTime.Now;
    content.TemplateId = dto.TemplateId;
    content.Title = dto.Title;
    content.UrlToDisplay = dto.UrlToDisplay;

    try
    {
      await _contentRepository.UpdateAsync(content);
      _logger.LogInformation("Updated content: {ContentId} - {ContentTitle}", content.ContentId, content.Title);
    }
    catch (DbUpdateConcurrencyException ex)
    {
      _logger.LogError(ex, "Concurrency exception updating content: {ContentId}", content.ContentId);
      if (!await _contentRepository.ExistsAsync(content.ContentId))
      {
        return NotFound();
      }
      else
      {
        throw;
      }
    }
    catch (DbUpdateException ex)
    {
      _logger.LogError(ex, "Database update exception updating content: {ContentId}", content.ContentId);
      return BadRequest("Unable to save changes. Please ensure all required fields have valid values.");
    }

    var resultDto = new ContentDetailDto
    {
      ContentId = content.ContentId,
      BodySource = content.BodySource,
      BodySourceTypeId = content.BodySourceTypeId,
      ContentTypeId = content.ContentTypeId,
      CreatedBy = content.CreatedBy,
      CreationDate = content.CreationDate,
      LastUpdatedBy = content.LastUpdatedBy,
      LastUpdatedDate = content.LastUpdatedDate,
      TemplateId = content.TemplateId,
      Title = content.Title,
      UrlToDisplay = content.UrlToDisplay,
      ContentTypeFieldItems = new List<ContentTypeFieldItemDto>()
    };

    return Ok(resultDto);
  }

  // DELETE: api/contents/{id}
  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(int id)
  {
    _logger.LogDebug("API Delete requested for id: {ContentId}", id);

    var content = await _contentRepository.GetByIdAsync(id);
    if (content == null)
    {
      _logger.LogWarning("Content not found for delete with id: {ContentId}", id);
      return NotFound();
    }

    await _contentRepository.DeleteAsync(content);
    _logger.LogInformation("Deleted content: {ContentId} - {ContentTitle}", content.ContentId, content.Title);

    return NoContent();
  }

  // GET: api/contents/form-options
  [HttpGet("form-options")]
  public async Task<ActionResult<ContentFormOptionsDto>> GetFormOptions()
  {
    _logger.LogDebug("API GetFormOptions requested");

    var contentTypes = await _contentTypeRepository.GetAllAsync();
    var templates = await _templateRepository.GetByIsForMultipleContentsAsync(0);
    var sourceTypes = await _sourceTypeRepository.GetAllAsync();

    var defaultTemplateLookup = new Dictionary<int, int>();
    var contentTypesWithDefaults = contentTypes.Where(ct => ct.DefaultSingleContentTemplateId != null);
    foreach (var ct in contentTypesWithDefaults)
    {
      defaultTemplateLookup[ct.ContentTypeId] = ct.DefaultSingleContentTemplateId ?? 0;
    }

    var dto = new ContentFormOptionsDto
    {
      ContentTypes = contentTypes.Select(ct => new LookupItemDto { Id = ct.ContentTypeId, Name = ct.Name ?? "" }).ToList(),
      Templates = templates.Select(t => new LookupItemDto { Id = t.TemplateId, Name = t.Name ?? "" }).ToList(),
      SourceTypes = sourceTypes.Select(st => new LookupItemDto { Id = st.SourceTypeId, Name = st.Description ?? "" }).ToList(),
      DefaultTemplateLookup = defaultTemplateLookup
    };

    _logger.LogInformation("Returning form options with {ContentTypeCount} content types, {TemplateCount} templates",
      dto.ContentTypes.Count, dto.Templates.Count);

    return Ok(dto);
  }
}
