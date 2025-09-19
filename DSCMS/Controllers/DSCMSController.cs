using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DSCMS.Data;
using DSCMS.Models;

namespace DSCMS.Controllers
{
  /// <summary>
  /// Responsible for handling requests related to displaying content types and individual content items.
  /// </summary>
  public class DSCMSController : Controller
  {
    private readonly ApplicationDbContext _context;

    public DSCMSController(ApplicationDbContext context)
    {
      _context = context;
    }

    /// <summary>
    /// Navigates to the content type or specific content item based on the provided parameters
    /// </summary>
    /// <param name="contentTypeName"></param>
    /// <param name="contentUrl"></param>
    /// <param name="page"></param>
    /// <returns></returns>
    public IActionResult Content(string contentTypeName = "blog", string contentUrl = "", string page = "")
    {
      string pContentTypeName = contentTypeName.ToLower();
      string pContentUrl = contentUrl.ToLower();
      Content content = null;
      Template template = null;

      ViewData["ContentTypeName"] = pContentTypeName;
      ViewData["ContentUrl"] = pContentUrl;

      // More robust ContentType lookup with case-insensitive comparison
      ContentType contentType = _context.ContentTypes
        .Include(ct => ct.ContentTypeItems)
        .Where(ct => ct.Name.ToLower() == pContentTypeName).FirstOrDefault();

      // If no content type found, show welcome page for first-time setup
      if (contentType == null)
      {
        ViewData["Title"] = "Welcome to DSCMS";
        return View("~/Views/DSCMS/Welcome.cshtml");
      }

      if (pContentUrl.Trim() != "") // Content was requested
      {
        content = _context.Contents
          .Include(c => c.CreatedByUser)
          .Include(c => c.LastUpdatedByUser)
          .Include(c => c.ContentItems)
          .ThenInclude(ci => ci.ContentTypeItem)
          .Where(c => c.UrlToDisplay == pContentUrl && c.ContentTypeId == contentType.ContentTypeId)
          .FirstOrDefault();
          
        if (content == null) return NotFound();
        content.ContentType = contentType;
        ViewData["Title"] = content.Title ?? "Title";

        // Use content's template, or fall back to ContentType's default template if content has no template
        int templateIdToUse = content.TemplateId > 0 ? content.TemplateId : 
                             (contentType.DefaultTemplateForContent > 0 ? contentType.DefaultTemplateForContent.Value : 0);
        
        // Check if we should display raw content with no template
        if (content.TemplateId == 0 && (contentType.DefaultTemplateForContent == null || contentType.DefaultTemplateForContent == 0))
        {
          // Return raw HTML content with no template
          return new ContentResult
          {
            Content = content.Body ?? "",
            ContentType = "text/html"
          };
        }
        
        if (templateIdToUse > 0)
        {
          template = _context.Templates
            .Include(t => t.Layout)
            .Where(t => t.TemplateId == templateIdToUse).FirstOrDefault();
        }
      }
      else // ContentType was requested
      {
        ViewData["Title"] = contentType.Title ?? "Title";

        if (contentType.TemplateId > 0)
        {
          template = _context.Templates
            .Include(t => t.Layout)
            .Where(t => t.TemplateId == contentType.TemplateId).FirstOrDefault();
        }

        // Handle paging
        int pageValue = 0;
        Int32.TryParse(page, out pageValue);
        if (pageValue < 1) pageValue = 1;
        ViewData["Page"] = pageValue;

        // Get Contents - include ContentItems and their ContentTypeItems for proper teaser text display
        try
        {
          contentType.Contents = _context.Contents
            .Where(c => c.ContentTypeId == contentType.ContentTypeId)
            .Include(c => c.ContentItems)
            .ThenInclude(ci => ci.ContentTypeItem)
            .ToList();
        }
        catch (Exception ex)
        {
          // If there's an issue with content loading, just use empty list
          contentType.Contents = new List<Content>();
          ViewData["ErrorMessage"] = "Some content could not be loaded due to data inconsistencies.";
        }

        if (contentType.ItemsPerPage > 0 && contentType.Contents.Any())
        {
          ViewData["OlderContentExists"] = contentType.ItemsPerPage * pageValue < contentType.Contents.Count();
          contentType.Contents = contentType.Contents
              .OrderByDescending(x => x.CreationDate)
              .Skip((pageValue - 1) * contentType.ItemsPerPage)
              .Take(contentType.ItemsPerPage)
              .ToList();
        }
      }

      ViewData["Layout"] = template?.Layout?.FileLocation ?? "";

      // Determine view location
      string viewLocationToUse = template?.FileLocation ?? "/Views/Home/Index.cshtml";
      
      // If we're looking at individual content and no template was found, try to use a content-specific fallback
      if (pContentUrl.Trim() != "" && template == null)
      {
        // Try to use a content template based on the content type name
        string ctName = contentType.Name.ToLower();
        string fallbackContentTemplate = $"/Views/DSCMS/Templates/Contents/Bootstrap{char.ToUpper(ctName[0])}{ctName.Substring(1)}.cshtml";
        viewLocationToUse = fallbackContentTemplate;
      }
      
      if (string.IsNullOrEmpty(viewLocationToUse))
      {
        viewLocationToUse = "/Views/DSCMS/Templates/Empty.cshtml";
      }

      if (pContentUrl.Trim() != "") // Content was requested
        return View(viewLocationToUse, content);
      else // ContentType was requested
        return View(viewLocationToUse, contentType);
    }
  }
}
