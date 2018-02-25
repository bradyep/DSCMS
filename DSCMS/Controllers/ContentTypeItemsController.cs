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
  public class ContentTypeItemsController : Controller
  {
    private readonly ApplicationDbContext _context;

    public ContentTypeItemsController(ApplicationDbContext context)
    {
      _context = context;
    }

    // GET: ContentTypeItems
    public async Task<IActionResult> Index()
    {
      var applicationDbContext = _context.ContentTypeItems.Include(c => c.ContentType);
      return View(await applicationDbContext.ToListAsync());
    }

    // GET: ContentTypeItems/Details/5
    public async Task<IActionResult> Details(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var contentTypeItem = await _context.ContentTypeItems.SingleOrDefaultAsync(m => m.ContentTypeItemId == id);
      if (contentTypeItem == null)
      {
        return NotFound();
      }

      return View(contentTypeItem);
    }

    // GET: ContentTypeItems/Create
    public IActionResult Create(int? id)
    {
      if (id > 0)
      {
        ViewData["ContentTypeId"] = new SelectList(_context.ContentTypes.Where(ct => ct.ContentTypeId == id), "ContentTypeId", "Name", id);
      }
      else
      {
        ViewData["ContentTypeId"] = new SelectList(_context.ContentTypes, "ContentTypeId", "Name");
      }

      return View();
    }

    // POST: ContentTypeItems/Create
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ContentTypeItemId,ContentTypeId,Name")] ContentTypeItem contentTypeItem)
    {
      // ContentType ct = contentTypeItem.ContentType;
      ContentType ct = _context.ContentTypes.Where(x => x.ContentTypeId == contentTypeItem.ContentTypeId).FirstOrDefault();

      if (ModelState.IsValid)
      {
        _context.Add(contentTypeItem);
        await _context.SaveChangesAsync();

        // return RedirectToAction("Index");
        // return View("/Views/ContentTypes/Edit.cshtml", ct);

        // Run ContentType controller action: Edit
        return RedirectToAction("Edit", "ContentTypes", new { id = ct.ContentTypeId });
      }
      ViewData["ContentTypeId"] = new SelectList(_context.ContentTypes, "ContentTypeId", "ContentTypeId", contentTypeItem.ContentTypeId);
      return View(contentTypeItem);
    }

    // GET: ContentTypeItems/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var contentTypeItem = await _context.ContentTypeItems.SingleOrDefaultAsync(m => m.ContentTypeItemId == id);
      if (contentTypeItem == null)
      {
        return NotFound();
      }
      ViewData["ContentTypeId"] = new SelectList(_context.ContentTypes, "ContentTypeId", "Name", contentTypeItem.ContentTypeId);
      return View(contentTypeItem);
    }

    // POST: ContentTypeItems/Edit/5
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("ContentTypeItemId,ContentTypeId,Name")] ContentTypeItem contentTypeItem)
    {
      if (id != contentTypeItem.ContentTypeItemId)
      {
        return NotFound();
      }

      if (ModelState.IsValid)
      {
        try
        {
          _context.Update(contentTypeItem);
          await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
          if (!ContentTypeItemExists(contentTypeItem.ContentTypeItemId))
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
      ViewData["ContentTypeId"] = new SelectList(_context.ContentTypes, "ContentTypeId", "Name", contentTypeItem.ContentTypeId);
      return View(contentTypeItem);
    }

    // GET: ContentTypeItems/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var contentTypeItem = await _context.ContentTypeItems.SingleOrDefaultAsync(m => m.ContentTypeItemId == id);
      if (contentTypeItem == null)
      {
        return NotFound();
      }

      return View(contentTypeItem);
    }

    // POST: ContentTypeItems/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      ContentTypeItem cti = _context.ContentTypeItems.Include(x => x.ContentType).Where(x => x.ContentTypeItemId == id).FirstOrDefault();
      // ContentType ct = cti.ContentType;
      var contentTypeItem = await _context.ContentTypeItems.SingleOrDefaultAsync(m => m.ContentTypeItemId == id);
      _context.ContentTypeItems.Remove(contentTypeItem);
      await _context.SaveChangesAsync();

      // return RedirectToAction("Index");
      // return View("/Views/ContentTypes/Edit.cshtml", cti.ContentType);
      return RedirectToAction("Edit", "ContentTypes", new { id = cti.ContentType.ContentTypeId });
    }

    private bool ContentTypeItemExists(int id)
    {
      return _context.ContentTypeItems.Any(e => e.ContentTypeItemId == id);
    }
  }
}
