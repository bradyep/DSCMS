using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DSCMS.Controllers;

[Authorize]
public class TemplatesViewController : Controller
{
  public IActionResult Index()
  {
    return View("~/Views/Templates/Index.cshtml");
  }

  public IActionResult Details(int id)
  {
    return View("~/Views/Templates/Details.cshtml");
  }

  public IActionResult Create()
  {
    return View("~/Views/Templates/Create.cshtml");
  }

  public IActionResult Edit(int id)
  {
    return View("~/Views/Templates/Edit.cshtml");
  }

  public IActionResult Delete(int id)
  {
    return View("~/Views/Templates/Delete.cshtml");
  }
}
