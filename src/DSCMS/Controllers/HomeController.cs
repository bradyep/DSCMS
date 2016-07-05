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

        public IActionResult ContentType(string name)
        {
            // Figure out which layout, title and view page to use
            ContentType contentType = _context.ContentTypes.Where(ct => ct.Name == name).FirstOrDefault();
            if (contentType == null) return NotFound();

            string layoutToUse = contentType.Template.Layout.FileLocation ?? "_Layout";
            ViewData["Layout"] = layoutToUse;

            string contentTypePageTitle = contentType.Title ?? "Title";
            ViewData["Title"] = contentTypePageTitle;

            string viewLocationToUse = contentType.Template.FileLocation ?? "/Views/Home/Index.cshtml";

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
