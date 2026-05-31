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
  public class ContentTypeFieldItemsController : Controller
  {
    private readonly IContentTypeFieldItemRepository _contentTypeFieldItemRepository;
    private readonly IContentRepository _contentRepository;
    private readonly IContentTypeFieldRepository _contentTypeFieldRepository;

    public ContentTypeFieldItemsController(
      IContentTypeFieldItemRepository contentTypeFieldItemRepository,
      IContentRepository contentRepository,
      IContentTypeFieldRepository contentTypeFieldRepository)
    {
      _contentTypeFieldItemRepository = contentTypeFieldItemRepository;
      _contentRepository = contentRepository;
      _contentTypeFieldRepository = contentTypeFieldRepository;
    }

    // GET: ContentTypeFieldItems
    public async Task<IActionResult> Index()
    {
      return View(await _contentTypeFieldItemRepository.GetAllWithDetailsAsync());
    }

    // GET: ContentTypeFieldItems/Details/5
    public async Task<IActionResult> Details(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var contentTypeFieldItem = await _contentTypeFieldItemRepository.GetByIdAsync(id.Value);
      if (contentTypeFieldItem == null)
      {
        return NotFound();
      }

      return View(contentTypeFieldItem);
    }

    // GET: ContentTypeFieldItems/Create
    public async Task<IActionResult> Create(int? id = 0)
    {
      var allContents = await _contentRepository.GetAllSimpleAsync();
      if (id > 0)
      {
        var content = await _contentRepository.GetByIdWithContentTypeAsync(id.Value);
        ViewData["ContentId"] = new SelectList(allContents, "ContentId", "UrlToDisplay", id);
        var fieldsForType = await _contentTypeFieldRepository.GetByContentTypeIdAsync(content.ContentTypeId);
        ViewData["ContentTypeFieldId"] = new SelectList(fieldsForType, "ContentTypeFieldId", "Name");
      }
      else
      {
        ViewData["ContentId"] = new SelectList(allContents, "ContentId", "UrlToDisplay");
        var allFields = await _contentTypeFieldRepository.GetAllAsync();
        ViewData["ContentTypeFieldId"] = new SelectList(allFields, "ContentTypeFieldId", "Name");
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
      var content = await _contentRepository.GetByIdAsync(contentTypeFieldItem.ContentId);

      if (ModelState.IsValid)
      {
        await _contentTypeFieldItemRepository.AddAsync(contentTypeFieldItem);
        return RedirectToAction("Edit", "Contents", new { id = content.ContentId });
      }

      var allContents = await _contentRepository.GetAllSimpleAsync();
      var allFields = await _contentTypeFieldRepository.GetAllAsync();
      ViewData["ContentId"] = new SelectList(allContents, "ContentId", "ContentId", contentTypeFieldItem.ContentId);
      ViewData["ContentTypeFieldId"] = new SelectList(allFields, "ContentTypeFieldId", "ContentTypeFieldId", contentTypeFieldItem.ContentTypeFieldId);
      return View(contentTypeFieldItem);
    }

    // GET: ContentTypeFieldItems/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var contentTypeFieldItem = await _contentTypeFieldItemRepository.GetByIdAsync(id.Value);
      if (contentTypeFieldItem == null)
      {
        return NotFound();
      }

      var allContents = await _contentRepository.GetAllSimpleAsync();
      var allFields = await _contentTypeFieldRepository.GetAllAsync();
      ViewData["ContentId"] = new SelectList(allContents, "ContentId", "UrlToDisplay", contentTypeFieldItem.ContentId);
      ViewData["ContentTypeFieldId"] = new SelectList(allFields, "ContentTypeFieldId", "Name", contentTypeFieldItem.ContentTypeFieldId);
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
          await _contentTypeFieldItemRepository.UpdateAsync(contentTypeFieldItem);
        }
        catch (DbUpdateConcurrencyException)
        {
          if (!await _contentTypeFieldItemRepository.ExistsAsync(contentTypeFieldItem.ContentTypeFieldItemId))
          {
            return NotFound();
          }
          else
          {
            throw;
          }
        }
        return RedirectToAction("Edit", "Contents", new { id = contentTypeFieldItem.ContentId });
      }

      var allContents = await _contentRepository.GetAllSimpleAsync();
      var allFields = await _contentTypeFieldRepository.GetAllAsync();
      ViewData["ContentId"] = new SelectList(allContents, "ContentId", "UrlToDisplay", contentTypeFieldItem.ContentId);
      ViewData["ContentTypeFieldId"] = new SelectList(allFields, "ContentTypeFieldId", "Name", contentTypeFieldItem.ContentTypeFieldId);
      return View(contentTypeFieldItem);
    }

    // GET: ContentTypeFieldItems/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var contentTypeFieldItem = await _contentTypeFieldItemRepository.GetByIdAsync(id.Value);
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
      var fieldItem = await _contentTypeFieldItemRepository.GetByIdWithContentAsync(id);
      var contentTypeFieldItem = await _contentTypeFieldItemRepository.GetByIdAsync(id);
      await _contentTypeFieldItemRepository.DeleteAsync(contentTypeFieldItem);
      return RedirectToAction("Edit", "Contents", new { id = fieldItem.Content.ContentId });
    }
  }
}

