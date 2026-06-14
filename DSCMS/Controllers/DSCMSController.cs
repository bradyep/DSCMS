using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DSCMS.Models;
using DSCMS.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace DSCMS.Controllers
{
  /// <summary>
  /// Responsible for handling requests related to displaying content types and individual content items.
  /// </summary>
  public class DSCMSController : Controller
  {
    private readonly IContentRepository _contentRepository;
    private readonly IContentTypeRepository _contentTypeRepository;
    private readonly ITemplateRepository _templateRepository;
    private readonly ILogger<DSCMSController> _logger;

    public DSCMSController(
      IContentRepository contentRepository,
      IContentTypeRepository contentTypeRepository,
      ITemplateRepository templateRepository,
      ILogger<DSCMSController> logger)
    {
      _contentRepository = contentRepository;
      _contentTypeRepository = contentTypeRepository;
      _templateRepository = templateRepository;
      _logger = logger;
    }

    /// <summary>
    /// Navigates to the content type or specific content item based on the provided parameters
    /// </summary>
    /// <param name="contentTypeName">Display multiple pieces of Content for a given ContentType</param>
    /// <param name="contentUrl">Display a specific piece of Content</param>
    /// <param name="page">Used for paging when displaying a ContentType</param>
    /// <returns></returns>
    public async Task<IActionResult> Content(string contentTypeName = "blog", string contentUrl = "", string page = "")
    {
      _logger.LogDebug("Content requested: ContentTypeName={ContentTypeName}, ContentUrl={ContentUrl}, Page={Page}",
        contentTypeName, contentUrl, page);

      string pContentTypeName = contentTypeName.ToLower();
      string pContentUrl = contentUrl.ToLower();
      Content content = null;
      Template template = null;

      ViewData["ContentTypeName"] = pContentTypeName;
      ViewData["ContentUrl"] = pContentUrl;

      ContentType contentType = await _contentTypeRepository.GetByNameAsync(pContentTypeName);

      // If no content type found, show welcome page for first-time setup
      if (contentType == null)
      {
        _logger.LogWarning("ContentType '{ContentTypeName}' not found, showing welcome page", pContentTypeName);
        ViewData["Title"] = "Welcome to DSCMS";
        return View("~/Views/DSCMS/Welcome.cshtml");
      }

      _logger.LogDebug("Found ContentType: {ContentTypeId} - {ContentTypeName}", contentType.ContentTypeId, contentType.Name);

      if (pContentUrl.Trim() != "") // Specific Content was requested
      {
        content = await _contentRepository.GetByUrlAndContentTypeAsync(pContentUrl, contentType.ContentTypeId);

        if (content == null)
        {
          _logger.LogWarning("Content not found: ContentUrl={ContentUrl}, ContentType={ContentTypeId}", pContentUrl, contentType.ContentTypeId);
          return NotFound();
        }

        _logger.LogDebug("Found Content: {ContentId} - {ContentTitle}", content.ContentId, content.Title);

        content.ContentType = contentType;
        ViewData["Title"] = content.Title ?? "Title";

        // DEBUG: Add debugging information
        ViewData["DebugInfo"] = $"Content ID: {content.ContentId}, BodySource Length: {content.BodySource?.Length ?? 0}, BodySource Preview: {content.BodySource?.Substring(0, Math.Min(100, content.BodySource?.Length ?? 0)) ?? "NULL"}";

        // Use content's template, or fall back to ContentType's default template if content has no template
        int templateIdToUse = content.TemplateId > 0 ? content.TemplateId :
                             (contentType.DefaultSingleContentTemplateId > 0 ? contentType.DefaultSingleContentTemplateId.Value : 0);

        // Check if we should display raw content with no template
        if (content.TemplateId == 0 && (contentType.DefaultSingleContentTemplateId == null || contentType.DefaultSingleContentTemplateId == 0))
        {
          _logger.LogDebug("Returning raw HTML content for ContentId={ContentId}", content.ContentId);
          // Return raw HTML content with no template
          return new ContentResult
          {
            Content = content.BodySource ?? "",
            ContentType = "text/html"
          };
        }

        if (templateIdToUse > 0)
        {
          template = await _templateRepository.GetByIdWithLayoutAsync(templateIdToUse);

          if (template != null)
          {
            _logger.LogDebug("Using template: {TemplateId} - {TemplateName}", template.TemplateId, template.Name);
          }
          else
          {
            _logger.LogWarning("Template not found: TemplateId={TemplateId}", templateIdToUse);
          }
        }
      }
      else // ContentType was requested
      {
        _logger.LogDebug("Displaying ContentType listing for: {ContentTypeName}", pContentTypeName);
        ViewData["Title"] = contentType.Title ?? "Title";

        if (contentType.MultipleContentsTemplateId > 0)
        {
          template = await _templateRepository.GetByIdWithLayoutAsync(contentType.MultipleContentsTemplateId);
        }

        // Handle paging
        int pageValue = 0;
        Int32.TryParse(page, out pageValue);
        if (pageValue < 1) pageValue = 1;
        ViewData["Page"] = pageValue;

        // Get Contents - include ContentItems and their ContentTypeFields for proper teaser text display
        try
        {
          contentType.Contents = await _contentRepository.GetByContentTypeIdWithDetailsAsync(contentType.ContentTypeId);

          _logger.LogDebug("Loaded {ContentCount} contents for ContentType {ContentTypeId}",
            contentType.Contents.Count, contentType.ContentTypeId);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error loading contents for ContentType {ContentTypeId}", contentType.ContentTypeId);
          // If there's an issue with content loading, just use empty list
          contentType.Contents = new List<Content>();
          ViewData["ErrorMessage"] = "Some content could not be loaded due to data inconsistencies.";
          ViewData["ExceptionDetails"] = ex.Message;
        }

        if (contentType.ItemsPerPage > 0 && contentType.Contents.Any())
        {
          ViewData["OlderContentExists"] = contentType.ItemsPerPage * pageValue < contentType.Contents.Count();
          contentType.Contents = contentType.Contents
              .OrderByDescending(x => x.CreationDate)
              .Skip((pageValue - 1) * contentType.ItemsPerPage)
              .Take(contentType.ItemsPerPage)
              .ToList();

          _logger.LogDebug("Applied paging: Page={Page}, ItemsPerPage={ItemsPerPage}, ResultCount={ResultCount}",
            pageValue, contentType.ItemsPerPage, contentType.Contents.Count);
        }
      }

      ViewData["Layout"] = template?.Layout?.LayoutSource ?? "";

      // Determine view location
      string viewLocationToUse = template?.TemplateSource ?? "/Views/Home/Index.cshtml";

      // If we're looking at individual content and no template was found, try to use a content-specific fallback
      if (pContentUrl.Trim() != "" && template == null)
      {
        // Try to use a content template based on the content type name
        string ctName = contentType.Name.ToLower();
        string fallbackContentTemplate = $"/Views/DSCMS/Templates/Contents/Bootstrap{char.ToUpper(ctName[0])}{ctName.Substring(1)}.cshtml";
        viewLocationToUse = fallbackContentTemplate;
        _logger.LogDebug("Using fallback content template: {ViewLocation}", viewLocationToUse);
      }

      if (string.IsNullOrEmpty(viewLocationToUse))
      {
        viewLocationToUse = "/Views/DSCMS/Templates/Empty.cshtml";
        _logger.LogDebug("Using empty template: {ViewLocation}", viewLocationToUse);
      }

      _logger.LogDebug("Rendering view: {ViewLocation}", viewLocationToUse);

      if (pContentUrl.Trim() != "") // Content was requested
        return View(viewLocationToUse, content);
      else // ContentType was requested
        return View(viewLocationToUse, contentType);
    }
  }
}
