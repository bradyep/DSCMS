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
public class TemplatesController : ControllerBase
{
  private readonly ITemplateRepository _templateRepository;
  private readonly ILayoutRepository _layoutRepository;
  private readonly ILogger<TemplatesController> _logger;

  public TemplatesController(
    ITemplateRepository templateRepository,
    ILayoutRepository layoutRepository,
    ILogger<TemplatesController> logger)
  {
    _templateRepository = templateRepository;
    _layoutRepository = layoutRepository;
    _logger = logger;
  }

  // GET: api/templates
  [HttpGet]
  public async Task<ActionResult<IEnumerable<TemplateListDto>>> GetAll()
  {
    _logger.LogDebug("API GetAll templates requested");

    var templates = await _templateRepository.GetAllWithLayoutAsync();

    var dtos = templates.Select(t => new TemplateListDto
    {
      TemplateId = t.TemplateId,
      Name = t.Name,
      TemplateSource = t.TemplateSource,
      LayoutName = t.Layout?.Name,
      IsForMultipleContents = t.IsForMultipleContents
    }).ToList();

    _logger.LogInformation("Returning {TemplateCount} templates", dtos.Count);
    return Ok(dtos);
  }

  // GET: api/templates/{id}
  [HttpGet("{id}")]
  public async Task<ActionResult<TemplateDetailDto>> GetById(int id)
  {
    _logger.LogDebug("API GetById requested for template id: {TemplateId}", id);

    var template = await _templateRepository.GetByIdWithLayoutAsync(id);
    if (template == null)
    {
      _logger.LogWarning("Template not found with id: {TemplateId}", id);
      return NotFound();
    }

    var dto = new TemplateDetailDto
    {
      TemplateId = template.TemplateId,
      Name = template.Name,
      TemplateSource = template.TemplateSource,
      SourceTypeId = template.SourceTypeId,
      IsForMultipleContents = template.IsForMultipleContents,
      LayoutId = template.LayoutId,
      LayoutName = template.Layout?.Name
    };

    _logger.LogDebug("Found template: {TemplateId} - {TemplateName}", template.TemplateId, template.Name);
    return Ok(dto);
  }

  // POST: api/templates
  [HttpPost]
  public async Task<ActionResult<TemplateDetailDto>> Create([FromBody] TemplateCreateDto dto)
  {
    _logger.LogDebug("API Create template requested for name: {TemplateName}", dto.Name);

    if (!ModelState.IsValid)
    {
      _logger.LogWarning("Model state invalid for template creation: {TemplateName}", dto.Name);
      return BadRequest(ModelState);
    }

    var template = new Template
    {
      Name = dto.Name,
      TemplateSource = dto.TemplateSource,
      SourceTypeId = dto.SourceTypeId,
      IsForMultipleContents = dto.IsForMultipleContents,
      LayoutId = dto.LayoutId
    };

    await _templateRepository.AddAsync(template);
    _logger.LogInformation("Created new template: {TemplateId} - {TemplateName}", template.TemplateId, template.Name);

    var resultDto = new TemplateDetailDto
    {
      TemplateId = template.TemplateId,
      Name = template.Name,
      TemplateSource = template.TemplateSource,
      SourceTypeId = template.SourceTypeId,
      IsForMultipleContents = template.IsForMultipleContents,
      LayoutId = template.LayoutId
    };

    return CreatedAtAction(nameof(GetById), new { id = template.TemplateId }, resultDto);
  }

  // PUT: api/templates/{id}
  [HttpPut("{id}")]
  public async Task<ActionResult<TemplateDetailDto>> Update(int id, [FromBody] TemplateUpdateDto dto)
  {
    _logger.LogDebug("API Update template requested for: {TemplateId} - {TemplateName}", id, dto.Name);

    if (!ModelState.IsValid)
    {
      _logger.LogWarning("Model state invalid for template update: {TemplateId}", id);
      return BadRequest(ModelState);
    }

    var template = await _templateRepository.GetByIdAsync(id);
    if (template == null)
    {
      _logger.LogWarning("Template not found for update with id: {TemplateId}", id);
      return NotFound();
    }

    template.Name = dto.Name;
    template.TemplateSource = dto.TemplateSource;
    template.SourceTypeId = dto.SourceTypeId;
    template.IsForMultipleContents = dto.IsForMultipleContents;
    template.LayoutId = dto.LayoutId;

    try
    {
      await _templateRepository.UpdateAsync(template);
      _logger.LogInformation("Updated template: {TemplateId} - {TemplateName}", template.TemplateId, template.Name);
    }
    catch (DbUpdateConcurrencyException ex)
    {
      _logger.LogError(ex, "Concurrency exception updating template: {TemplateId}", template.TemplateId);
      if (!await _templateRepository.ExistsAsync(template.TemplateId))
      {
        return NotFound();
      }
      else
      {
        throw;
      }
    }

    var resultDto = new TemplateDetailDto
    {
      TemplateId = template.TemplateId,
      Name = template.Name,
      TemplateSource = template.TemplateSource,
      SourceTypeId = template.SourceTypeId,
      IsForMultipleContents = template.IsForMultipleContents,
      LayoutId = template.LayoutId
    };

    return Ok(resultDto);
  }

  // DELETE: api/templates/{id}
  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(int id)
  {
    _logger.LogDebug("API Delete requested for template id: {TemplateId}", id);

    var template = await _templateRepository.GetByIdAsync(id);
    if (template == null)
    {
      _logger.LogWarning("Template not found for delete with id: {TemplateId}", id);
      return NotFound();
    }

    await _templateRepository.DeleteAsync(template);
    _logger.LogInformation("Deleted template: {TemplateId} - {TemplateName}", template.TemplateId, template.Name);

    return NoContent();
  }

  // GET: api/templates/form-options
  [HttpGet("form-options")]
  public async Task<ActionResult<TemplateFormOptionsDto>> GetFormOptions()
  {
    _logger.LogDebug("API GetFormOptions requested");

    var layouts = await _layoutRepository.GetAllAsync();

    var dto = new TemplateFormOptionsDto
    {
      Layouts = layouts.Select(l => new LookupItemDto
      {
        Id = l.LayoutId,
        Name = l.Name ?? $"Layout {l.LayoutId}"
      }).ToList()
    };

    _logger.LogInformation("Returning form options with {LayoutCount} layouts", dto.Layouts.Count);
    return Ok(dto);
  }
}


