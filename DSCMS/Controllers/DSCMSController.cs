﻿using System;
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

    public IActionResult Content(string contentTypeName = "blog", string contentUrl = "", string page = "") 
    {
      string pContentTypeName = contentTypeName.ToLower();
      string pContentUrl = contentUrl.ToLower();
      Content content = null;
      Template template;

      ViewData["ContentTypeName"] = pContentTypeName;
      ViewData["ContentUrl"] = pContentUrl;

      ContentType contentType = _context.ContentTypes
        .Include(ct => ct.ContentTypeItems)
        .Where(ct => ct.Name == pContentTypeName).FirstOrDefault();
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
        if (pageValue < 1) pageValue = 1;
        ViewData["Page"] = pageValue;
        // Handle associated Contents
        /*
        if (pageValue < 1 || contentType.ItemsPerPage < 1) // Get all Contents
          contentType.Contents = _context.Contents
            .Include(c => c.CreatedByUser)
            .Include(c => c.LastUpdatedByUser)
            .Include(c => c.ContentItems)
            .Where(c => c.ContentTypeId == contentType.ContentTypeId).ToList();
            */
        // else // Get Contents based on page
        contentType.Contents = _context.Contents
          .Where(c => c.ContentTypeId == contentType.ContentTypeId)
          .Include(c => c.CreatedByUser)
          .Include(c => c.LastUpdatedByUser)
          .Include(c => c.ContentItems)
          .ToList();
        if (contentType.ItemsPerPage > 0)
        { 
        ViewData["OlderContentExists"] = contentType.ItemsPerPage * pageValue < contentType.Contents.Count();
        contentType.Contents = contentType.Contents
            .OrderByDescending(x => x.CreationDate)
            .Skip((pageValue - 1) * contentType.ItemsPerPage)
            .Take(contentType.ItemsPerPage)
            .ToList();
        }
      }

      ViewData["Layout"] = template != null ? template.Layout.FileLocation ?? null : null;

      // string viewLocationToUse = template.FileLocation ?? "/Views/Home/Index.cshtml";
      string viewLocationToUse = template != null ? template.FileLocation ?? "/Views/Home/Index.cshtml" : "/Views/DSCMS/Templates/Empty.cshtml";

      if (pContentUrl.Trim() != "") // Content was requested
        return View(viewLocationToUse, content);
      else // ContentType was requested
        return View(viewLocationToUse, contentType);
    }
  }
}
