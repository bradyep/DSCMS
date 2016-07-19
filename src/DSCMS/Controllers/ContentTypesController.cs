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
            var applicationDbContext = _context.ContentTypes.Include(c => c.Template);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ContentTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contentType = await _context.ContentTypes.Include(ct => ct.Template).SingleOrDefaultAsync(m => m.ContentTypeId == id);
            if (contentType == null)
            {
                return NotFound();
            }

            return View(contentType);
        }

        // GET: ContentTypes/Create
        public IActionResult Create()
        {
            ViewData["TemplateId"] = new SelectList(_context.Templates, "TemplateId", "Name");
            return View();
        }

        // POST: ContentTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ContentTypeId,Description,Name,TemplateId,Title,ItemsPerPage")] ContentType contentType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(contentType);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["TemplateId"] = new SelectList(_context.Templates, "TemplateId", "Name", contentType.TemplateId);
            return View(contentType);
        }

        // GET: ContentTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contentType = await _context.ContentTypes.Include(ct => ct.ContentTypeItems).SingleOrDefaultAsync(m => m.ContentTypeId == id);
            if (contentType == null)
            {
                return NotFound();
            }
            ViewData["TemplateId"] = new SelectList(_context.Templates, "TemplateId", "Name", contentType.TemplateId);
            return View(contentType);
        }

        // POST: ContentTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ContentTypeId,Description,Name,TemplateId,Title,ItemsPerPage")] ContentType contentType)
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
            ViewData["TemplateId"] = new SelectList(_context.Templates, "TemplateId", "Name", contentType.TemplateId);
            return View(contentType);
        }

        // GET: ContentTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contentType = await _context.ContentTypes.Include(ct => ct.Template).SingleOrDefaultAsync(m => m.ContentTypeId == id);
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
