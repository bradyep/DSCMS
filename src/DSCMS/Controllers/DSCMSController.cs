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

        /*
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }
        */

        new public IActionResult Content(string contentTypeName, string contentUrl = "") // The id here is Content.UrlToDisplay
        {
            Content content = null;
            Template template;

            ContentType contentType = _context.ContentTypes.Where(ct => ct.Name == contentTypeName).FirstOrDefault();
            if (contentType == null) return NotFound();

            if (contentUrl.Trim() != "") { // Content was requested
                content = _context.Contents.Where(c => c.UrlToDisplay == contentUrl && c.ContentTypeId == contentType.ContentTypeId).FirstOrDefault();
                if (content == null) return NotFound();
                content.ContentType = contentType;
                ViewData["Title"] = content.Title ?? "Title";
                template = _context.Templates.Where(t => t.TemplateId == content.TemplateId).FirstOrDefault();
                // Handle ContentItems
                content.ContentType.ContentTypeItems = _context.ContentTypeItems.Where(cti => cti.ContentTypeId == contentType.ContentTypeId).ToList();
                content.ContentItems = _context.ContentItems
                    .Where(ci => content.ContentType.ContentTypeItems.Select(cti => cti.ContentTypeItemId).Contains(ci.ContentTypeItemId) && ci.ContentId == content.ContentId)
                    .ToList();
            }
            else // ContentType was requested
            {
                ViewData["Title"] = contentType.Title ?? "Title";
                template = _context.Templates.Where(t => t.TemplateId == contentType.TemplateId).FirstOrDefault();
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
