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
  public class ContentTypesController : Controller
  {
    private readonly ApplicationDbContext _context;

    public ContentTypesController(ApplicationDbContext context)
    {
      _context = context;
    }

    // GET: ContentTypes
    public async Task<IActionResult> Index()
    {
      var applicationDbContext = _context.ContentTypes.Include(c => c.MultipleContentsTemplate);
      return View(await applicationDbContext.ToListAsync());
    }

    // GET: ContentTypes/Details/5
    public async Task<IActionResult> Details(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var contentType = await _context.ContentTypes.Include(ct => ct.MultipleContentsTemplate).SingleOrDefaultAsync(m => m.ContentTypeId == id);
      if (contentType == null)
      {
        return NotFound();
      }

      return View(contentType);
    }

    // GET: ContentTypes/Create
    public IActionResult Create()
    {
      // ViewData for Multiple Contents Template (for ContentType listings)
      ViewData["MultipleContentsTemplateId"] = new SelectList(_context.Templates.Where(t => t.IsForMultipleContents == 1), "TemplateId", "Name");

      // ViewData for Default Single Content Template
      List<Template> ts = new List<Template>();
      ts.Add(new Template { Name = "", TemplateId = 0 });
      ts.AddRange(_context.Templates.Where(t => t.IsForMultipleContents == 0).ToList());
      var tsSelectList = new SelectList(ts, "TemplateId", "Name", ts);
      ViewData["DefaultSingleContentTemplateId"] = tsSelectList;
      
      return View();
    }

    // POST: ContentTypes/Create
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ContentTypeId,Description,Name,MultipleContentsTemplateId,DefaultSingleContentTemplateId,Title,ItemsPerPage,IsDefaultContentType")] ContentType contentType)
    {
      if (contentType.DefaultSingleContentTemplateId < 1) contentType.DefaultSingleContentTemplateId = null;
      if (ModelState.IsValid)
      {
        _context.Add(contentType);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
      }
      ViewData["MultipleContentsTemplateId"] = new SelectList(_context.Templates.Where(t => t.IsForMultipleContents == 1), "TemplateId", "Name", contentType.MultipleContentsTemplateId);

      // Figure out the default single content template ID to use
      int defaultSingleTemplateIdToUse = contentType.DefaultSingleContentTemplateId ?? 0;

      List <Template> ts = new List<Template>();
      ts.Add(new Template { Name = "", TemplateId = 0 });
      ts.AddRange(_context.Templates.Where(t => t.IsForMultipleContents == 0).ToList());
      var tsSelectList = new SelectList(ts, "TemplateId", "Name", defaultSingleTemplateIdToUse);
      ViewData["DefaultSingleContentTemplateId"] = tsSelectList;
      
      return View(contentType);
    }

    // GET: ContentTypes/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var contentType = await _context.ContentTypes.Include(ct => ct.ContentTypeFields).SingleOrDefaultAsync(m => m.ContentTypeId == id);
      if (contentType == null)
      {
        return NotFound();
      }
      ViewData["MultipleContentsTemplateId"] = new SelectList(_context.Templates.Where(t => t.IsForMultipleContents == 1), "TemplateId", "Name", contentType.MultipleContentsTemplateId);

      List<Template> ts = new List<Template>();
      ts.Add(new Template { Name = "", TemplateId = 0 });
      ts.AddRange(_context.Templates.Where(t => t.IsForMultipleContents == 0).ToList());
      var tsSelectList = new SelectList(ts, "TemplateId", "Name", contentType.DefaultSingleContentTemplateId);
      ViewData["DefaultSingleContentTemplateId"] = tsSelectList;

      return View(contentType);
    }

    // POST: ContentTypes/Edit/5
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("ContentTypeId,Description,Name,MultipleContentsTemplateId,Title,ItemsPerPage,DefaultSingleContentTemplateId,IsDefaultContentType")] ContentType contentType)
    {
      if (id != contentType.ContentTypeId)
      {
        return NotFound();
      }

      if (ModelState.IsValid)
      {
        try
        {
          _context.Update(contentType);
          await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
          if (!ContentTypeExists(contentType.ContentTypeId))
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
      ViewData["MultipleContentsTemplateId"] = new SelectList(_context.Templates, "TemplateId", "Name", contentType.MultipleContentsTemplateId);
      return View(contentType);
    }

    // GET: ContentTypes/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var contentType = await _context.ContentTypes.Include(ct => ct.MultipleContentsTemplate).SingleOrDefaultAsync(m => m.ContentTypeId == id);
      if (contentType == null)
      {
        return NotFound();
      }

      return View(contentType);
    }

    // POST: ContentTypes/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      var contentType = await _context.ContentTypes.SingleOrDefaultAsync(m => m.ContentTypeId == id);
      _context.ContentTypes.Remove(contentType);
      await _context.SaveChangesAsync();
      return RedirectToAction("Index");
    }

    private bool ContentTypeExists(int id)
    {
      return _context.ContentTypes.Any(e => e.ContentTypeId == id);
    }
  }
}
