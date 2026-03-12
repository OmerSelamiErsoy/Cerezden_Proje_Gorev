using Microsoft.AspNetCore.Mvc;

namespace ProjeGorevYonetimi.Controllers;

public class ParametrelerController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "Parametreler";
        return View();
    }
}
