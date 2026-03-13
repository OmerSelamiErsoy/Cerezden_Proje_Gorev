using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjeGorevYonetimi.Data;

namespace ProjeGorevYonetimi.Controllers;

public class GorevlerController : Controller
{
    private readonly ApplicationDbContext _db;

    public GorevlerController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "Görevler";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GrupDetay(int id)
    {
        var grup = await _db.GorevGruplar.FirstOrDefaultAsync(g => g.Id == id);
        var grupAd = grup?.Ad ?? $"Grup #{id}";

        ViewData["Title"] = $"Görev Grubu - {grupAd}";
        ViewData["GrupId"] = id;
        ViewData["GrupAd"] = grupAd;
        return View();
    }
}
