using Microsoft.AspNetCore.Mvc;

namespace ProjeGorevYonetimi.Controllers;

public class SahaDenetimController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "Saha Denetim";
        return View();
    }

    [HttpGet]
    public IActionResult Yeni()
    {
        ViewData["Title"] = "Yeni Denetim";
        return View();
    }

    [HttpGet]
    public IActionResult Detay(int id)
    {
        ViewData["Title"] = "Denetim Detay";
        ViewData["DenetimId"] = id;
        return View();
    }
}

