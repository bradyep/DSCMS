using System;
using System.Collections.Generic;
using System.Linq;
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
      Content content = null;
      Template template;

      ViewData["ContentTypeName"] = contentTypeName;
      ViewData["ContentUrl"] = contentUrl;

      ContentType contentType = _context.ContentTypes.Where(ct => ct.Name == contentTypeName).FirstOrDefault();
      if (contentType == null) return NotFound();

      if (contentUrl.Trim() != "")
      { // Content was requested
        content = _context.Contents.Where(c => c.UrlToDisplay == contentUrl && c.ContentTypeId == contentType.ContentTypeId).FirstOrDefault();
        if (content == null) return NotFound();
        content.ContentType = contentType;
        content.CreatedByUser = _context.Users.Where(u => u.UserId == content.CreatedBy).FirstOrDefault();
        content.LastUpdatedByUser = _context.Users.Where(u => u.UserId == content.LastUpdatedBy).FirstOrDefault();
        ViewData["Title"] = content.Title ?? "Title";
        template = _context.Templates.Where(t => t.TemplateId == content.TemplateId).FirstOrDefault();
        // Handle ContentItems
        content.ContentType.ContentTypeItems = _context.ContentTypeItems.Where(cti => cti.ContentTypeId == contentType.ContentTypeId).ToList();
        content.ContentItems = _context.ContentItems
            .Where(ci => content.ContentType.ContentTypeItems.Select(cti => cti.ContentTypeItemId)
            .Contains(ci.ContentTypeItemId) && ci.ContentId == content.ContentId)
            .ToList();
      }
      else // ContentType was requested
      {
        ViewData["Title"] = contentType.Title ?? "Title";
        template = _context.Templates.Where(t => t.TemplateId == contentType.TemplateId).FirstOrDefault();
        contentType.ContentTypeItems = _context.ContentTypeItems.Where(cti => cti.ContentTypeId == contentType.ContentTypeId).ToList();
        // Handle paging
        int pageValue = 0;
        Int32.TryParse(page, out pageValue);
        ViewData["Page"] = pageValue;
        // Handle associated Contents
        if (pageValue < 1 || contentType.ItemsPerPage < 1) // Get all Contents
          contentType.Contents = _context.Contents.Where(c => c.ContentTypeId == contentType.ContentTypeId).ToList();
        else // Get Contents based on page
          contentType.Contents = _context.Contents.Where(c => c.ContentTypeId == contentType.ContentTypeId)
            .Skip((pageValue - 1) * contentType.ItemsPerPage)
            .Take(contentType.ItemsPerPage)
            .ToList();
        foreach (Content contentItem in contentType.Contents)
        {
          contentItem.CreatedByUser = _context.Users.Where(u => u.UserId == contentItem.CreatedBy).FirstOrDefault();
          contentItem.LastUpdatedByUser = _context.Users.Where(u => u.UserId == contentItem.LastUpdatedBy).FirstOrDefault();
          contentItem.ContentItems = _context.ContentItems
              .Where(ci => contentType.ContentTypeItems.Select(cti => cti.ContentTypeItemId).Contains(ci.ContentTypeItemId) && ci.ContentId == contentItem.ContentId)
              .ToList();
        }
      }

      Layout layout = _context.Layouts.Where(l => l.LayoutId == template.LayoutId).FirstOrDefault();
      ViewData["Layout"] = layout.FileLocation ?? "_Layout";

      string viewLocationToUse = template.FileLocation ?? "/Views/Home/Index.cshtml";

      if (contentUrl.Trim() != "") // Content was requested
        return View(viewLocationToUse, content);
      else // ContentType was requested
        return View(viewLocationToUse, contentType);
    }
  }
}
