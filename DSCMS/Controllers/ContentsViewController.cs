using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DSCMS.Controllers;

/// <summary>
/// MVC controller serving Razor shell pages for Contents UI.
/// The actual data and operations are handled by the ContentsController API.
/// </summary>
[Authorize]
public class ContentsViewController : Controller
{
  // GET: /Contents
  public IActionResult Index()
  {
    return View("~/Views/Contents/Index.cshtml");
  }

  // GET: /Contents/Details/{id}
  public IActionResult Details(int id)
  {
    ViewData["ContentId"] = id;
    return View("~/Views/Contents/Details.cshtml");
  }

  // GET: /Contents/Create
  public IActionResult Create()
  {
    return View("~/Views/Contents/Create.cshtml");
  }

  // GET: /Contents/Edit/{id}
  public IActionResult Edit(int id)
  {
    ViewData["ContentId"] = id;
    return View("~/Views/Contents/Edit.cshtml");
  }

  // GET: /Contents/Delete/{id}
  public IActionResult Delete(int id)
  {
    ViewData["ContentId"] = id;
    return View("~/Views/Contents/Delete.cshtml");
  }
}
