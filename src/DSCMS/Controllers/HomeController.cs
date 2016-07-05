using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DSCMS.Data;
using DSCMS.Models;

namespace DSCMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ContentType(string id = "Blog")
        {
            ContentType contentType = _context.ContentTypes.Where(ct => ct.Name == id).FirstOrDefault();
            if (contentType == null) return NotFound();

            ViewData["Title"] = contentType.Title ?? "Title";

            Template template = _context.Templates.Where(t => t.TemplateId == contentType.TemplateId).FirstOrDefault();
            Layout layout = _context.Layouts.Where(l => l.LayoutId == template.LayoutId).FirstOrDefault();
            ViewData["Layout"] = layout.FileLocation ?? "_Layout";

            string viewLocationToUse = template.FileLocation ?? "/Views/Home/Index.cshtml";

            return View(viewLocationToUse);
        }

        public IActionResult Content(int contentId)
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
