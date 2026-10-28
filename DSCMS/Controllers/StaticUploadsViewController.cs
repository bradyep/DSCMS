using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DSCMS.Controllers;

[Authorize]
public class StaticUploadsViewController : Controller
{
  public IActionResult Index()
  {
    return View("~/Views/StaticUploads/Index.cshtml");
  }

  public IActionResult Details(int id)
  {
    return View("~/Views/StaticUploads/Details.cshtml");
  }

  public IActionResult Create()
  {
    return View("~/Views/StaticUploads/Create.cshtml");
  }

  public IActionResult Delete(int id)
  {
    return View("~/Views/StaticUploads/Delete.cshtml");
  }
}
