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
    private readonly IYetkiService _yetki;

    public DashboardController(ApplicationDbContext db, IProjeService projeService, ICurrentUserService currentUser, IYetkiService yetki)
    {
        _db = db;
        _projeService = projeService;
        _currentUser = currentUser;
        _yetki = yetki;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get(CancellationToken ct)
    {
        var projeler = await _projeService.GetVisibleProjelerAsync(ct);
        var aktifProjeler = projeler
            .Where(p => p.ProjeDurumAd != "Tamamlandı" && p.ProjeDurumAd != "İptal Edildi")
            .ToList();

        var userId = _currentUser.GetCurrentUserId();
        var gorevIds = await _db.Gorevler.AsNoTracking().Where(g => g.OlusturanKullaniciId == userId).Select(g => g.Id).ToListAsync(ct);
        var atananIds = await _db.GorevAtamalar.AsNoTracking().Where(a => a.KullaniciId == userId).Select(a => a.GorevId).ToListAsync(ct);
        var tumGorevIds = gorevIds.Union(atananIds).Distinct().ToList();

        var gorevlerAktif = await _db.Gorevler
            .AsNoTracking()
            .Where(g => tumGorevIds.Contains(g.Id) && g.DurumId != 4 && g.DurumId != 5)
            .Include(g => g.Durum)
            .ToListAsync(ct);

        var gorevDurumOzeti = gorevlerAktif
            .GroupBy(g => new { g.DurumId, DurumAd = g.Durum?.Ad ?? "" })
            .OrderBy(g => g.Key.DurumId)
            .Select(g => new GorevDurumOzetDto { DurumAd = g.Key.DurumAd, Adet = g.Count() })
            .ToList();

        var gorevGrubuSayisi = await _db.GorevGruplar.AsNoTracking().CountAsync(ct);
        var meGenelYetkiliMi = _yetki.GenelYetkiliMi();

        return Ok(new DashboardDto
        {
            ProjeSayisi = projeler.Count,
            AktifProjeler = aktifProjeler,
            GorevDurumOzeti = gorevDurumOzeti,
            GorevGrubuSayisi = gorevGrubuSayisi,
            MeGenelYetkiliMi = meGenelYetkiliMi
        });
    }
}

public class DashboardDto
{
    public int ProjeSayisi { get; set; }
    public List<ProjeGorevYonetimi.Services.ProjeListDto> AktifProjeler { get; set; } = new();
    public List<GorevDurumOzetDto> GorevDurumOzeti { get; set; } = new();
    public int GorevGrubuSayisi { get; set; }
    public bool MeGenelYetkiliMi { get; set; }
}

public class GorevDurumOzetDto
{
    public string DurumAd { get; set; } = "";
    public int Adet { get; set; }
}
