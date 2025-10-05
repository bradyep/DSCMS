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
using Microsoft.Extensions.Logging;

namespace DSCMS.Controllers
{
  [Authorize]
  public class ContentsController : Controller
  {
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ContentsController> _logger;

    public ContentsController(ApplicationDbContext context, ILogger<ContentsController> logger)
    {
      _context = context;
      _logger = logger;
    }

    // GET: Contents
    public async Task<IActionResult> Index(string contentType)
    {
      _logger.LogDebug("Contents Index requested with contentType filter: {ContentType}", contentType);

      var contents = String.IsNullOrEmpty(contentType) ? 
        _context.Contents.Include(c => c.ContentType).Include(c => c.CreatedByUser).Include(c => c.LastUpdatedByUser).Include(c => c.Template).Include(c => c.ContentItems) :
        _context.Contents.Include(c => c.ContentType).Include(c => c.CreatedByUser).Include(c => c.LastUpdatedByUser).Include(c => c.Template).Include(c => c.ContentItems)
          .Where(c => c.ContentType.Name == contentType);

      List<ContentType> cts = new List<ContentType>();
      cts.Add(new ContentType { Name = "" });
      cts.AddRange(_context.ContentTypes.ToList());
      var ctSelectList = new SelectList(cts, "Name", "Name", contentType);
      ViewData["ContentType"] = ctSelectList;

      var result = await contents.ToListAsync();
      _logger.LogInformation("Returning {ContentCount} contents for type '{ContentType}'", result.Count, contentType ?? "all");

      return View(result);
    }

    // GET: Contents/Details/5
    public async Task<IActionResult> Details(int? id)
    {
      if (id == null)
      {
        _logger.LogWarning("Contents Details requested with null id");
        return NotFound();
      }

      _logger.LogDebug("Contents Details requested for id: {ContentId}", id);

      var content = await _context.Contents.SingleOrDefaultAsync(m => m.ContentId == id);
      if (content == null)
      {
        _logger.LogWarning("Content not found with id: {ContentId}", id);
        return NotFound();
      }

      _logger.LogDebug("Found content: {ContentId} - {ContentTitle}", content.ContentId, content.Title);
      return View(content);
    }

    // GET: Contents/Create
    public IActionResult Create()
    {
      _logger.LogDebug("Contents Create form requested");

      var allContentTypes = _context.ContentTypes.ToList();
      var ctSelectList = new SelectList(allContentTypes, "ContentTypeId", "Name");
      ViewData["ContentTypeId"] = ctSelectList;

      ViewData["CreatedBy"] = new SelectList(_context.Users, "Id", "DisplayName");
      ViewData["LastUpdatedBy"] = new SelectList(_context.Users, "Id", "DisplayName");
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
      _logger.LogDebug("Contents Create POST received for title: {ContentTitle}", content.Title);

      if (ModelState.IsValid)
      {
        content.CreationDate = DateTime.Now;
        content.LastUpdatedDate = DateTime.Now;
        _context.Add(content);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created new content: {ContentId} - {ContentTitle}", content.ContentId, content.Title);
        return RedirectToAction("Index");
      }

      _logger.LogWarning("Model state invalid for content creation: {ContentTitle}", content.Title);
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
        _logger.LogWarning("Contents Edit requested with null id");
        return NotFound();
      }

      _logger.LogDebug("Contents Edit requested for id: {ContentId}", id);

      var content = await _context.Contents
        .Include(x => x.ContentItems)
        .ThenInclude(x => x.ContentTypeItem)
        .SingleOrDefaultAsync(m => m.ContentId == id);
      if (content == null)
      {
        _logger.LogWarning("Content not found for edit with id: {ContentId}", id);
        return NotFound();
      }

      _logger.LogDebug("Loading edit form for content: {ContentId} - {ContentTitle}", content.ContentId, content.Title);
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
        _logger.LogWarning("Content Edit POST received with mismatched id: {UrlId} vs {ContentId}", id, content.ContentId);
        return NotFound();
      }

      _logger.LogDebug("Contents Edit POST received for: {ContentId} - {ContentTitle}", content.ContentId, content.Title);

      if (ModelState.IsValid)
      {
        content.LastUpdatedDate = DateTime.Now;
        try
        {
          _context.Update(content);
          await _context.SaveChangesAsync();
          _logger.LogInformation("Updated content: {ContentId} - {ContentTitle}", content.ContentId, content.Title);
        }
        catch (DbUpdateConcurrencyException ex)
        {
          _logger.LogError(ex, "Concurrency exception updating content: {ContentId}", content.ContentId);
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

      _logger.LogWarning("Model state invalid for content edit: {ContentId} - {ContentTitle}", content.ContentId, content.Title);
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
        _logger.LogWarning("Contents Delete requested with null id");
        return NotFound();
      }

      _logger.LogDebug("Contents Delete requested for id: {ContentId}", id);

      var content = await _context.Contents.SingleOrDefaultAsync(m => m.ContentId == id);
      if (content == null)
      {
        _logger.LogWarning("Content not found for delete with id: {ContentId}", id);
        return NotFound();
      }

      _logger.LogDebug("Loading delete confirmation for content: {ContentId} - {ContentTitle}", content.ContentId, content.Title);
      return View(content);
    }

    // POST: Contents/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      _logger.LogDebug("Contents Delete POST confirmed for id: {ContentId}", id);

      var content = await _context.Contents.SingleOrDefaultAsync(m => m.ContentId == id);
      if (content != null)
      {
        _context.Contents.Remove(content);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Deleted content: {ContentId} - {ContentTitle}", content.ContentId, content.Title);
      }
      else
      {
        _logger.LogWarning("Attempted to delete non-existent content with id: {ContentId}", id);
      }

      return RedirectToAction("Index");
    }

    private bool ContentExists(int id)
    {
      return _context.Contents.Any(e => e.ContentId == id);
    }
  }
}
