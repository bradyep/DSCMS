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
  public class ContentTypeFieldsController : Controller
  {
    private readonly ApplicationDbContext _context;

    public ContentTypeFieldsController(ApplicationDbContext context)
    {
      _context = context;
    }

    // GET: ContentTypeFields
    public async Task<IActionResult> Index()
    {
      var applicationDbContext = _context.ContentTypeFields.Include(c => c.ContentType);
      return View(await applicationDbContext.ToListAsync());
    }

    // GET: ContentTypeFields/Details/5
    public async Task<IActionResult> Details(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var contentTypeField = await _context.ContentTypeFields.SingleOrDefaultAsync(m => m.ContentTypeFieldId == id);
      if (contentTypeField == null)
      {
        return NotFound();
      }

      return View(contentTypeField);
    }

    // GET: ContentTypeFields/Create
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

    // POST: ContentTypeFields/Create
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ContentTypeFieldId,ContentTypeId,Name,Required")] ContentTypeField contentTypeField)
    {
      // ContentType ct = contentTypeField.ContentType;
      ContentType ct = _context.ContentTypes.Where(x => x.ContentTypeId == contentTypeField.ContentTypeId).FirstOrDefault();

      if (ModelState.IsValid)
      {
        _context.Add(contentTypeField);
        await _context.SaveChangesAsync();

        // return RedirectToAction("Index");
        // return View("/Views/ContentTypes/Edit.cshtml", ct);

        // Run ContentType controller action: Edit
        return RedirectToAction("Edit", "ContentTypes", new { id = ct.ContentTypeId });
      }
      ViewData["ContentTypeId"] = new SelectList(_context.ContentTypes, "ContentTypeId", "ContentTypeId", contentTypeField.ContentTypeId);
      return View(contentTypeField);
    }

    // GET: ContentTypeFields/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var contentTypeField = await _context.ContentTypeFields.SingleOrDefaultAsync(m => m.ContentTypeFieldId == id);
      if (contentTypeField == null)
      {
        return NotFound();
      }
      ViewData["ContentTypeId"] = new SelectList(_context.ContentTypes, "ContentTypeId", "Name", contentTypeField.ContentTypeId);
      return View(contentTypeField);
    }

    // POST: ContentTypeFields/Edit/5
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("ContentTypeFieldId,ContentTypeId,Name,Required")] ContentTypeField contentTypeField)
    {
      if (id != contentTypeField.ContentTypeFieldId)
      {
        return NotFound();
      }

      if (ModelState.IsValid)
      {
        try
        {
          _context.Update(contentTypeField);
          await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
          if (!ContentTypeFieldExists(contentTypeField.ContentTypeFieldId))
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
      ViewData["ContentTypeId"] = new SelectList(_context.ContentTypes, "ContentTypeId", "Name", contentTypeField.ContentTypeId);
      return View(contentTypeField);
    }

    // GET: ContentTypeFields/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var contentTypeField = await _context.ContentTypeFields.SingleOrDefaultAsync(m => m.ContentTypeFieldId == id);
      if (contentTypeField == null)
      {
        return NotFound();
      }

      return View(contentTypeField);
    }

    // POST: ContentTypeFields/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      ContentTypeField ctf = _context.ContentTypeFields.Include(x => x.ContentType).Where(x => x.ContentTypeFieldId == id).FirstOrDefault();
      // ContentType ct = ctf.ContentType;
      var contentTypeField = await _context.ContentTypeFields.SingleOrDefaultAsync(m => m.ContentTypeFieldId == id);
      _context.ContentTypeFields.Remove(contentTypeField);
      await _context.SaveChangesAsync();

      // return RedirectToAction("Index");
      // return View("/Views/ContentTypes/Edit.cshtml", ctf.ContentType);
      return RedirectToAction("Edit", "ContentTypes", new { id = ctf.ContentType.ContentTypeId });
    }

    private bool ContentTypeFieldExists(int id)
    {
      return _context.ContentTypeFields.Any(e => e.ContentTypeFieldId == id);
    }
  }
}
