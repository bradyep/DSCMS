using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DSCMS.Controllers;

[Authorize]
public class BackupViewController : Controller
{
  public IActionResult Index()
  {
    return View("~/Views/Backup/Index.cshtml");
  }
}
