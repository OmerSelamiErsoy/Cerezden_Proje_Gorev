using Microsoft.AspNetCore.Mvc;

namespace ProjeGorevYonetimi.Controllers;

public class SablonlarController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "Proje Şablonları";
        return View();
    }

    [HttpGet]
    public IActionResult Detay(int id)
    {
        ViewData["Title"] = "Şablon Detay";
        ViewData["SablonId"] = id;
        return View();
    }
}
