using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DSCMS.Controllers;

[Authorize]
public class ContentTypesViewController : Controller
{
  public IActionResult Index()
  {
    return View("~/Views/ContentTypes/Index.cshtml");
  }

  public IActionResult Details(int id)
  {
    return View("~/Views/ContentTypes/Details.cshtml");
  }

  public IActionResult Create()
  {
    return View("~/Views/ContentTypes/Create.cshtml");
  }

  public IActionResult Edit(int id)
  {
    return View("~/Views/ContentTypes/Edit.cshtml");
  }

  public IActionResult Delete(int id)
  {
    return View("~/Views/ContentTypes/Delete.cshtml");
  }
}
