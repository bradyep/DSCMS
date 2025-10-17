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
  public class ContentTypeFieldItemsController : Controller
  {
    private readonly ApplicationDbContext _context;

    public ContentTypeFieldItemsController(ApplicationDbContext context)
    {
      _context = context;
    }

    // GET: ContentTypeFieldItems
    public async Task<IActionResult> Index()
    {
      var applicationDbContext = _context.ContentTypeFieldItems.Include(c => c.Content).Include(c => c.ContentTypeField);
      return View(await applicationDbContext.ToListAsync());
    }

    // GET: ContentTypeFieldItems/Details/5
    public async Task<IActionResult> Details(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var contentTypeFieldItem = await _context.ContentTypeFieldItems.SingleOrDefaultAsync(m => m.ContentTypeFieldItemId == id);
      if (contentTypeFieldItem == null)
      {
        return NotFound();
      }

      return View(contentTypeFieldItem);
    }

    // GET: ContentTypeFieldItems/Create
    public IActionResult Create(int? id=0)
    {
      Content content;
      if (id > 0)
      {
        content = _context.Contents.Include(c => c.ContentType).Where(c => c.ContentId == id).FirstOrDefault();
        ViewData["ContentId"] = new SelectList(_context.Contents, "ContentId", "UrlToDisplay", id);
        ViewData["ContentTypeFieldId"] = new SelectList(_context.ContentTypeFields.Where(c => c.ContentTypeId == content.ContentTypeId), "ContentTypeFieldId", "Name");
      }
      else
      {
        ViewData["ContentId"] = new SelectList(_context.Contents, "ContentId", "UrlToDisplay");
        ViewData["ContentTypeFieldId"] = new SelectList(_context.ContentTypeFields, "ContentTypeFieldId", "Name");
      }
      return View();
    }

    // POST: ContentTypeFieldItems/Create
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ContentTypeFieldItemId,ContentId,ContentTypeFieldId,Value")] ContentTypeFieldItem contentTypeFieldItem)
    {
      Content content = _context.Contents.Where(x => x.ContentId == contentTypeFieldItem.ContentId).FirstOrDefault();

      if (ModelState.IsValid)
      {
        _context.Add(contentTypeFieldItem);
        await _context.SaveChangesAsync();

        // return RedirectToAction("Index");
        // Redirect to Contents
        return RedirectToAction("Edit", "Contents", new { id = content.ContentId });
      }
      ViewData["ContentId"] = new SelectList(_context.Contents, "ContentId", "ContentId", contentTypeFieldItem.ContentId);
      ViewData["ContentTypeFieldId"] = new SelectList(_context.ContentTypeFields, "ContentTypeFieldId", "ContentTypeFieldId", contentTypeFieldItem.ContentTypeFieldId);
      return View(contentTypeFieldItem);
    }

    // GET: ContentTypeFieldItems/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var contentTypeFieldItem = await _context.ContentTypeFieldItems.SingleOrDefaultAsync(m => m.ContentTypeFieldItemId == id);
      if (contentTypeFieldItem == null)
      {
        return NotFound();
      }
      ViewData["ContentId"] = new SelectList(_context.Contents, "ContentId", "UrlToDisplay", contentTypeFieldItem.ContentId);
      ViewData["ContentTypeFieldId"] = new SelectList(_context.ContentTypeFields, "ContentTypeFieldId", "Name", contentTypeFieldItem.ContentTypeFieldId);
      return View(contentTypeFieldItem);
    }

    // POST: ContentTypeFieldItems/Edit/5
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("ContentTypeFieldItemId,ContentId,ContentTypeFieldId,Value")] ContentTypeFieldItem contentTypeFieldItem)
    {
      if (id != contentTypeFieldItem.ContentTypeFieldItemId)
      {
        return NotFound();
      }

      if (ModelState.IsValid)
      {
        try
        {
          _context.Update(contentTypeFieldItem);
          await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
          if (!ContentTypeFieldItemExists(contentTypeFieldItem.ContentTypeFieldItemId))
          {
            return NotFound();
          }
          else
          {
            throw;
          }
        }
        // return RedirectToAction("Index");
        // Take them to the edit for the parent Content
        return RedirectToAction("Edit", "Contents", new { id = contentTypeFieldItem.ContentId });
      }
      ViewData["ContentId"] = new SelectList(_context.Contents, "ContentId", "UrlToDisplay", contentTypeFieldItem.ContentId);
      ViewData["ContentTypeFieldId"] = new SelectList(_context.ContentTypeFields, "ContentTypeFieldId", "Name", contentTypeFieldItem.ContentTypeFieldId);
      return View(contentTypeFieldItem);
    }

    // GET: ContentTypeFieldItems/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var contentTypeFieldItem = await _context.ContentTypeFieldItems.SingleOrDefaultAsync(m => m.ContentTypeFieldItemId == id);
      if (contentTypeFieldItem == null)
      {
        return NotFound();
      }

      return View(contentTypeFieldItem);
    }

    // POST: ContentTypeFieldItems/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      ContentTypeFieldItem fieldItem = _context.ContentTypeFieldItems.Include(x => x.Content).Where(x => x.ContentTypeFieldItemId == id).FirstOrDefault();

      var contentTypeFieldItem = await _context.ContentTypeFieldItems.SingleOrDefaultAsync(m => m.ContentTypeFieldItemId == id);
      _context.ContentTypeFieldItems.Remove(contentTypeFieldItem);
      await _context.SaveChangesAsync();
      // return RedirectToAction("Index");
      return RedirectToAction("Edit", "Contents", new { id = fieldItem.Content.ContentId });
    }

    private bool ContentTypeFieldItemExists(int id)
    {
      return _context.ContentTypeFieldItems.Any(e => e.ContentTypeFieldItemId == id);
    }
  }
}
