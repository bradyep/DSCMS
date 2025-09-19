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
  public class ContentsController : Controller
  {
    private readonly ApplicationDbContext _context;

    public ContentsController(ApplicationDbContext context)
    {
      _context = context;
    }

    // GET: Contents
    public async Task<IActionResult> Index(string contentType)
    {
      var contents = String.IsNullOrEmpty(contentType) ? 
        _context.Contents.Include(c => c.ContentType).Include(c => c.CreatedByUser).Include(c => c.LastUpdatedByUser).Include(c => c.Template).Include(c => c.ContentItems) :
        _context.Contents.Include(c => c.ContentType).Include(c => c.CreatedByUser).Include(c => c.LastUpdatedByUser).Include(c => c.Template).Include(c => c.ContentItems)
          .Where(c => c.ContentType.Name == contentType);

      List<ContentType> cts = new List<ContentType>();
      cts.Add(new ContentType { Name = "" });
      cts.AddRange(_context.ContentTypes.ToList());
      var ctSelectList = new SelectList(cts, "Name", "Name", contentType);
      ViewData["ContentType"] = ctSelectList;

      return View(await contents.ToListAsync());
    }

    // GET: Contents/Details/5
    public async Task<IActionResult> Details(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var content = await _context.Contents.SingleOrDefaultAsync(m => m.ContentId == id);
      if (content == null)
      {
        return NotFound();
      }

      return View(content);
    }

    // GET: Contents/Create
    public IActionResult Create()
    {
      // var contentTypes = new List<ContentType>();
      // templates.Add(new Template { Name = "", TemplateId = 0 });
      var allContentTypes = _context.ContentTypes.ToList();
      // contentTypes.AddRange(allContentTypes);
      var ctSelectList = new SelectList(allContentTypes, "ContentTypeId", "Name");
      ViewData["ContentTypeId"] = ctSelectList;

      // ViewData["ContentTypeId"] = new SelectList(_context.ContentTypes, "ContentTypeId", "Name");

      ViewData["CreatedBy"] = new SelectList(_context.Users, "Id", "DisplayName");
      ViewData["LastUpdatedBy"] = new SelectList(_context.Users, "Id", "DisplayName");

      /*
      List<Template> templates = new List<Template>();
      // templates.Add(new Template { Name = "", TemplateId = 0 });
      templates.AddRange(_context.Templates.Where(t => t.IsForContentType == 0).ToList());
      var tsSelectList = new SelectList(templates, "TemplateId", "Name");
      ViewData["TemplateId"] = tsSelectList;
      */

      ViewData["TemplateId"] = new SelectList(_context.Templates.Where(t => t.IsForContentType == 0), "TemplateId", "Name");

      // Put together a Dictionary of all ContentTypes and their DefaultTemplateForContent (if they have one)
      var contentTypeDefaultTemplateLookup = new Dictionary<int, int>();
      var contentTypesWithDefaultTemplates = allContentTypes.Where(ct => ct.DefaultTemplateForContent != null).ToList();
      foreach (var item in contentTypesWithDefaultTemplates)
      {
        contentTypeDefaultTemplateLookup.Add(item.ContentTypeId, item.DefaultTemplateForContent ?? 0);
      }
      ViewData["DefaultTemplateLookup"] = contentTypeDefaultTemplateLookup;

      return View();
    }

    // POST: Contents/Create
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ContentId,Body,ContentTypeId,CreatedBy,LastUpdatedBy,TemplateId,Title,UrlToDisplay")] Content content)
    {
      if (ModelState.IsValid)
      {
        content.CreationDate = DateTime.Now;
        content.LastUpdatedDate = DateTime.Now;
        _context.Add(content);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
      }
      ViewData["ContentTypeId"] = new SelectList(_context.ContentTypes, "ContentTypeId", "Name", content.ContentTypeId);
      ViewData["CreatedBy"] = new SelectList(_context.Users, "Id", "DisplayName", content.CreatedBy);
      ViewData["LastUpdatedBy"] = new SelectList(_context.Users, "Id", "DisplayName", content.LastUpdatedBy);
      ViewData["TemplateId"] = new SelectList(_context.Templates, "TemplateId", "Name", content.TemplateId);
      return View(content);
    }

    // GET: Contents/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var content = await _context.Contents
        .Include(x => x.ContentItems)
        .ThenInclude(x => x.ContentTypeItem)
        .SingleOrDefaultAsync(m => m.ContentId == id);
      if (content == null)
      {
        return NotFound();
      }
      ViewData["ContentTypeId"] = new SelectList(_context.ContentTypes, "ContentTypeId", "Name", content.ContentTypeId);
      ViewData["CreatedBy"] = new SelectList(_context.Users, "Id", "DisplayName", content.CreatedBy);
      ViewData["LastUpdatedBy"] = new SelectList(_context.Users, "Id", "DisplayName", content.LastUpdatedBy);
      ViewData["TemplateId"] = new SelectList(_context.Templates.Where(t => t.IsForContentType == 0), "TemplateId", "Name", content.TemplateId);
      return View(content);
    }

    // POST: Contents/Edit/5
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("ContentId,Body,ContentTypeId,CreatedBy,CreationDate,LastUpdatedBy,LastUpdatedDate,TemplateId,Title,UrlToDisplay")] Content content)
    {
      if (id != content.ContentId)
      {
        return NotFound();
      }

      if (ModelState.IsValid)
      {
        content.LastUpdatedDate = DateTime.Now;
        try
        {
          _context.Update(content);
          await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
          if (!ContentExists(content.ContentId))
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
      ViewData["ContentTypeId"] = new SelectList(_context.ContentTypes, "ContentTypeId", "Name", content.ContentTypeId);
      ViewData["CreatedBy"] = new SelectList(_context.Users, "Id", "DisplayName", content.CreatedBy);
      ViewData["LastUpdatedBy"] = new SelectList(_context.Users, "Id", "DisplayName", content.LastUpdatedBy);
      ViewData["TemplateId"] = new SelectList(_context.Templates, "TemplateId", "Name", content.TemplateId);
      return View(content);
    }

    // GET: Contents/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var content = await _context.Contents.SingleOrDefaultAsync(m => m.ContentId == id);
      if (content == null)
      {
        return NotFound();
      }

      return View(content);
    }

    // POST: Contents/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      var content = await _context.Contents.SingleOrDefaultAsync(m => m.ContentId == id);
      _context.Contents.Remove(content);
      await _context.SaveChangesAsync();
      return RedirectToAction("Index");
    }

    private bool ContentExists(int id)
    {
      return _context.Contents.Any(e => e.ContentId == id);
    }
  }
}
