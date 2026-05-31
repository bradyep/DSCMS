using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DSCMS.Models;
using DSCMS.Repositories.Interfaces;

namespace DSCMS.Controllers
{
  [Authorize]
  public class ContentsController : Controller
  {
    private readonly IContentRepository _contentRepository;
    private readonly IContentTypeRepository _contentTypeRepository;
    private readonly ITemplateRepository _templateRepository;
    private readonly ISourceTypeRepository _sourceTypeRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ContentsController> _logger;

    public ContentsController(
      IContentRepository contentRepository,
      IContentTypeRepository contentTypeRepository,
      ITemplateRepository templateRepository,
      ISourceTypeRepository sourceTypeRepository,
      IUserRepository userRepository,
      ILogger<ContentsController> logger)
    {
      _contentRepository = contentRepository;
      _contentTypeRepository = contentTypeRepository;
      _templateRepository = templateRepository;
      _sourceTypeRepository = sourceTypeRepository;
      _userRepository = userRepository;
      _logger = logger;
    }

    // GET: Contents
    public async Task<IActionResult> Index(string contentType)
    {
      _logger.LogDebug("Contents Index requested with contentType filter: {ContentType}", contentType);

      var result = await _contentRepository.GetAllWithDetailsAsync(contentType);
      _logger.LogInformation("Returning {ContentCount} contents for type '{ContentType}'", result.Count, contentType ?? "all");

      var allContentTypes = await _contentTypeRepository.GetAllAsync();
      List<ContentType> cts = new List<ContentType>();
      cts.Add(new ContentType { Name = "" });
      cts.AddRange(allContentTypes);
      ViewData["ContentType"] = new SelectList(cts, "Name", "Name", contentType);

      return View(result);
    }

    // GET: Contents/Details/5
    public async Task<IActionResult> Details(int? id)
    {
      if (id == null)
      {
        _logger.LogWarning("Contents Details requested with null id");
        return NotFound();
      }

      _logger.LogDebug("Contents Details requested for id: {ContentId}", id);

      var content = await _contentRepository.GetByIdAsync(id.Value);
      if (content == null)
      {
        _logger.LogWarning("Content not found with id: {ContentId}", id);
        return NotFound();
      }

      _logger.LogDebug("Found content: {ContentId} - {ContentTitle}", content.ContentId, content.Title);
      return View(content);
    }

    // GET: Contents/Create
    public async Task<IActionResult> Create()
    {
      _logger.LogDebug("Contents Create form requested");

      var allContentTypes = await _contentTypeRepository.GetAllAsync();
      ViewData["ContentTypeId"] = new SelectList(allContentTypes, "ContentTypeId", "Name");

      var users = await _userRepository.GetAllAsync();
      ViewData["CreatedBy"] = new SelectList(users, "Id", "DisplayName");
      ViewData["LastUpdatedBy"] = new SelectList(users, "Id", "DisplayName");

      var singleContentTemplates = await _templateRepository.GetByIsForMultipleContentsAsync(0);
      ViewData["TemplateId"] = new SelectList(singleContentTemplates, "TemplateId", "Name");

      var sourceTypes = await _sourceTypeRepository.GetAllAsync();
      ViewData["BodySourceTypeId"] = new SelectList(sourceTypes, "SourceTypeId", "Description", (int)SourceTypeEnum.HTML);

      // Put together a Dictionary of all ContentTypes and their DefaultSingleContentTemplateId (if they have one)
      var contentTypeDefaultTemplateLookup = new Dictionary<int, int>();
      var contentTypesWithDefaultTemplates = allContentTypes.Where(ct => ct.DefaultSingleContentTemplateId != null).ToList();
      foreach (var item in contentTypesWithDefaultTemplates)
      {
        contentTypeDefaultTemplateLookup.Add(item.ContentTypeId, item.DefaultSingleContentTemplateId ?? 0);
      }
      ViewData["DefaultTemplateLookup"] = contentTypeDefaultTemplateLookup;

      return View();
    }

    // POST: Contents/Create
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ContentId,BodySource,BodySourceTypeId,ContentTypeId,CreatedBy,LastUpdatedBy,TemplateId,Title,UrlToDisplay")] Content content)
    {
      _logger.LogDebug("Contents Create POST received for title: {ContentTitle}", content.Title);

      if (ModelState.IsValid)
      {
        content.CreationDate = DateTime.Now;
        content.LastUpdatedDate = DateTime.Now;
        await _contentRepository.AddAsync(content);

        _logger.LogInformation("Created new content: {ContentId} - {ContentTitle}", content.ContentId, content.Title);
        return RedirectToAction("Index");
      }

      _logger.LogWarning("Model state invalid for content creation: {ContentTitle}", content.Title);
      var contentTypes = await _contentTypeRepository.GetAllAsync();
      var users = await _userRepository.GetAllAsync();
      var sourceTypes = await _sourceTypeRepository.GetAllAsync();
      var allTemplates = await _templateRepository.GetAllAsync();
      ViewData["ContentTypeId"] = new SelectList(contentTypes, "ContentTypeId", "Name", content.ContentTypeId);
      ViewData["CreatedBy"] = new SelectList(users, "Id", "DisplayName", content.CreatedBy);
      ViewData["LastUpdatedBy"] = new SelectList(users, "Id", "DisplayName", content.LastUpdatedBy);
      ViewData["TemplateId"] = new SelectList(allTemplates, "TemplateId", "Name", content.TemplateId);
      ViewData["BodySourceTypeId"] = new SelectList(sourceTypes, "SourceTypeId", "Description", content.BodySourceTypeId);
      return View(content);
    }

    // GET: Contents/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
      if (id == null)
      {
        _logger.LogWarning("Contents Edit requested with null id");
        return NotFound();
      }

      _logger.LogDebug("Contents Edit requested for id: {ContentId}", id);

      var content = await _contentRepository.GetByIdWithFieldItemsAsync(id.Value);
      if (content == null)
      {
        _logger.LogWarning("Content not found for edit with id: {ContentId}", id);
        return NotFound();
      }

      _logger.LogDebug("Loading edit form for content: {ContentId} - {ContentTitle}", content.ContentId, content.Title);
      var contentTypes = await _contentTypeRepository.GetAllAsync();
      var users = await _userRepository.GetAllAsync();
      var singleContentTemplates = await _templateRepository.GetByIsForMultipleContentsAsync(0);
      var sourceTypes = await _sourceTypeRepository.GetAllAsync();
      ViewData["ContentTypeId"] = new SelectList(contentTypes, "ContentTypeId", "Name", content.ContentTypeId);
      ViewData["CreatedBy"] = new SelectList(users, "Id", "DisplayName", content.CreatedBy);
      ViewData["LastUpdatedBy"] = new SelectList(users, "Id", "DisplayName", content.LastUpdatedBy);
      ViewData["TemplateId"] = new SelectList(singleContentTemplates, "TemplateId", "Name", content.TemplateId);
      ViewData["BodySourceTypeId"] = new SelectList(sourceTypes, "SourceTypeId", "Description", content.BodySourceTypeId);
      return View(content);
    }

    // POST: Contents/Edit/5
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("ContentId,BodySource,BodySourceTypeId,ContentTypeId,CreatedBy,CreationDate,LastUpdatedBy,LastUpdatedDate,TemplateId,Title,UrlToDisplay")] Content content)
    {
      if (id != content.ContentId)
      {
        _logger.LogWarning("Content Edit POST received with mismatched id: {UrlId} vs {ContentId}", id, content.ContentId);
        return NotFound();
      }

      _logger.LogDebug("Contents Edit POST received for: {ContentId} - {ContentTitle}", content.ContentId, content.Title);

      if (ModelState.IsValid)
      {
        content.LastUpdatedDate = DateTime.Now;
        try
        {
          await _contentRepository.UpdateAsync(content);
          _logger.LogInformation("Updated content: {ContentId} - {ContentTitle}", content.ContentId, content.Title);
        }
        catch (DbUpdateConcurrencyException ex)
        {
          _logger.LogError(ex, "Concurrency exception updating content: {ContentId}", content.ContentId);
          if (!await _contentRepository.ExistsAsync(content.ContentId))
          {
            return NotFound();
          }
          else
          {
            throw;
          }
        }
        catch (DbUpdateException ex)
        {
          _logger.LogError(ex, "Database update exception updating content: {ContentId}", content.ContentId);
          ModelState.AddModelError("", "Unable to save changes. Please ensure all required fields have valid values.");
          var contentTypes = await _contentTypeRepository.GetAllAsync();
          var users = await _userRepository.GetAllAsync();
          var allTemplates = await _templateRepository.GetAllAsync();
          var sourceTypes = await _sourceTypeRepository.GetAllAsync();
          ViewData["ContentTypeId"] = new SelectList(contentTypes, "ContentTypeId", "Name", content.ContentTypeId);
          ViewData["CreatedBy"] = new SelectList(users, "Id", "DisplayName", content.CreatedBy);
          ViewData["LastUpdatedBy"] = new SelectList(users, "Id", "DisplayName", content.LastUpdatedBy);
          ViewData["TemplateId"] = new SelectList(allTemplates, "TemplateId", "Name", content.TemplateId);
          ViewData["BodySourceTypeId"] = new SelectList(sourceTypes, "SourceTypeId", "Description", content.BodySourceTypeId);
          return View(content);
        }
        return RedirectToAction("Index");
      }

      _logger.LogWarning("Model state invalid for content edit: {ContentId} - {ContentTitle}", content.ContentId, content.Title);
      var cts = await _contentTypeRepository.GetAllAsync();
      var allUsers = await _userRepository.GetAllAsync();
      var templates = await _templateRepository.GetAllAsync();
      var srcTypes = await _sourceTypeRepository.GetAllAsync();
      ViewData["ContentTypeId"] = new SelectList(cts, "ContentTypeId", "Name", content.ContentTypeId);
      ViewData["CreatedBy"] = new SelectList(allUsers, "Id", "DisplayName", content.CreatedBy);
      ViewData["LastUpdatedBy"] = new SelectList(allUsers, "Id", "DisplayName", content.LastUpdatedBy);
      ViewData["TemplateId"] = new SelectList(templates, "TemplateId", "Name", content.TemplateId);
      ViewData["BodySourceTypeId"] = new SelectList(srcTypes, "SourceTypeId", "Description", content.BodySourceTypeId);
      return View(content);
    }

    // GET: Contents/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
      if (id == null)
      {
        _logger.LogWarning("Contents Delete requested with null id");
        return NotFound();
      }

      _logger.LogDebug("Contents Delete requested for id: {ContentId}", id);

      var content = await _contentRepository.GetByIdAsync(id.Value);
      if (content == null)
      {
        _logger.LogWarning("Content not found for delete with id: {ContentId}", id);
        return NotFound();
      }

      _logger.LogDebug("Loading delete confirmation for content: {ContentId} - {ContentTitle}", content.ContentId, content.Title);
      return View(content);
    }

    // POST: Contents/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      _logger.LogDebug("Contents Delete POST confirmed for id: {ContentId}", id);

      var content = await _contentRepository.GetByIdAsync(id);
      if (content != null)
      {
        await _contentRepository.DeleteAsync(content);
        _logger.LogInformation("Deleted content: {ContentId} - {ContentTitle}", content.ContentId, content.Title);
      }
      else
      {
        _logger.LogWarning("Attempted to delete non-existent content with id: {ContentId}", id);
      }

      return RedirectToAction("Index");
    }
  }
}
