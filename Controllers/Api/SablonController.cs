using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjeGorevYonetimi.Data;
using ProjeGorevYonetimi.Models.Entities;
using ProjeGorevYonetimi.Services;

namespace ProjeGorevYonetimi.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[ServiceFilter(typeof(Filters.ApiKeyAuthFilter))]
public class SablonController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IProjeService _projeService;
    private readonly ICurrentUserService _currentUser;

    public SablonController(ApplicationDbContext db, IProjeService projeService, ICurrentUserService currentUser)
    {
        _db = db;
        _projeService = projeService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<List<SablonListDto>>> List(CancellationToken ct)
    {
        var list = await _db.ProjeSablonlar
            .AsNoTracking()
            .OrderByDescending(s => s.Id)
            .Include(s => s.Detaylar!)
                .ThenInclude(d => d.Kategori)
            .Select(s => new SablonListDto
            {
                Id = s.Id,
                Ad = s.Ad,
                Aciklama = s.Aciklama,
                DetaySayisi = s.Detaylar != null ? s.Detaylar.Count : 0
            })
            .ToListAsync(ct);
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SablonDetayDto>> Get(int id, CancellationToken ct)
    {
        var s = await _db.ProjeSablonlar
            .AsNoTracking()
            .Include(x => x.Detaylar!)
                .ThenInclude(d => d.Kategori)
            .Include(x => x.Detaylar!)
                .ThenInclude(d => d.AdetBirimi)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (s == null) return NotFound();
        return Ok(new SablonDetayDto
        {
            Id = s.Id,
            Ad = s.Ad,
            Aciklama = s.Aciklama,
            Detaylar = (s.Detaylar ?? Array.Empty<ProjeSablonDetay>())
                .OrderBy(d => d.Sira)
                .Select(d => new SablonDetayItemDto
                {
                    Id = d.Id,
                    KategoriId = d.KategoriId,
                    KategoriAd = d.Kategori != null ? d.Kategori.Ad : null,
                    AdimAdi = d.AdimAdi,
                    Aciklama = d.Aciklama,
                    Adet = d.Adet,
                    AdetBirimiId = d.AdetBirimiId,
                    AdetBirimiAd = d.AdetBirimi != null ? d.AdetBirimi.Ad : null,
                    Sira = d.Sira
                })
                .ToList()
        });
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] SablonCreateDto dto, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        var sablon = new ProjeSablon { Ad = dto.Ad, Aciklama = dto.Aciklama, OlusturanKullaniciId = userId };
        _db.ProjeSablonlar.Add(sablon);
        await _db.SaveChangesAsync(ct);
        if (dto.Detaylar != null)
        {
            var sira = 0;
            foreach (var item in dto.Detaylar)
            {
                _db.ProjeSablonDetaylar.Add(new ProjeSablonDetay
                {
                    ProjeSablonId = sablon.Id,
                    KategoriId = item.KategoriId,
                    AdimAdi = item.AdimAdi,
                    Aciklama = item.Aciklama,
                    Adet = item.Adet,
                    AdetBirimiId = item.AdetBirimiId,
                    Sira = ++sira
                });
            }
            await _db.SaveChangesAsync(ct);
        }
        return Ok(new { id = sablon.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] SablonCreateDto dto, CancellationToken ct)
    {
        var sablon = await _db.ProjeSablonlar.Include(s => s.Detaylar).FirstOrDefaultAsync(s => s.Id == id, ct);
        if (sablon == null) return NotFound();
        sablon.Ad = dto.Ad;
        sablon.Aciklama = dto.Aciklama;
        if (sablon.Detaylar != null)
            foreach (var d in sablon.Detaylar)
            { d.IsDeleted = true; d.DeleteDate = DateTime.UtcNow; }
        await _db.SaveChangesAsync(ct);
        if (dto.Detaylar != null)
        {
            var sira = 0;
            foreach (var item in dto.Detaylar)
            {
                _db.ProjeSablonDetaylar.Add(new ProjeSablonDetay
                {
                    ProjeSablonId = id,
                    KategoriId = item.KategoriId,
                    AdimAdi = item.AdimAdi,
                    Aciklama = item.Aciklama,
                    Adet = item.Adet,
                    AdetBirimiId = item.AdetBirimiId,
                    Sira = ++sira
                });
            }
            await _db.SaveChangesAsync(ct);
        }
        return Ok();
    }

    /// <summary>Şablon adımlarının sırasını günceller.</summary>
    [HttpPut("{id}/detay-sira")]
    public async Task<IActionResult> DetaySiraGuncelle(int id, [FromBody] SablonSiraGuncelleDto dto, CancellationToken ct)
    {
        var sablon = await _db.ProjeSablonlar.FindAsync(new object[] { id }, ct);
        if (sablon == null) return NotFound();
        if (dto.DetayIds == null || dto.DetayIds.Count == 0) return Ok();
        var entities = await _db.ProjeSablonDetaylar
            .Where(d => d.ProjeSablonId == id && dto.DetayIds.Contains(d.Id))
            .ToListAsync(ct);
        for (var i = 0; i < dto.DetayIds.Count; i++)
        {
            var e = entities.FirstOrDefault(x => x.Id == dto.DetayIds[i]);
            if (e != null) e.Sira = i + 1;
        }
        await _db.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var sablon = await _db.ProjeSablonlar.FindAsync(new object[] { id }, ct);
        if (sablon == null) return NotFound();
        sablon.IsDeleted = true;
        sablon.DeleteDate = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok();
    }

    /// <summary>Şablonu yeni aktif proje olarak klonla.</summary>
    [HttpPost("{id}/proje-baslat")]
    public async Task<ActionResult<object>> ProjeBaslat(int id, [FromBody] ProjeBaslatDto dto, CancellationToken ct)
    {
        var sablon = await _db.ProjeSablonlar.Include(s => s.Detaylar).FirstOrDefaultAsync(s => s.Id == id, ct);
        if (sablon == null) return NotFound();
        var userId = _currentUser.GetCurrentUserId();
        var islemBekliyorId = await _db.Durumlar.Where(x => x.Ad == "İşlem Bekliyor").Select(x => x.Id).FirstOrDefaultAsync(ct);
        if (islemBekliyorId == 0) islemBekliyorId = 1;

        var proje = new Proje
        {
            Ad = dto.Ad ?? sablon.Ad,
            ProjeDurumId = islemBekliyorId,
            Aciklama = sablon.Aciklama,
            BaslangicTarihi = dto.BaslangicTarihi,
            PlanlananBitisTarihi = dto.PlanlananBitisTarihi,
            SorumluKullaniciId = dto.SorumluKullaniciId,
            AktifMi = true,
            YetkiTipi = dto.YetkiTipi,
            OlusturanKullaniciId = userId
        };
        _db.Projeler.Add(proje);
        await _db.SaveChangesAsync(ct);

        if (dto.YetkiKullaniciIds != null && dto.YetkiTipi == 1)
        {
            foreach (var kid in dto.YetkiKullaniciIds)
            {
                _db.ProjeYetkiKullanicilar.Add(new ProjeYetkiKullanici { ProjeId = proje.Id, KullaniciId = kid });
            }
            await _db.SaveChangesAsync(ct);
        }

        if (sablon.Detaylar != null)
        {
            foreach (var d in sablon.Detaylar.OrderBy(x => x.Sira))
            {
                _db.ProjeDetaylar.Add(new ProjeDetay
                {
                    ProjeId = proje.Id,
                    KategoriId = d.KategoriId,
                    AdimAdi = d.AdimAdi,
                    DurumId = islemBekliyorId,
                    Aciklama = d.Aciklama,
                    Adet = d.Adet,
                    AdetBirimiId = d.AdetBirimiId,
                    Sira = d.Sira
                });
            }
            await _db.SaveChangesAsync(ct);
        }

        return Ok(new { id = proje.Id });
    }
}

public class SablonListDto { public int Id { get; set; } public string Ad { get; set; } = ""; public string? Aciklama { get; set; } public int DetaySayisi { get; set; } }
public class SablonDetayDto { public int Id { get; set; } public string Ad { get; set; } = ""; public string? Aciklama { get; set; } public List<SablonDetayItemDto> Detaylar { get; set; } = new(); }
public class SablonDetayItemDto
{
    public int Id { get; set; }
    public int? KategoriId { get; set; }
    public string? KategoriAd { get; set; }
    public string AdimAdi { get; set; } = "";
    public string? Aciklama { get; set; }
    public decimal? Adet { get; set; }
    public int? AdetBirimiId { get; set; }
    public string? AdetBirimiAd { get; set; }
    public int Sira { get; set; }
}
public class SablonCreateDto { public string Ad { get; set; } = ""; public string? Aciklama { get; set; } public List<SablonDetayItemCreateDto>? Detaylar { get; set; } }
public class SablonDetayItemCreateDto
{
    public int? KategoriId { get; set; }
    public string AdimAdi { get; set; } = "";
    public string? Aciklama { get; set; }
    public decimal? Adet { get; set; }
    public int? AdetBirimiId { get; set; }
}
public class ProjeBaslatDto { public string? Ad { get; set; } public DateTime? BaslangicTarihi { get; set; } public DateTime? PlanlananBitisTarihi { get; set; } public int? SorumluKullaniciId { get; set; } public int YetkiTipi { get; set; } public List<int>? YetkiKullaniciIds { get; set; } }
public class SablonSiraGuncelleDto { public List<int> DetayIds { get; set; } = new(); }
