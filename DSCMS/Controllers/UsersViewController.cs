using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DSCMS.Controllers;

[Authorize]
public class UsersViewController : Controller
{
  public IActionResult Index()
  {
    return View("~/Views/Users/Index.cshtml");
  }

  public IActionResult Details(string id)
  {
    return View("~/Views/Users/Details.cshtml");
  }

  public IActionResult Create()
  {
    return View("~/Views/Users/Create.cshtml");
  }

  public IActionResult Edit(string id)
  {
    return View("~/Views/Users/Edit.cshtml");
  }

  public IActionResult Delete(string id)
  {
    return View("~/Views/Users/Delete.cshtml");
  }
}
