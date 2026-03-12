using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjeGorevYonetimi.Data;
using ProjeGorevYonetimi.Services;

namespace ProjeGorevYonetimi.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[ServiceFilter(typeof(Filters.ApiKeyAuthFilter))]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IProjeService _projeService;
    private readonly ICurrentUserService _currentUser;

    public DashboardController(ApplicationDbContext db, IProjeService projeService, ICurrentUserService currentUser)
    {
        _db = db;
        _projeService = projeService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get(CancellationToken ct)
    {
        var projeler = await _projeService.GetVisibleProjelerAsync(ct);
        var aktifProjeler = projeler
            .Where(p => p.ProjeDurumAd != "Tamamlandı" && p.ProjeDurumAd != "İptal Edildi")
            .ToList();

        var userId = _currentUser.GetCurrentUserId();
        var gorevIds = await _db.Gorevler.Where(g => g.OlusturanKullaniciId == userId).Select(g => g.Id).ToListAsync(ct);
        var atananIds = await _db.GorevAtamalar.Where(a => a.KullaniciId == userId).Select(a => a.GorevId).ToListAsync(ct);
        var tumGorevIds = gorevIds.Union(atananIds).Distinct().ToList();

        var gorevlerAktif = await _db.Gorevler
            .Where(g => tumGorevIds.Contains(g.Id) && g.DurumId != 4 && g.DurumId != 5)
            .Include(g => g.Durum)
            .ToListAsync(ct);

        var gorevDurumOzeti = gorevlerAktif
            .GroupBy(g => new { g.DurumId, DurumAd = g.Durum?.Ad ?? "" })
            .OrderBy(g => g.Key.DurumId)
            .Select(g => new GorevDurumOzetDto { DurumAd = g.Key.DurumAd, Adet = g.Count() })
            .ToList();

        return Ok(new DashboardDto
        {
            ProjeSayisi = projeler.Count,
            AktifProjeler = aktifProjeler,
            GorevDurumOzeti = gorevDurumOzeti
        });
    }
}

public class DashboardDto
{
    public int ProjeSayisi { get; set; }
    public List<ProjeGorevYonetimi.Services.ProjeListDto> AktifProjeler { get; set; } = new();
    public List<GorevDurumOzetDto> GorevDurumOzeti { get; set; } = new();
}

public class GorevDurumOzetDto
{
    public string DurumAd { get; set; } = "";
    public int Adet { get; set; }
}
