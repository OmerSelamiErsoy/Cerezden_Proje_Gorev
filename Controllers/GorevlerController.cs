using Microsoft.AspNetCore.Mvc;

namespace ProjeGorevYonetimi.Controllers;

public class GorevlerController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "Görevler";
        return View();
    }

    [HttpGet]
    public IActionResult GrupDetay(int id)
    {
        ViewData["Title"] = "Görev Grubu";
        ViewData["GrupId"] = id;
        return View();
    }
}
