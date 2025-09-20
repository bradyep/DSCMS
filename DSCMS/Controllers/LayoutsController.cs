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
    public class LayoutsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LayoutsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Layouts
        public async Task<IActionResult> Index()
        {
            try
            {
                // First, try to fix any NULL values in the database
                await FixNullLayouts();
                
                var layouts = await _context.Layouts.ToListAsync();
                return View(layouts);
            }
            catch (Exception ex)
            {
                // If there's an error loading layouts, try to create a default one
                ViewBag.ErrorMessage = $"Error loading layouts: {ex.Message}. Attempting to create default layout...";
                
                try
                {
                    await CreateDefaultLayoutIfNone();
                    var layouts = await _context.Layouts.ToListAsync();
                    return View(layouts);
                }
                catch (Exception innerEx)
                {
                    ViewBag.ErrorMessage = $"Failed to create default layout: {innerEx.Message}";
                    return View(new List<Layout>());
                }
            }
        }

        private async Task FixNullLayouts()
        {
            var layouts = await _context.Layouts.ToListAsync();
            bool hasChanges = false;
            
            foreach (var layout in layouts)
            {
                if (string.IsNullOrEmpty(layout.Name))
                {
                    layout.Name = $"Layout {layout.LayoutId}";
                    hasChanges = true;
                }
                
                if (string.IsNullOrEmpty(layout.FileLocation))
                {
                    layout.FileLocation = "/Views/DSCMS/Layouts/_BootstrapBlog.cshtml";
                    hasChanges = true;
                }
                
                // FileContents can be null - that's acceptable, so we don't fix it
            }
            
            if (hasChanges)
            {
                await _context.SaveChangesAsync();
            }
        }
        
        private async Task CreateDefaultLayoutIfNone()
        {
            var layoutCount = await _context.Layouts.CountAsync();
            if (layoutCount == 0)
            {
                var defaultLayout = new Layout
                {
                    Name = "Bootstrap Blog Layout",
                    FileLocation = "/Views/DSCMS/Layouts/_BootstrapBlog.cshtml", 
                    FileContents = null // Reference external file, no inline content needed
                };
                
                _context.Layouts.Add(defaultLayout);
                await _context.SaveChangesAsync();
            }
        }

        // GET: Layouts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var layout = await _context.Layouts
                .FirstOrDefaultAsync(m => m.LayoutId == id);
            if (layout == null)
            {
                return NotFound();
            }

            return View(layout);
        }

        // GET: Layouts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Layouts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LayoutId,Name,FileLocation,FileContents")] Layout layout)
        {
            if (ModelState.IsValid)
            {
                _context.Add(layout);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(layout);
        }

        // GET: Layouts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var layout = await _context.Layouts.FindAsync(id);
            if (layout == null)
            {
                return NotFound();
            }
            return View(layout);
        }

        // POST: Layouts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LayoutId,Name,FileLocation,FileContents")] Layout layout)
        {
            if (id != layout.LayoutId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(layout);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LayoutExists(layout.LayoutId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(layout);
        }

        // GET: Layouts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var layout = await _context.Layouts
                .FirstOrDefaultAsync(m => m.LayoutId == id);
            if (layout == null)
            {
                return NotFound();
            }

            return View(layout);
        }

        // POST: Layouts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var layout = await _context.Layouts.FindAsync(id);
            if (layout != null)
            {
                _context.Layouts.Remove(layout);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool LayoutExists(int id)
        {
            return _context.Layouts.Any(e => e.LayoutId == id);
        }
    }
}