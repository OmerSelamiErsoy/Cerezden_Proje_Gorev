using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjeGorevYonetimi.Data;
using ProjeGorevYonetimi.Models.Entities;

namespace ProjeGorevYonetimi.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[ServiceFilter(typeof(Filters.ApiKeyAuthFilter))]
public class RaporController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;

    public RaporController(ApplicationDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    /// <summary>
    /// Görev Listesi - /Gorevler sayfasındaki "Toplam aktif görev adetleri" ile aynı yetkilendirme mantığı.
    /// </summary>
    [HttpGet("gorev-listesi")]
    public async Task<ActionResult<List<GorevRaporDto>>> GorevListesi([FromQuery] int kullaniciId, CancellationToken ct)
    {
        var user = await _db.Kullanicilar.AsNoTracking().FirstOrDefaultAsync(k => k.CerezdenKullaniciId == kullaniciId, ct);
        if (user == null)
            return Ok(new List<GorevRaporDto>());

        var userId = user.Id;
        var genelYetkili = GenelYetkiliMiForCerezden(kullaniciId);

        var hedefDurumAdlari = new[] { "İşlem Bekliyor", "Beklemede", "İşleme Alındı" };
        var durumlar = await _db.Durumlar
            .AsNoTracking()
            .Where(d => hedefDurumAdlari.Contains(d.Ad))
            .Select(d => new { d.Id, d.Ad })
            .ToListAsync(ct);
        var durumIdler = durumlar.Select(d => d.Id).ToList();
        if (!durumIdler.Any())
            return Ok(new List<GorevRaporDto>());

        var gorevlerQuery = _db.Gorevler
            .AsNoTracking()
            .Where(g => !g.IsDeleted
                        && durumIdler.Contains(g.DurumId));

        // GeneralAuthority ise tüm görevleri; değilse sorumlu olduğum + paylaşılanlarda olduğum görevleri getir.
        if (!genelYetkili)
        {
            gorevlerQuery = gorevlerQuery.Where(g =>
                g.SorumluKullaniciId == userId || g.Atamalar!.Any(a => a.KullaniciId == userId));
        }

        var gorevler = await gorevlerQuery
            .Include(g => g.GorevGrubu)
            .Include(g => g.Durum)
            .Include(g => g.SorumluKullanici)
            .Include(g => g.Yorumlar)
            .OrderBy(g => g.OlusturmaTarihi)
            .ToListAsync(ct);

        var result = gorevler.Select(g => new GorevRaporDto
        {
            GorevId = g.Id,
            GorevAdi = g.Ad,
            GorevGrubuId = g.GorevGrubuId,
            GorevGrubuAdi = g.GorevGrubu != null ? g.GorevGrubu.Ad : null,
            Sorumlu = g.SorumluKullanici?.AdSoyad,
            IsSorumluKendim = g.SorumluKullaniciId == userId,
            DurumAdi = g.Durum?.Ad ?? "",
            DurumId = g.DurumId,
            IsMesaj = g.Yorumlar != null && g.Yorumlar.Count > 0,
            Aciklama = g.Aciklama,
            IsAciklama = !string.IsNullOrWhiteSpace(g.Aciklama),
            KayitTarihi = g.OlusturmaTarihi
        }).ToList();

        return Ok(result);
    }

    /// <summary>
    /// Proje Listesi - /Projeler sayfasındaki yetkilendirmeye göre görünür projeler.
    /// </summary>
    [HttpGet("proje-listesi")]
    public async Task<ActionResult<List<ProjeRaporDto>>> ProjeListesi([FromQuery] int kullaniciId, CancellationToken ct)
    {
        var user = await _db.Kullanicilar.AsNoTracking().FirstOrDefaultAsync(k => k.CerezdenKullaniciId == kullaniciId, ct);
        if (user == null)
            return Ok(new List<ProjeRaporDto>());

        var userId = user.Id;
        var genelYetkili = GenelYetkiliMiForCerezden(kullaniciId);

        var query = _db.Projeler
            .AsNoTracking()
            .Include(p => p.ProjeDurum)
            .Include(p => p.SorumluKullanici)
            .Include(p => p.Detaylar);

        List<Proje> projeler;
        if (genelYetkili)
        {
            projeler = await query
                .OrderByDescending(p => p.Id)
                .ToListAsync(ct);
        }
        else
        {
            var kisiProjeler = await _db.ProjeYetkiKullanicilar
                .Where(y => y.KullaniciId == userId)
                .Select(y => y.ProjeId)
                .ToListAsync(ct);

            projeler = await query
                .Where(p => p.YetkiTipi == 0 || kisiProjeler.Contains(p.Id))
                .OrderByDescending(p => p.Id)
                .ToListAsync(ct);
        }

        var result = new List<ProjeRaporDto>();
        foreach (var p in projeler)
        {
            var toplam = p.Detaylar?.Count ?? 0;
            var tamamlanan = p.Detaylar?.Count(d => d.DurumId == 4 || d.DurumId == 5) ?? 0; // İptal=4, Tamamlandı=5
            var yuzde = toplam == 0 ? 0 : (int)Math.Round(100.0 * tamamlanan / toplam);

            result.Add(new ProjeRaporDto
            {
                ProjeId = p.Id,
                ProjeAdi = p.Ad,
                DurumAdi = p.ProjeDurum?.Ad ?? "",
                DurumId = p.ProjeDurumId,
                IlerlemeYuzde = yuzde,
                PlanlananBitisTarihi = p.PlanlananBitisTarihi,
                SorumluKisi = p.SorumluKullanici?.AdSoyad
            });
        }

        return Ok(result);
    }

    private bool GenelYetkiliMiForCerezden(int cerezdenKullaniciId)
    {
        var ids = _config["GeneralAuthority"]?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s.Trim(), out var n) ? n : (int?)null)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ToHashSet() ?? new HashSet<int>();
        return ids.Contains(cerezdenKullaniciId);
    }
}

public class GorevRaporDto
{
    public int GorevId { get; set; }
    public string GorevAdi { get; set; } = "";
    public int? GorevGrubuId { get; set; }
    public string? GorevGrubuAdi { get; set; }
    public string? Sorumlu { get; set; }
    public bool IsSorumluKendim { get; set; }
    public string DurumAdi { get; set; } = "";
    public int DurumId { get; set; }
    public bool IsMesaj { get; set; }
    public string? Aciklama { get; set; }
    public bool IsAciklama { get; set; }
    public DateTime KayitTarihi { get; set; }
}

public class ProjeRaporDto
{
    public int ProjeId { get; set; }
    public string ProjeAdi { get; set; } = "";
    public string DurumAdi { get; set; } = "";
    public int DurumId { get; set; }
    public int IlerlemeYuzde { get; set; }
    public DateTime? PlanlananBitisTarihi { get; set; }
    public string? SorumluKisi { get; set; }
}

