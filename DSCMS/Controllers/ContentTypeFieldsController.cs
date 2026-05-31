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

namespace DSCMS.Controllers
{
  [Authorize]
  public class ContentTypeFieldsController : Controller
  {
    private readonly IContentTypeFieldRepository _contentTypeFieldRepository;
    private readonly IContentTypeRepository _contentTypeRepository;

    public ContentTypeFieldsController(IContentTypeFieldRepository contentTypeFieldRepository, IContentTypeRepository contentTypeRepository)
    {
      _contentTypeFieldRepository = contentTypeFieldRepository;
      _contentTypeRepository = contentTypeRepository;
    }

    // GET: ContentTypeFields
    public async Task<IActionResult> Index()
    {
      return View(await _contentTypeFieldRepository.GetAllWithContentTypeAsync());
    }

    // GET: ContentTypeFields/Details/5
    public async Task<IActionResult> Details(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var contentTypeField = await _contentTypeFieldRepository.GetByIdAsync(id.Value);
      if (contentTypeField == null)
      {
        return NotFound();
      }

      return View(contentTypeField);
    }

    // GET: ContentTypeFields/Create
    public async Task<IActionResult> Create(int? id)
    {
      if (id > 0)
      {
        var filtered = await _contentTypeRepository.GetAllAsync();
        ViewData["ContentTypeId"] = new SelectList(filtered.Where(ct => ct.ContentTypeId == id), "ContentTypeId", "Name", id);
      }
      else
      {
        var allContentTypes = await _contentTypeRepository.GetAllAsync();
        ViewData["ContentTypeId"] = new SelectList(allContentTypes, "ContentTypeId", "Name");
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
      var ct = await _contentTypeRepository.GetByIdAsync(contentTypeField.ContentTypeId);

      if (ModelState.IsValid)
      {
        await _contentTypeFieldRepository.AddAsync(contentTypeField);
        return RedirectToAction("Edit", "ContentTypes", new { id = ct.ContentTypeId });
      }

      var allContentTypes = await _contentTypeRepository.GetAllAsync();
      ViewData["ContentTypeId"] = new SelectList(allContentTypes, "ContentTypeId", "ContentTypeId", contentTypeField.ContentTypeId);
      return View(contentTypeField);
    }

    // GET: ContentTypeFields/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var contentTypeField = await _contentTypeFieldRepository.GetByIdAsync(id.Value);
      if (contentTypeField == null)
      {
        return NotFound();
      }

      var allContentTypes = await _contentTypeRepository.GetAllAsync();
      ViewData["ContentTypeId"] = new SelectList(allContentTypes, "ContentTypeId", "Name", contentTypeField.ContentTypeId);
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
          await _contentTypeFieldRepository.UpdateAsync(contentTypeField);
        }
        catch (DbUpdateConcurrencyException)
        {
          if (!await _contentTypeFieldRepository.ExistsAsync(contentTypeField.ContentTypeFieldId))
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

      var allContentTypes = await _contentTypeRepository.GetAllAsync();
      ViewData["ContentTypeId"] = new SelectList(allContentTypes, "ContentTypeId", "Name", contentTypeField.ContentTypeId);
      return View(contentTypeField);
    }

    // GET: ContentTypeFields/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var contentTypeField = await _contentTypeFieldRepository.GetByIdAsync(id.Value);
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
      var ctf = await _contentTypeFieldRepository.GetByIdWithContentTypeAsync(id);
      var contentTypeField = await _contentTypeFieldRepository.GetByIdAsync(id);
      await _contentTypeFieldRepository.DeleteAsync(contentTypeField);
      return RedirectToAction("Edit", "ContentTypes", new { id = ctf.ContentType.ContentTypeId });
    }
  }
}

