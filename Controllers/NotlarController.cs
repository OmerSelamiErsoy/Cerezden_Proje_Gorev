using Microsoft.AspNetCore.Mvc;

namespace ProjeGorevYonetimi.Controllers;

public class NotlarController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "Notlar";
        return View();
    }

    [HttpGet]
    public IActionResult Yeni()
    {
        ViewData["Title"] = "Yeni Not";
        return View();
    }

    [HttpGet]
    public IActionResult Detay(int id)
    {
        ViewData["Title"] = "Not Detay";
        ViewData["NotId"] = id;
        return View();
    }
}

