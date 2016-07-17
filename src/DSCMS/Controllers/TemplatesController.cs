using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DSCMS.Data;
using DSCMS.Models;
using Microsoft.AspNetCore.Authorization;

namespace DSCMS.Controllers
{
  [Authorize]
  public class TemplatesController : Controller
  {
    private readonly ApplicationDbContext _context;

    public TemplatesController(ApplicationDbContext context)
    {
      _context = context;
    }

    // GET: Templates
    public async Task<IActionResult> Index()
    {
      var applicationDbContext = _context.Templates.Include(t => t.Layout);
      return View(await applicationDbContext.ToListAsync());
    }

    // GET: Templates/Details/5
    public async Task<IActionResult> Details(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var template = await _context.Templates.Include(t => t.Layout).SingleOrDefaultAsync(m => m.TemplateId == id);
      if (template == null)
      {
        return NotFound();
      }

      return View(template);
    }

    // GET: Templates/Create
    public IActionResult Create()
    {
      ViewData["LayoutId"] = new SelectList(_context.Layouts, "LayoutId", "Name");
      return View();
    }

    // POST: Templates/Create
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("TemplateId,FileContents,FileLocation,LayoutId,Name,IsForContentType")] Template template)
    {
      if (ModelState.IsValid)
      {
        _context.Add(template);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
      }
      ViewData["LayoutId"] = new SelectList(_context.Layouts, "LayoutId", "Name", template.LayoutId);
      return View(template);
    }

    // GET: Templates/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var template = await _context.Templates.SingleOrDefaultAsync(m => m.TemplateId == id);
      // var template = await _context.Templates.Include(t => t.Layout).SingleOrDefaultAsync(m => m.TemplateId == id);
      if (template == null)
      {
        return NotFound();
      }
      ViewData["LayoutId"] = new SelectList(_context.Layouts, "LayoutId", "Name", template.LayoutId);
      var types = new[]
      {
        new { Name = "Content", Value = 0 },
        new { Name = "ContentType", Value = 1 }
      };
      ViewData["Types"] = new SelectList(types, "Value", "Name", Convert.ToInt32(template.IsForContentType));
      return View(template);
    }

    // POST: Templates/Edit/5
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("TemplateId,FileContents,FileLocation,LayoutId,Name,IsForContentType")] Template template)
    {
      if (id != template.TemplateId)
      {
        return NotFound();
      }

      if (ModelState.IsValid)
      {
        try
        {
          _context.Update(template);
          await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
          if (!TemplateExists(template.TemplateId))
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
      ViewData["LayoutId"] = new SelectList(_context.Layouts, "LayoutId", "LayoutId", template.LayoutId);
      return View(template);
    }

    // GET: Templates/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var template = await _context.Templates.Include(t => t.Layout).SingleOrDefaultAsync(m => m.TemplateId == id);
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
      var template = await _context.Templates.SingleOrDefaultAsync(m => m.TemplateId == id);
      _context.Templates.Remove(template);
      await _context.SaveChangesAsync();
      return RedirectToAction("Index");
    }

    private bool TemplateExists(int id)
    {
      return _context.Templates.Any(e => e.TemplateId == id);
    }
  }
}
