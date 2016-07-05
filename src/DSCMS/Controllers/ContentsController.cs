using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DSCMS.Data;
using DSCMS.Models;

namespace DSCMS.Controllers
{
    public class ContentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContentsController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: Contents
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Contents.Include(c => c.ContentType).Include(c => c.CreatedByUser).Include(c => c.LastUpdatedByUser).Include(c => c.Template);
            return View(await applicationDbContext.ToListAsync());
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
            ViewData["ContentTypeId"] = new SelectList(_context.ContentTypes, "ContentTypeId", "ContentTypeId");
            ViewData["CreatedBy"] = new SelectList(_context.Users, "UserId", "UserId");
            ViewData["LastUpdatedBy"] = new SelectList(_context.Users, "UserId", "UserId");
            ViewData["TemplateId"] = new SelectList(_context.Templates, "TemplateId", "TemplateId");
            return View();
        }

        // POST: Contents/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ContentId,Body,ContentTypeId,CreatedBy,CreationDate,LastUpdatedBy,LastUpdatedDate,TemplateId,UrlToDisplay")] Content content)
        {
            if (ModelState.IsValid)
            {
                _context.Add(content);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["ContentTypeId"] = new SelectList(_context.ContentTypes, "ContentTypeId", "ContentTypeId", content.ContentTypeId);
            ViewData["CreatedBy"] = new SelectList(_context.Users, "UserId", "UserId", content.CreatedBy);
            ViewData["LastUpdatedBy"] = new SelectList(_context.Users, "UserId", "UserId", content.LastUpdatedBy);
            ViewData["TemplateId"] = new SelectList(_context.Templates, "TemplateId", "TemplateId", content.TemplateId);
            return View(content);
        }

        // GET: Contents/Edit/5
        public async Task<IActionResult> Edit(int? id)
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
            ViewData["ContentTypeId"] = new SelectList(_context.ContentTypes, "ContentTypeId", "ContentTypeId", content.ContentTypeId);
            ViewData["CreatedBy"] = new SelectList(_context.Users, "UserId", "UserId", content.CreatedBy);
            ViewData["LastUpdatedBy"] = new SelectList(_context.Users, "UserId", "UserId", content.LastUpdatedBy);
            ViewData["TemplateId"] = new SelectList(_context.Templates, "TemplateId", "TemplateId", content.TemplateId);
            return View(content);
        }

        // POST: Contents/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ContentId,Body,ContentTypeId,CreatedBy,CreationDate,LastUpdatedBy,LastUpdatedDate,TemplateId,UrlToDisplay")] Content content)
        {
            if (id != content.ContentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
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
            ViewData["ContentTypeId"] = new SelectList(_context.ContentTypes, "ContentTypeId", "ContentTypeId", content.ContentTypeId);
            ViewData["CreatedBy"] = new SelectList(_context.Users, "UserId", "UserId", content.CreatedBy);
            ViewData["LastUpdatedBy"] = new SelectList(_context.Users, "UserId", "UserId", content.LastUpdatedBy);
            ViewData["TemplateId"] = new SelectList(_context.Templates, "TemplateId", "TemplateId", content.TemplateId);
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
