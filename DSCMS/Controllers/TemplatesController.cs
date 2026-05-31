using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DSCMS.Models;
using DSCMS.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace DSCMS.Controllers
{
  [Authorize]
  public class TemplatesController : Controller
  {
    private readonly ITemplateRepository _templateRepository;
    private readonly ILayoutRepository _layoutRepository;

    public TemplatesController(ITemplateRepository templateRepository, ILayoutRepository layoutRepository)
    {
      _templateRepository = templateRepository;
      _layoutRepository = layoutRepository;
    }

    // GET: Templates
    public async Task<IActionResult> Index()
    {
      return View(await _templateRepository.GetAllWithLayoutAsync());
    }

    // GET: Templates/Details/5
    public async Task<IActionResult> Details(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var template = await _templateRepository.GetByIdWithLayoutAsync(id.Value);
      if (template == null)
      {
        return NotFound();
      }

      return View(template);
    }

    /// <summary>
    /// Displays the Create Template view
    /// </summary>
    /// <returns></returns>
    // GET: Templates/Create
    public async Task<IActionResult> Create()
    {
      var layouts = await _layoutRepository.GetAllAsync();
      var layoutItems = layouts.Select(l => new { LayoutId = l.LayoutId, Name = l.Name ?? $"Layout {l.LayoutId}" }).ToList();
      ViewData["LayoutId"] = new SelectList(layoutItems, "LayoutId", "Name");

      var types = new[]
      {
        new { Name = "Content", Value = 0 },
        new { Name = "ContentType", Value = 1 }
      };
      ViewData["Types"] = new SelectList(types, "Value", "Name");

      return View();
    }

    /// <summary>
    /// Processes the Create Template form submission
    /// </summary>
    /// <param name="template"></param>
    /// <returns></returns>
    // POST: Templates/Create
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("TemplateId,TemplateSource,TemplateSource,LayoutId,Name,IsForMultipleContents")] Template template)
    {
      if (ModelState.IsValid)
      {
        await _templateRepository.AddAsync(template);
        return RedirectToAction("Index");
      }

      var layouts = await _layoutRepository.GetAllAsync();
      var layoutItems = layouts.Select(l => new { LayoutId = l.LayoutId, Name = l.Name ?? $"Layout {l.LayoutId}" }).ToList();
      ViewData["LayoutId"] = new SelectList(layoutItems, "LayoutId", "Name", template.LayoutId);

      var types = new[]
      {
        new { Name = "Content", Value = 0 },
        new { Name = "ContentType", Value = 1 }
      };
      ViewData["Types"] = new SelectList(types, "Value", "Name", Convert.ToInt32(template.IsForMultipleContents));

      return View(template);
    }

    // GET: Templates/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var template = await _templateRepository.GetByIdAsync(id.Value);
      if (template == null)
      {
        return NotFound();
      }

      var layouts = await _layoutRepository.GetAllAsync();
      var layoutItems = layouts.Select(l => new { LayoutId = l.LayoutId, Name = l.Name ?? $"Layout {l.LayoutId}" }).ToList();
      ViewData["LayoutId"] = new SelectList(layoutItems, "LayoutId", "Name", template.LayoutId);

      var types = new[]
      {
        new { Name = "Content", Value = 0 },
        new { Name = "ContentType", Value = 1 }
      };
      ViewData["Types"] = new SelectList(types, "Value", "Name", Convert.ToInt32(template.IsForMultipleContents));
      return View(template);
    }

    // POST: Templates/Edit/5
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("TemplateId,TemplateSource,TemplateSource,LayoutId,Name,IsForMultipleContents")] Template template)
    {
      if (id != template.TemplateId)
      {
        return NotFound();
      }

      if (ModelState.IsValid)
      {
        try
        {
          await _templateRepository.UpdateAsync(template);
        }
        catch (DbUpdateConcurrencyException)
        {
          if (!await _templateRepository.ExistsAsync(template.TemplateId))
          {
            return NotFound();
          }
          else
          {
            throw;
          }
        }
        return RedirectToAction("Index");
      }

      var layouts = await _layoutRepository.GetAllAsync();
      var layoutItems = layouts.Select(l => new { LayoutId = l.LayoutId, Name = l.Name ?? $"Layout {l.LayoutId}" }).ToList();
      ViewData["LayoutId"] = new SelectList(layoutItems, "LayoutId", "Name", template.LayoutId);

      var types = new[]
      {
        new { Name = "Content", Value = 0 },
        new { Name = "ContentType", Value = 1 }
      };
      ViewData["Types"] = new SelectList(types, "Value", "Name", Convert.ToInt32(template.IsForMultipleContents));
      return View(template);
    }

    // GET: Templates/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var template = await _templateRepository.GetByIdWithLayoutAsync(id.Value);
      if (template == null)
      {
        return NotFound();
      }

      return View(template);
    }

    // POST: Templates/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      var template = await _templateRepository.GetByIdAsync(id);
      if (template != null)
      {
        await _templateRepository.DeleteAsync(template);
      }
      return RedirectToAction("Index");
    }
  }
}


