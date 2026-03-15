using Microsoft.AspNetCore.Mvc;
using ProjeGorevYonetimi.Services;

namespace ProjeGorevYonetimi.Controllers;

public class ParametrelerController : Controller
{
    private readonly IYetkiService _yetkiService;

    public ParametrelerController(IYetkiService yetkiService)
    {
        _yetkiService = yetkiService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "Parametreler";
        ViewBag.GenelYetkiliMi = _yetkiService.GenelYetkiliMi();
        return View();
    }
}
