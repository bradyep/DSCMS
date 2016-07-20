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
    public class ContentItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContentItemsController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: ContentItems
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ContentItems.Include(c => c.Content).Include(c => c.ContentTypeItem);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ContentItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contentItem = await _context.ContentItems.SingleOrDefaultAsync(m => m.ContentItemId == id);
            if (contentItem == null)
            {
                return NotFound();
            }

            return View(contentItem);
        }

        // GET: ContentItems/Create
        public IActionResult Create()
        {
            ViewData["ContentId"] = new SelectList(_context.Contents, "ContentId", "ContentId");
            ViewData["ContentTypeItemId"] = new SelectList(_context.ContentTypeItems, "ContentTypeItemId", "ContentTypeItemId");
            return View();
        }

        // POST: ContentItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ContentItemId,ContentId,ContentTypeItemId,Value")] ContentItem contentItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(contentItem);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["ContentId"] = new SelectList(_context.Contents, "ContentId", "ContentId", contentItem.ContentId);
            ViewData["ContentTypeItemId"] = new SelectList(_context.ContentTypeItems, "ContentTypeItemId", "ContentTypeItemId", contentItem.ContentTypeItemId);
            return View(contentItem);
        }

        // GET: ContentItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contentItem = await _context.ContentItems.SingleOrDefaultAsync(m => m.ContentItemId == id);
            if (contentItem == null)
            {
                return NotFound();
            }
            ViewData["ContentId"] = new SelectList(_context.Contents, "ContentId", "UrlToDisplay", contentItem.ContentId);
            ViewData["ContentTypeItemId"] = new SelectList(_context.ContentTypeItems, "ContentTypeItemId", "Name", contentItem.ContentTypeItemId);
            return View(contentItem);
        }

        // POST: ContentItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ContentItemId,ContentId,ContentTypeItemId,Value")] ContentItem contentItem)
        {
            if (id != contentItem.ContentItemId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contentItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContentItemExists(contentItem.ContentItemId))
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
            ViewData["ContentId"] = new SelectList(_context.Contents, "ContentId", "UrlToDisplay", contentItem.ContentId);
            ViewData["ContentTypeItemId"] = new SelectList(_context.ContentTypeItems, "ContentTypeItemId", "Name", contentItem.ContentTypeItemId);
            return View(contentItem);
        }

        // GET: ContentItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contentItem = await _context.ContentItems.SingleOrDefaultAsync(m => m.ContentItemId == id);
            if (contentItem == null)
            {
                return NotFound();
            }

            return View(contentItem);
        }

        // POST: ContentItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contentItem = await _context.ContentItems.SingleOrDefaultAsync(m => m.ContentItemId == id);
            _context.ContentItems.Remove(contentItem);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool ContentItemExists(int id)
        {
            return _context.ContentItems.Any(e => e.ContentItemId == id);
        }
    }
}
