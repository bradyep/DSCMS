using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DSCMS.Models;
using DSCMS.Models.DTOs;
using DSCMS.Repositories.Interfaces;

namespace DSCMS.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ContentTypesController : ControllerBase
{
  private readonly IContentTypeRepository _contentTypeRepository;
  private readonly ITemplateRepository _templateRepository;
  private readonly ILogger<ContentTypesController> _logger;

  public ContentTypesController(
    IContentTypeRepository contentTypeRepository,
    ITemplateRepository templateRepository,
    ILogger<ContentTypesController> logger)
  {
    _contentTypeRepository = contentTypeRepository;
    _templateRepository = templateRepository;
    _logger = logger;
  }

  // GET: api/contenttypes
  [HttpGet]
  public async Task<ActionResult<IEnumerable<ContentTypeListDto>>> GetAll()
  {
    _logger.LogDebug("API GetAll content types requested");

    var contentTypes = await _contentTypeRepository.GetAllWithTemplateAsync();

    var dtos = contentTypes.Select(ct => new ContentTypeListDto
    {
      ContentTypeId = ct.ContentTypeId,
      Name = ct.Name,
      Title = ct.Title,
      Description = ct.Description,
      ItemsPerPage = ct.ItemsPerPage,
      MultipleContentsTemplateName = ct.MultipleContentsTemplate?.Name
    }).ToList();

    _logger.LogInformation("Returning {ContentTypeCount} content types", dtos.Count);
    return Ok(dtos);
  }

  // GET: api/contenttypes/{id}
  [HttpGet("{id}")]
  public async Task<ActionResult<ContentTypeDetailDto>> GetById(int id)
  {
    _logger.LogDebug("API GetById requested for content type id: {ContentTypeId}", id);

    var contentType = await _contentTypeRepository.GetByIdWithTemplateAsync(id);
    if (contentType == null)
    {
      _logger.LogWarning("Content type not found with id: {ContentTypeId}", id);
      return NotFound();
    }

    var dto = new ContentTypeDetailDto
    {
      ContentTypeId = contentType.ContentTypeId,
      Name = contentType.Name,
      Title = contentType.Title,
      Description = contentType.Description,
      ItemsPerPage = contentType.ItemsPerPage,
      MultipleContentsTemplateId = contentType.MultipleContentsTemplateId,
      DefaultSingleContentTemplateId = contentType.DefaultSingleContentTemplateId,
      IsDefaultContentType = contentType.IsDefaultContentType,
      MultipleContentsTemplateName = contentType.MultipleContentsTemplate?.Name,
      DefaultSingleContentTemplateName = contentType.DefaultSingleContentTemplate?.Name
    };

    _logger.LogDebug("Found content type: {ContentTypeId} - {ContentTypeName}", contentType.ContentTypeId, contentType.Name);
    return Ok(dto);
  }

  // POST: api/contenttypes
  [HttpPost]
  public async Task<ActionResult<ContentTypeDetailDto>> Create([FromBody] ContentTypeCreateDto dto)
  {
    _logger.LogDebug("API Create content type requested for name: {ContentTypeName}", dto.Name);

    if (dto.DefaultSingleContentTemplateId < 1)
    {
      dto.DefaultSingleContentTemplateId = null;
    }

    if (!ModelState.IsValid)
    {
      _logger.LogWarning("Model state invalid for content type creation: {ContentTypeName}", dto.Name);
      return BadRequest(ModelState);
    }

    var contentType = new ContentType
    {
      Name = dto.Name,
      Title = dto.Title,
      Description = dto.Description,
      ItemsPerPage = dto.ItemsPerPage,
      MultipleContentsTemplateId = dto.MultipleContentsTemplateId,
      DefaultSingleContentTemplateId = dto.DefaultSingleContentTemplateId,
      IsDefaultContentType = dto.IsDefaultContentType
    };

    await _contentTypeRepository.AddAsync(contentType);
    _logger.LogInformation("Created new content type: {ContentTypeId} - {ContentTypeName}", contentType.ContentTypeId, contentType.Name);

    var resultDto = new ContentTypeDetailDto
    {
      ContentTypeId = contentType.ContentTypeId,
      Name = contentType.Name,
      Title = contentType.Title,
      Description = contentType.Description,
      ItemsPerPage = contentType.ItemsPerPage,
      MultipleContentsTemplateId = contentType.MultipleContentsTemplateId,
      DefaultSingleContentTemplateId = contentType.DefaultSingleContentTemplateId,
      IsDefaultContentType = contentType.IsDefaultContentType
    };

    return CreatedAtAction(nameof(GetById), new { id = contentType.ContentTypeId }, resultDto);
  }

  // PUT: api/contenttypes/{id}
  [HttpPut("{id}")]
  public async Task<ActionResult<ContentTypeDetailDto>> Update(int id, [FromBody] ContentTypeUpdateDto dto)
  {
    _logger.LogDebug("API Update content type requested for: {ContentTypeId} - {ContentTypeName}", id, dto.Name);

    if (!ModelState.IsValid)
    {
      _logger.LogWarning("Model state invalid for content type update: {ContentTypeId}", id);
      return BadRequest(ModelState);
    }

    var contentType = await _contentTypeRepository.GetByIdAsync(id);
    if (contentType == null)
    {
      _logger.LogWarning("Content type not found for update with id: {ContentTypeId}", id);
      return NotFound();
    }

    contentType.Name = dto.Name;
    contentType.Title = dto.Title;
    contentType.Description = dto.Description;
    contentType.ItemsPerPage = dto.ItemsPerPage;
    contentType.MultipleContentsTemplateId = dto.MultipleContentsTemplateId;
    contentType.DefaultSingleContentTemplateId = dto.DefaultSingleContentTemplateId;
    contentType.IsDefaultContentType = dto.IsDefaultContentType;

    try
    {
      await _contentTypeRepository.UpdateAsync(contentType);
      _logger.LogInformation("Updated content type: {ContentTypeId} - {ContentTypeName}", contentType.ContentTypeId, contentType.Name);
    }
    catch (DbUpdateConcurrencyException ex)
    {
      _logger.LogError(ex, "Concurrency exception updating content type: {ContentTypeId}", contentType.ContentTypeId);
      if (!await _contentTypeRepository.ExistsAsync(contentType.ContentTypeId))
      {
        return NotFound();
      }
      else
      {
        throw;
      }
    }

    var resultDto = new ContentTypeDetailDto
    {
      ContentTypeId = contentType.ContentTypeId,
      Name = contentType.Name,
      Title = contentType.Title,
      Description = contentType.Description,
      ItemsPerPage = contentType.ItemsPerPage,
      MultipleContentsTemplateId = contentType.MultipleContentsTemplateId,
      DefaultSingleContentTemplateId = contentType.DefaultSingleContentTemplateId,
      IsDefaultContentType = contentType.IsDefaultContentType
    };

    return Ok(resultDto);
  }

  // DELETE: api/contenttypes/{id}
  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(int id)
  {
    _logger.LogDebug("API Delete requested for content type id: {ContentTypeId}", id);

    var contentType = await _contentTypeRepository.GetByIdAsync(id);
    if (contentType == null)
    {
      _logger.LogWarning("Content type not found for delete with id: {ContentTypeId}", id);
      return NotFound();
    }

    await _contentTypeRepository.DeleteAsync(contentType);
    _logger.LogInformation("Deleted content type: {ContentTypeId} - {ContentTypeName}", contentType.ContentTypeId, contentType.Name);

    return NoContent();
  }

  // GET: api/contenttypes/form-options
  [HttpGet("form-options")]
  public async Task<ActionResult<ContentTypeFormOptionsDto>> GetFormOptions()
  {
    _logger.LogDebug("API GetFormOptions requested");

    var multipleContentsTemplates = await _templateRepository.GetByIsForMultipleContentsAsync(1);
    var singleContentTemplates = await _templateRepository.GetByIsForMultipleContentsAsync(0);

    var dto = new ContentTypeFormOptionsDto
    {
      MultipleContentsTemplates = multipleContentsTemplates.Select(t => new LookupItemDto
      {
        Id = t.TemplateId,
        Name = t.Name ?? $"Template {t.TemplateId}"
      }).ToList(),
      SingleContentTemplates = new List<LookupItemDto>
      {
        new LookupItemDto { Id = 0, Name = "" }
      }
    };

    dto.SingleContentTemplates.AddRange(singleContentTemplates.Select(t => new LookupItemDto
    {
      Id = t.TemplateId,
      Name = t.Name ?? $"Template {t.TemplateId}"
    }));

    _logger.LogInformation("Returning form options with {MultiCount} multiple and {SingleCount} single templates",
      dto.MultipleContentsTemplates.Count, dto.SingleContentTemplates.Count);
    return Ok(dto);
  }
}
