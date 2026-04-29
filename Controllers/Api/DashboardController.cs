using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjeGorevYonetimi.Data;
using ProjeGorevYonetimi.Models.Entities;
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
        var genelYetkiliMi = _yetki.GenelYetkiliMi();

        // Notlar
        IQueryable<Not> notQuery = _db.Notlar
            .AsNoTracking()
            .Include(n => n.YetkiKullanicilar)
            .Include(n => n.OlusturanKullanici)
            .AsQueryable();
        if (!genelYetkiliMi)
        {
            notQuery = notQuery.Where(n =>
                n.OlusturanKullaniciId == userId ||
                (n.YetkiTipi == 1 && n.YetkiKullanicilar!.Any(y => y.KullaniciId == userId)));
        }

        var notToplam = await notQuery.CountAsync(ct);
        var notPaylasimli = await notQuery.Where(n => n.YetkiTipi == 1).CountAsync(ct);
        var notSadeceBen = notToplam - notPaylasimli;
        var sonNotlar = await notQuery
            .OrderByDescending(n => n.UpdateDate ?? n.InsertDate)
            .ThenByDescending(n => n.Id)
            .Select(n => new DashboardNotItemDto
            {
                Id = n.Id,
                Baslik = n.Baslik,
                YetkiTipi = n.YetkiTipi,
                InsertDate = n.InsertDate,
                OlusturanAdSoyad = n.OlusturanKullanici.AdSoyad
            })
            .Take(5)
            .ToListAsync(ct);

        // Saha denetim
        var sdQuery = _db.SahaDenetimler
            .AsNoTracking()
            .Include(x => x.OlusturanKullanici)
            .Include(x => x.YetkiKullanicilar)
            .AsQueryable();

        // Yetki:
        // - Genel paylaşım: tüm kullanıcılar
        // - Kişi bazlı paylaşım: seçili kişi(ler) + oluşturan kişi
        if (!genelYetkiliMi)
        {
            sdQuery = sdQuery.Where(x =>
                x.YetkiTipi == 0 ||
                x.OlusturanKullaniciId == userId ||
                (x.YetkiTipi == 1 && x.YetkiKullanicilar!.Any(y => y.KullaniciId == userId)));
        }

        var sdToplam = await sdQuery.CountAsync(ct);
        var sdAcik = await sdQuery.Where(x => !x.KapaliMi).CountAsync(ct);
        var sdKapali = sdToplam - sdAcik;
        var sonDenetimler = await sdQuery
            .OrderByDescending(x => x.KayitTarihi)
            .ThenByDescending(x => x.Id)
            .Select(x => new DashboardDenetimItemDto
            {
                Id = x.Id,
                Ad = x.Ad,
                KayitTarihi = x.KayitTarihi,
                KapaliMi = x.KapaliMi,
                LokasyonId = x.LokasyonId,
                LokasyonAdi = x.LokasyonAdi,
                OlusturanAdSoyad = x.OlusturanKullanici.AdSoyad,
                AdimSayisi = _db.SahaDenetimAdimlar.Count(a => a.SahaDenetimId == x.Id),
                YetkiTipi = x.YetkiTipi
            })
            .Take(5)
            .ToListAsync(ct);

        List<int> tumGorevIds = new();
        if (!genelYetkiliMi)
        {
            var sorumluIds = await _db.Gorevler
                .AsNoTracking()
                .Where(g => !g.IsDeleted && g.SorumluKullaniciId == userId)
                .Select(g => g.Id)
                .ToListAsync(ct);
            var paylasilanIds = await _db.GorevAtamalar
                .AsNoTracking()
                .Where(a => a.KullaniciId == userId)
                .Select(a => a.GorevId)
                .ToListAsync(ct);
            tumGorevIds = sorumluIds.Union(paylasilanIds).Distinct().ToList();
        }

        var kapaliDurumIdler = await _db.Durumlar
            .AsNoTracking()
            .Where(d => d.Ad == "İptal Edildi" || d.Ad == "Tamamlandı")
            .Select(d => d.Id)
            .ToListAsync(ct);

        IQueryable<ProjeGorevYonetimi.Models.Entities.Gorev> gorevlerQuery = _db.Gorevler
            .AsNoTracking()
            .Where(g => !g.IsDeleted && !kapaliDurumIdler.Contains(g.DurumId));
        if (!genelYetkiliMi)
        {
            gorevlerQuery = gorevlerQuery.Where(g => tumGorevIds.Contains(g.Id));
        }
        var gorevlerAktif = await gorevlerQuery
            .Include(g => g.Durum)
            .ToListAsync(ct);

        var gorevDurumOzeti = gorevlerAktif
            .GroupBy(g => new { g.DurumId, DurumAd = g.Durum?.Ad ?? "" })
            .OrderBy(g => g.Key.DurumId)
            .Select(g => new GorevDurumOzetDto { DurumAd = g.Key.DurumAd, Adet = g.Count() })
            .ToList();

        var gorevGrubuSayisi = await _db.GorevGruplar.AsNoTracking().CountAsync(ct);

        return Ok(new DashboardDto
        {
            ProjeSayisi = projeler.Count,
            AktifProjeler = aktifProjeler,
            GorevDurumOzeti = gorevDurumOzeti,
            GorevGrubuSayisi = gorevGrubuSayisi,
            MeGenelYetkiliMi = genelYetkiliMi,
            Notlar = new DashboardNotOzetDto
            {
                Toplam = notToplam,
                SadeceBen = notSadeceBen,
                Paylasimli = notPaylasimli,
                SonNotlar = sonNotlar
            },
            SahaDenetim = new DashboardSahaDenetimOzetDto
            {
                Toplam = sdToplam,
                Acik = sdAcik,
                Kapali = sdKapali,
                SonDenetimler = sonDenetimler
            }
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
    public DashboardNotOzetDto Notlar { get; set; } = new();
    public DashboardSahaDenetimOzetDto SahaDenetim { get; set; } = new();
}

public class GorevDurumOzetDto
{
    public string DurumAd { get; set; } = "";
    public int Adet { get; set; }
}

public class DashboardNotOzetDto
{
    public int Toplam { get; set; }
    public int SadeceBen { get; set; }
    public int Paylasimli { get; set; }
    public List<DashboardNotItemDto> SonNotlar { get; set; } = new();
}

public class DashboardNotItemDto
{
    public int Id { get; set; }
    public string Baslik { get; set; } = "";
    public int YetkiTipi { get; set; }
    public DateTime? InsertDate { get; set; }
    public string? OlusturanAdSoyad { get; set; }
}

public class DashboardSahaDenetimOzetDto
{
    public int Toplam { get; set; }
    public int Acik { get; set; }
    public int Kapali { get; set; }
    public List<DashboardDenetimItemDto> SonDenetimler { get; set; } = new();
}

public class DashboardDenetimItemDto
{
    public int Id { get; set; }
    public string Ad { get; set; } = "";
    public DateTime KayitTarihi { get; set; }
    public bool KapaliMi { get; set; }
    public int? LokasyonId { get; set; }
    public string? LokasyonAdi { get; set; }
    public string? OlusturanAdSoyad { get; set; }
    public int AdimSayisi { get; set; }
    public int YetkiTipi { get; set; }
}
