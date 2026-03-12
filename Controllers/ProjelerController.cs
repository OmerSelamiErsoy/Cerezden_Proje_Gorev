using Microsoft.AspNetCore.Mvc;

namespace ProjeGorevYonetimi.Controllers;

public class ProjelerController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "Projeler";
        return View();
    }

    [HttpGet]
    public IActionResult Detay(int id)
    {
        ViewData["Title"] = "Proje Detay";
        ViewData["ProjeId"] = id;
        return View();
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "Yeni Proje";
        return View();
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        ViewData["Title"] = "Proje Düzenle";
        ViewData["ProjeId"] = id;
        return View();
    }
}
