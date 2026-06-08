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
public class LayoutsController : ControllerBase
{
  private readonly ILayoutRepository _layoutRepository;
  private readonly ILogger<LayoutsController> _logger;

  public LayoutsController(ILayoutRepository layoutRepository, ILogger<LayoutsController> logger)
  {
    _layoutRepository = layoutRepository;
    _logger = logger;
  }

  // GET: api/layouts
  [HttpGet]
  public async Task<ActionResult<IEnumerable<LayoutListDto>>> GetAll()
  {
    _logger.LogDebug("API GetAll layouts requested");

    var layouts = await _layoutRepository.GetAllAsync();

    // Inline FixNullLayouts logic
    bool hasChanges = false;
    foreach (var layout in layouts)
    {
      if (string.IsNullOrEmpty(layout.Name))
      {
        layout.Name = $"Layout {layout.LayoutId}";
        hasChanges = true;
      }

      if (string.IsNullOrEmpty(layout.LayoutSource))
      {
        layout.LayoutSource = "/Views/DSCMS/Layouts/_BootstrapBlog.cshtml";
        hasChanges = true;
      }
    }

    if (hasChanges)
    {
      foreach (var layout in layouts)
      {
        await _layoutRepository.UpdateAsync(layout);
      }
      layouts = await _layoutRepository.GetAllAsync();
    }

    var dtos = layouts.Select(l => new LayoutListDto
    {
      LayoutId = l.LayoutId,
      Name = l.Name,
      LayoutSource = l.LayoutSource,
      SourceTypeName = l.SourceType?.Description
    }).ToList();

    _logger.LogInformation("Returning {LayoutCount} layouts", dtos.Count);
    return Ok(dtos);
  }

  // GET: api/layouts/{id}
  [HttpGet("{id}")]
  public async Task<ActionResult<LayoutDetailDto>> GetById(int id)
  {
    _logger.LogDebug("API GetById requested for layout id: {LayoutId}", id);

    var layout = await _layoutRepository.GetByIdAsync(id);
    if (layout == null)
    {
      _logger.LogWarning("Layout not found with id: {LayoutId}", id);
      return NotFound();
    }

    var dto = new LayoutDetailDto
    {
      LayoutId = layout.LayoutId,
      Name = layout.Name,
      LayoutSource = layout.LayoutSource,
      SourceTypeId = layout.SourceTypeId,
      SourceTypeName = layout.SourceType?.Description
    };

    _logger.LogDebug("Found layout: {LayoutId} - {LayoutName}", layout.LayoutId, layout.Name);
    return Ok(dto);
  }

  // POST: api/layouts
  [HttpPost]
  public async Task<ActionResult<LayoutDetailDto>> Create([FromBody] LayoutCreateDto dto)
  {
    _logger.LogDebug("API Create layout requested for name: {LayoutName}", dto.Name);

    if (!ModelState.IsValid)
    {
      _logger.LogWarning("Model state invalid for layout creation: {LayoutName}", dto.Name);
      return BadRequest(ModelState);
    }

    var layout = new Layout
    {
      Name = dto.Name,
      LayoutSource = dto.LayoutSource,
      SourceTypeId = dto.SourceTypeId
    };

    await _layoutRepository.AddAsync(layout);
    _logger.LogInformation("Created new layout: {LayoutId} - {LayoutName}", layout.LayoutId, layout.Name);

    var resultDto = new LayoutDetailDto
    {
      LayoutId = layout.LayoutId,
      Name = layout.Name,
      LayoutSource = layout.LayoutSource,
      SourceTypeId = layout.SourceTypeId
    };

    return CreatedAtAction(nameof(GetById), new { id = layout.LayoutId }, resultDto);
  }

  // PUT: api/layouts/{id}
  [HttpPut("{id}")]
  public async Task<ActionResult<LayoutDetailDto>> Update(int id, [FromBody] LayoutUpdateDto dto)
  {
    _logger.LogDebug("API Update layout requested for: {LayoutId} - {LayoutName}", id, dto.Name);

    if (!ModelState.IsValid)
    {
      _logger.LogWarning("Model state invalid for layout update: {LayoutId}", id);
      return BadRequest(ModelState);
    }

    var layout = await _layoutRepository.GetByIdAsync(id);
    if (layout == null)
    {
      _logger.LogWarning("Layout not found for update with id: {LayoutId}", id);
      return NotFound();
    }

    layout.Name = dto.Name;
    layout.LayoutSource = dto.LayoutSource;
    layout.SourceTypeId = dto.SourceTypeId;

    try
    {
      await _layoutRepository.UpdateAsync(layout);
      _logger.LogInformation("Updated layout: {LayoutId} - {LayoutName}", layout.LayoutId, layout.Name);
    }
    catch (DbUpdateConcurrencyException ex)
    {
      _logger.LogError(ex, "Concurrency exception updating layout: {LayoutId}", layout.LayoutId);
      if (!await _layoutRepository.ExistsAsync(layout.LayoutId))
      {
        return NotFound();
      }
      else
      {
        throw;
      }
    }

    var resultDto = new LayoutDetailDto
    {
      LayoutId = layout.LayoutId,
      Name = layout.Name,
      LayoutSource = layout.LayoutSource,
      SourceTypeId = layout.SourceTypeId
    };

    return Ok(resultDto);
  }

  // DELETE: api/layouts/{id}
  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(int id)
  {
    _logger.LogDebug("API Delete requested for layout id: {LayoutId}", id);

    var layout = await _layoutRepository.GetByIdAsync(id);
    if (layout == null)
    {
      _logger.LogWarning("Layout not found for delete with id: {LayoutId}", id);
      return NotFound();
    }

    await _layoutRepository.DeleteAsync(layout);
    _logger.LogInformation("Deleted layout: {LayoutId} - {LayoutName}", layout.LayoutId, layout.Name);

    return NoContent();
  }
}


