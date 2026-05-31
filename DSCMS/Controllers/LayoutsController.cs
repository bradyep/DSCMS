using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DSCMS.Models;
using DSCMS.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace DSCMS.Controllers
{
    [Authorize]
    public class LayoutsController : Controller
    {
        private readonly ILayoutRepository _layoutRepository;
        private readonly ILogger<LayoutsController> _logger;

        public LayoutsController(ILayoutRepository layoutRepository, ILogger<LayoutsController> logger)
        {
            _layoutRepository = layoutRepository;
            _logger = logger;
        }

        // GET: Layouts
        public async Task<IActionResult> Index()
        {
            try
            {
                var layouts = await _layoutRepository.GetAllAsync();
                await FixNullLayouts(layouts);
                layouts = await _layoutRepository.GetAllAsync();
                return View(layouts);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error loading layouts: {ex.Message}. Attempting to create default layout...";

                try
                {
                    await CreateDefaultLayoutIfNone();
                    var layouts = await _layoutRepository.GetAllAsync();
                    return View(layouts);
                }
                catch (Exception innerEx)
                {
                    ViewBag.ErrorMessage = $"Failed to create default layout: {innerEx.Message}";
                    return View(new List<Layout>());
                }
            }
        }

        private async Task FixNullLayouts(List<Layout> layouts)
        {
            bool hasChanges = false;

            foreach (var layout in layouts)
            {
                if (string.IsNullOrEmpty(layout.Name))
                {
                    layout.Name = $"Layout {layout.LayoutId}";
                    hasChanges = true;
                }

                if (string.IsNullOrEmpty(layout.LayoutSource))
                {
                    layout.LayoutSource = "/Views/DSCMS/Layouts/_BootstrapBlog.cshtml";
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                foreach (var layout in layouts)
                {
                    await _layoutRepository.UpdateAsync(layout);
                }
            }
        }

        private async Task CreateDefaultLayoutIfNone()
        {
            var layoutCount = await _layoutRepository.CountAsync();
            if (layoutCount == 0)
            {
                var defaultLayout = new Layout
                {
                    Name = "Bootstrap Blog Layout",
                    LayoutSource = "/Views/DSCMS/Layouts/_BootstrapBlog.cshtml",
                    SourceTypeId = 1 // RazorFile
                };

                await _layoutRepository.AddAsync(defaultLayout);
            }
        }

        // GET: Layouts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var layout = await _layoutRepository.GetByIdAsync(id.Value);
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
        public async Task<IActionResult> Create([Bind("LayoutId,Name,LayoutSource,SourceTypeId")] Layout layout)
        {
            if (ModelState.IsValid)
            {
                await _layoutRepository.AddAsync(layout);
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

            var layout = await _layoutRepository.GetByIdAsync(id.Value);
            if (layout == null)
            {
                return NotFound();
            }
            return View(layout);
        }

        // POST: Layouts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LayoutId,Name,LayoutSource,SourceTypeId")] Layout layout)
        {
            if (id != layout.LayoutId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _layoutRepository.UpdateAsync(layout);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _layoutRepository.ExistsAsync(layout.LayoutId))
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

            var layout = await _layoutRepository.GetByIdAsync(id.Value);
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
            var layout = await _layoutRepository.GetByIdAsync(id);
            if (layout != null)
            {
                await _layoutRepository.DeleteAsync(layout);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}


