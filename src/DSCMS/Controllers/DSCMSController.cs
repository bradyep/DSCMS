using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DSCMS.Data;
using DSCMS.Models;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace DSCMS.Controllers
{
  public class DSCMSController : Controller
  {
    private readonly ApplicationDbContext _context;

    public DSCMSController(ApplicationDbContext context)
    {
      _context = context;
    }

    public IActionResult Content(string contentTypeName, string contentUrl = "", string page = "") 
    {
      string pContentUrl = contentUrl.ToLower();
      Content content = null;
      Template template;

      ViewData["ContentTypeName"] = contentTypeName;
      ViewData["ContentUrl"] = pContentUrl;

      ContentType contentType = _context.ContentTypes
        .Include(ct => ct.ContentTypeItems)
        .Where(ct => ct.Name == contentTypeName).FirstOrDefault();
      if (contentType == null) return NotFound();

      if (pContentUrl.Trim() != "") // Content was requested
      { 
        content = _context.Contents
          .Include(c => c.CreatedByUser)
          .Include(c => c.LastUpdatedByUser)
          .Include(c => c.ContentItems)
          .Where(c => c.UrlToDisplay == pContentUrl && c.ContentTypeId == contentType.ContentTypeId)
          .FirstOrDefault();
        if (content == null) return NotFound();
        content.ContentType = contentType;
        ViewData["Title"] = content.Title ?? "Title";
        template = _context.Templates
          .Include(t => t.Layout)
          .Where(t => t.TemplateId == content.TemplateId).FirstOrDefault();
      }
      else // ContentType was requested
      {
        ViewData["Title"] = contentType.Title ?? "Title";
        template = _context.Templates
          .Include(t => t.Layout)
          .Where(t => t.TemplateId == contentType.TemplateId).FirstOrDefault();
        // Handle paging
        int pageValue = 0;
        Int32.TryParse(page, out pageValue);
        ViewData["Page"] = pageValue;
        // Handle associated Contents
        if (pageValue < 1 || contentType.ItemsPerPage < 1) // Get all Contents
          contentType.Contents = _context.Contents
            .Include(c => c.CreatedByUser)
            .Include(c => c.LastUpdatedByUser)
            .Include(c => c.ContentItems)
            .Where(c => c.ContentTypeId == contentType.ContentTypeId).ToList();
        else // Get Contents based on page
          contentType.Contents = _context.Contents
            .Include(c => c.CreatedByUser)
            .Include(c => c.LastUpdatedByUser)
            .Include(c => c.ContentItems)
            .Where(c => c.ContentTypeId == contentType.ContentTypeId)
            .Skip((pageValue - 1) * contentType.ItemsPerPage)
            .Take(contentType.ItemsPerPage)
            .ToList();
      }

      ViewData["Layout"] = template.Layout.FileLocation ?? "_Layout";

      string viewLocationToUse = template.FileLocation ?? "/Views/Home/Index.cshtml";

      if (pContentUrl.Trim() != "") // Content was requested
        return View(viewLocationToUse, content);
      else // ContentType was requested
        return View(viewLocationToUse, contentType);
    }
  }
}
