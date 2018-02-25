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
            return View(await _context.Layouts.ToListAsync());
        }

        // GET: Layouts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var layout = await _context.Layouts.SingleOrDefaultAsync(m => m.LayoutId == id);
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
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LayoutId,FileContents,FileLocation,Name")] Layout layout)
        {
            if (ModelState.IsValid)
            {
                _context.Add(layout);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
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

            var layout = await _context.Layouts.SingleOrDefaultAsync(m => m.LayoutId == id);
            if (layout == null)
            {
                return NotFound();
            }
            return View(layout);
        }

        // POST: Layouts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LayoutId,FileContents,FileLocation,Name")] Layout layout)
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
                return RedirectToAction("Index");
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

            var layout = await _context.Layouts.SingleOrDefaultAsync(m => m.LayoutId == id);
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
            var layout = await _context.Layouts.SingleOrDefaultAsync(m => m.LayoutId == id);
            _context.Layouts.Remove(layout);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool LayoutExists(int id)
        {
            return _context.Layouts.Any(e => e.LayoutId == id);
        }
    }
}
