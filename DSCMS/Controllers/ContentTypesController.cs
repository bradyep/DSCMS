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
  public class ContentTypesController : Controller
  {
    private readonly IContentTypeRepository _contentTypeRepository;
    private readonly ITemplateRepository _templateRepository;

    public ContentTypesController(IContentTypeRepository contentTypeRepository, ITemplateRepository templateRepository)
    {
      _contentTypeRepository = contentTypeRepository;
      _templateRepository = templateRepository;
    }

    // GET: ContentTypes
    public async Task<IActionResult> Index()
    {
      return View(await _contentTypeRepository.GetAllWithTemplateAsync());
    }

    // GET: ContentTypes/Details/5
    public async Task<IActionResult> Details(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var contentType = await _contentTypeRepository.GetByIdWithTemplateAsync(id.Value);
      if (contentType == null)
      {
        return NotFound();
      }

      return View(contentType);
    }

    // GET: ContentTypes/Create
    public async Task<IActionResult> Create()
    {
      var multipleContentsTemplates = await _templateRepository.GetByIsForMultipleContentsAsync(1);
      ViewData["MultipleContentsTemplateId"] = new SelectList(multipleContentsTemplates, "TemplateId", "Name");

      var singleContentTemplates = await _templateRepository.GetByIsForMultipleContentsAsync(0);
      List<Template> ts = new List<Template>();
      ts.Add(new Template { Name = "", TemplateId = 0 });
      ts.AddRange(singleContentTemplates);
      ViewData["DefaultSingleContentTemplateId"] = new SelectList(ts, "TemplateId", "Name", ts);

      return View();
    }

    // POST: ContentTypes/Create
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ContentTypeId,Description,Name,MultipleContentsTemplateId,DefaultSingleContentTemplateId,Title,ItemsPerPage,IsDefaultContentType")] ContentType contentType)
    {
      if (contentType.DefaultSingleContentTemplateId < 1) contentType.DefaultSingleContentTemplateId = null;
      if (ModelState.IsValid)
      {
        await _contentTypeRepository.AddAsync(contentType);
        return RedirectToAction("Index");
      }

      var multipleContentsTemplates = await _templateRepository.GetByIsForMultipleContentsAsync(1);
      ViewData["MultipleContentsTemplateId"] = new SelectList(multipleContentsTemplates, "TemplateId", "Name", contentType.MultipleContentsTemplateId);

      int defaultSingleTemplateIdToUse = contentType.DefaultSingleContentTemplateId ?? 0;
      var singleContentTemplates = await _templateRepository.GetByIsForMultipleContentsAsync(0);
      List<Template> ts = new List<Template>();
      ts.Add(new Template { Name = "", TemplateId = 0 });
      ts.AddRange(singleContentTemplates);
      ViewData["DefaultSingleContentTemplateId"] = new SelectList(ts, "TemplateId", "Name", defaultSingleTemplateIdToUse);

      return View(contentType);
    }

    // GET: ContentTypes/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var contentType = await _contentTypeRepository.GetByIdWithFieldsAsync(id.Value);
      if (contentType == null)
      {
        return NotFound();
      }

      var multipleContentsTemplates = await _templateRepository.GetByIsForMultipleContentsAsync(1);
      ViewData["MultipleContentsTemplateId"] = new SelectList(multipleContentsTemplates, "TemplateId", "Name", contentType.MultipleContentsTemplateId);

      var singleContentTemplates = await _templateRepository.GetByIsForMultipleContentsAsync(0);
      List<Template> ts = new List<Template>();
      ts.Add(new Template { Name = "", TemplateId = 0 });
      ts.AddRange(singleContentTemplates);
      ViewData["DefaultSingleContentTemplateId"] = new SelectList(ts, "TemplateId", "Name", contentType.DefaultSingleContentTemplateId);

      return View(contentType);
    }

    // POST: ContentTypes/Edit/5
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("ContentTypeId,Description,Name,MultipleContentsTemplateId,Title,ItemsPerPage,DefaultSingleContentTemplateId,IsDefaultContentType")] ContentType contentType)
    {
      if (id != contentType.ContentTypeId)
      {
        return NotFound();
      }

      if (ModelState.IsValid)
      {
        try
        {
          await _contentTypeRepository.UpdateAsync(contentType);
        }
        catch (DbUpdateConcurrencyException)
        {
          if (!await _contentTypeRepository.ExistsAsync(contentType.ContentTypeId))
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

      var allTemplates = await _templateRepository.GetAllAsync();
      ViewData["MultipleContentsTemplateId"] = new SelectList(allTemplates, "TemplateId", "Name", contentType.MultipleContentsTemplateId);
      return View(contentType);
    }

    // GET: ContentTypes/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var contentType = await _contentTypeRepository.GetByIdWithTemplateAsync(id.Value);
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
      var contentType = await _contentTypeRepository.GetByIdAsync(id);
      if (contentType != null)
      {
        await _contentTypeRepository.DeleteAsync(contentType);
      }
      return RedirectToAction("Index");
    }
  }
}
