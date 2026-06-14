using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DSCMS.Controllers;

[Authorize]
public class LayoutsViewController : Controller
{
  public IActionResult Index()
  {
    return View("~/Views/Layouts/Index.cshtml");
  }

  public IActionResult Details(int id)
  {
    return View("~/Views/Layouts/Details.cshtml");
  }

  public IActionResult Create()
  {
    return View("~/Views/Layouts/Create.cshtml");
  }

  public IActionResult Edit(int id)
  {
    return View("~/Views/Layouts/Edit.cshtml");
  }

  public IActionResult Delete(int id)
  {
    return View("~/Views/Layouts/Delete.cshtml");
  }
}
