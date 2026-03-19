using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjeGorevYonetimi.Data;
using ProjeGorevYonetimi.Models.Entities;
using ProjeGorevYonetimi.Services;

namespace ProjeGorevYonetimi.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[ServiceFilter(typeof(Filters.ApiKeyAuthFilter))]
public class GorevlerController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IYetkiService _yetki;
    private readonly IWebHostEnvironment _env;

    public GorevlerController(ApplicationDbContext db, ICurrentUserService currentUser, IYetkiService yetki, IWebHostEnvironment env)
    {
        _db = db;
        _currentUser = currentUser;
        _yetki = yetki;
        _env = env;
    }

    /// <summary>Görevlerim: bana atanan veya benim oluşturduğum görevler. grupId verilirse sadece o gruptaki görevler. Kanban: son 50 tamamlanan/iptal hariç hepsi.</summary>
    [HttpGet("kanban")]
    public async Task<ActionResult<KanbanViewModel>> Kanban([FromQuery] int? grupId, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        var durumlar = await _db.Durumlar.AsNoTracking().OrderBy(d => d.Sira).Select(d => new { d.Id, d.Ad }).ToListAsync(ct);

        var gorevIds = await _db.Gorevler.AsNoTracking().Where(g => g.OlusturanKullaniciId == userId).Select(g => g.Id).ToListAsync(ct);
        var atananIds = await _db.GorevAtamalar.AsNoTracking().Where(a => a.KullaniciId == userId).Select(a => a.GorevId).ToListAsync(ct);
        var sorumluIds = await _db.Gorevler.AsNoTracking().Where(g => g.SorumluKullaniciId == userId).Select(g => g.Id).ToListAsync(ct);
        var tumIds = gorevIds.Union(atananIds).Union(sorumluIds).Distinct().ToList();

        var aktifGorevler = await _db.Gorevler
            .AsNoTracking()
            .Where(g => tumIds.Contains(g.Id) && g.DurumId != 4 && g.DurumId != 5 && (grupId == null || g.GorevGrubuId == grupId))
            .Include(g => g.Durum)
            .Include(g => g.OlusturanKullanici)
            .Include(g => g.SorumluKullanici)
            .Include(g => g.Atamalar!).ThenInclude(a => a.Kullanici)
            .Include(g => g.Yorumlar)
            .OrderBy(g => g.OlusturmaTarihi)
            .AsSplitQuery()
            .ToListAsync(ct);

        var tamamlananIptal = await _db.Gorevler
            .AsNoTracking()
            .Where(g => tumIds.Contains(g.Id) && (g.DurumId == 4 || g.DurumId == 5) && (grupId == null || g.GorevGrubuId == grupId))
            .OrderByDescending(g => g.UpdateDate ?? g.OlusturmaTarihi)
            .Take(50)
            .Include(g => g.Durum)
            .Include(g => g.OlusturanKullanici)
            .Include(g => g.SorumluKullanici)
            .Include(g => g.Atamalar!).ThenInclude(a => a.Kullanici)
            .Include(g => g.Yorumlar)
            .AsSplitQuery()
            .ToListAsync(ct);

        object Map(Gorev g) => new
        {
            g.Id,
            g.Ad,
            g.Aciklama,
            g.Turu,
            g.DurumId,
            DurumAd = g.Durum?.Ad,
            g.Renk,
            g.OlusturmaTarihi,
            g.OlusturanKullaniciId,
            OlusturanAdSoyad = g.OlusturanKullanici?.AdSoyad,
            g.SorumluKullaniciId,
            SorumluAdSoyad = g.SorumluKullanici?.AdSoyad,
            Atananlar = g.Atamalar?.Select(a => a.Kullanici?.AdSoyad).ToList(),
            AtananKullaniciIds = g.Atamalar?.Select(a => a.KullaniciId).ToList(),
            BanaAtanmis = g.Atamalar?.Any(a => a.KullaniciId == userId) ?? false,
            YorumSayisi = g.Yorumlar?.Count ?? 0
        };

        var columns = durumlar.Select(d => new { d.Id, d.Ad }).ToList();
        var aktifByDurum = aktifGorevler.GroupBy(g => g.DurumId).ToDictionary(g => g.Key, g => g.Select(Map).ToList());
        var tamamlananIptalList = tamamlananIptal.Select(Map).ToList();

        return Ok(new KanbanViewModel
        {
            Durumlar = columns.Select(c => new KanbanKolonDto { Id = c.Id, Ad = c.Ad }).ToList(),
            Kartlar = durumlar.Select(d => new { d.Id, Kartlar = aktifByDurum.GetValueOrDefault(d.Id, new List<object>()) }).ToDictionary(x => x.Id, x => x.Kartlar),
            TamamlananIptalSon50 = tamamlananIptalList,
            Me = new KanbanMeDto { KullaniciId = userId, GenelYetkiliMi = _yetki.GenelYetkiliMi() }
        });
    }

    [HttpGet]
    public async Task<ActionResult<List<GorevDto>>> List(CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        var gorevIds = await _db.Gorevler.AsNoTracking().Where(g => g.OlusturanKullaniciId == userId).Select(g => g.Id).ToListAsync(ct);
        var atananIds = await _db.GorevAtamalar.AsNoTracking().Where(a => a.KullaniciId == userId).Select(a => a.GorevId).ToListAsync(ct);
        var tumIds = gorevIds.Union(atananIds).Distinct().ToList();
        var list = await _db.Gorevler
            .AsNoTracking()
            .Where(g => tumIds.Contains(g.Id))
            .OrderByDescending(g => g.OlusturmaTarihi)
            .Include(g => g.Durum)
            .Include(g => g.OlusturanKullanici)
            .Include(g => g.Atamalar!).ThenInclude(a => a.Kullanici)
            .AsSplitQuery()
            .Select(g => new GorevDto
            {
                Id = g.Id,
                Ad = g.Ad,
                Aciklama = g.Aciklama,
                Turu = g.Turu,
                DurumId = g.DurumId,
                DurumAd = g.Durum.Ad,
                Renk = g.Renk,
                OlusturmaTarihi = g.OlusturmaTarihi,
                OlusturanAdSoyad = g.OlusturanKullanici != null ? g.OlusturanKullanici.AdSoyad : null,
                Atananlar = g.Atamalar!.Select(a => a.Kullanici!.AdSoyad).ToList()
            })
            .ToListAsync(ct);
        return Ok(list);
    }

    /// <summary>
    /// Özet: İşlem Bekliyor, Beklemede, İşleme Alındı durumlarındaki görev adetleri (sadece benim oluşturduğum veya bana atananlar).
    /// </summary>
    [HttpGet("durum-ozet")]
    public async Task<ActionResult<List<GorevDurumOzetDto>>> DurumOzet([FromQuery] bool sorumluOldugum = false, CancellationToken ct = default)
    {
        var userId = _currentUser.GetCurrentUserId();

        var hedefDurumAdlari = new[] { "İşlem Bekliyor", "Beklemede", "İşleme Alındı" };
        var durumlar = await _db.Durumlar
            .AsNoTracking()
            .Where(d => hedefDurumAdlari.Contains(d.Ad))
            .Select(d => new { d.Id, d.Ad })
            .ToListAsync(ct);
        var durumIdler = durumlar.Select(d => d.Id).ToList();
        if (!durumIdler.Any()) return Ok(new List<GorevDurumOzetDto>());

        if (sorumluOldugum)
        {
            // Kanban ile aynı görünürlük: sadece benim oluşturduğum veya bana atanan görevlerden sorumlu olduklarım
            var gorevIdsSorumlu = await _db.Gorevler.AsNoTracking().Where(g => !g.IsDeleted && g.OlusturanKullaniciId == userId).Select(g => g.Id).ToListAsync(ct);
            var atananIdsSorumlu = await _db.GorevAtamalar.AsNoTracking().Where(a => a.KullaniciId == userId).Select(a => a.GorevId).ToListAsync(ct);
            var tumGorevIdsSorumlu = gorevIdsSorumlu.Union(atananIdsSorumlu).Distinct().ToList();
            if (!tumGorevIdsSorumlu.Any())
            {
                return Ok(durumlar.Select(d => new GorevDurumOzetDto { DurumAd = d.Ad, Adet = 0 }).OrderBy(x => Array.IndexOf(hedefDurumAdlari, x.DurumAd)).ToList());
            }
            var aktifGrupIdler = await _db.GorevGruplar.AsNoTracking().Select(x => x.Id).ToListAsync(ct);
            var sayilar = await _db.Gorevler
                .AsNoTracking()
                .Where(g => !g.IsDeleted && g.SorumluKullaniciId == userId && tumGorevIdsSorumlu.Contains(g.Id)
                    && durumIdler.Contains(g.DurumId)
                    && (g.GorevGrubuId == null || aktifGrupIdler.Contains(g.GorevGrubuId.Value)))
                .GroupBy(g => g.DurumId)
                .Select(g => new { DurumId = g.Key, Sayi = g.Count() })
                .ToListAsync(ct);
            var sonuc = durumlar
                .Select(d => new GorevDurumOzetDto
                {
                    DurumAd = d.Ad,
                    Adet = sayilar.FirstOrDefault(x => x.DurumId == d.Id)?.Sayi ?? 0
                })
                .OrderBy(x => Array.IndexOf(hedefDurumAdlari, x.DurumAd))
                .ToList();
            return Ok(sonuc);
        }

        var aktifGrupIdlerToplam = await _db.GorevGruplar.AsNoTracking().Select(x => x.Id).ToListAsync(ct);
        var gorevIds = await _db.Gorevler
            .AsNoTracking()
            .Where(g => !g.IsDeleted && g.OlusturanKullaniciId == userId && (g.GorevGrubuId == null || aktifGrupIdlerToplam.Contains(g.GorevGrubuId!.Value)))
            .Select(g => g.Id).ToListAsync(ct);
        var atananIds = await _db.GorevAtamalar.AsNoTracking().Where(a => a.KullaniciId == userId).Select(a => a.GorevId).ToListAsync(ct);
        var tumIds = gorevIds.Union(atananIds).Distinct().ToList();
        if (!tumIds.Any()) return Ok(new List<GorevDurumOzetDto>());

        var toplamSayilar = await _db.Gorevler
            .AsNoTracking()
            .Where(g => !g.IsDeleted && tumIds.Contains(g.Id) && durumIdler.Contains(g.DurumId)
                && (g.GorevGrubuId == null || aktifGrupIdlerToplam.Contains(g.GorevGrubuId!.Value)))
            .GroupBy(g => g.DurumId)
            .Select(g => new { DurumId = g.Key, Sayi = g.Count() })
            .ToListAsync(ct);

        var toplamSonuc = durumlar
            .Select(d => new GorevDurumOzetDto
            {
                DurumAd = d.Ad,
                Adet = toplamSayilar.FirstOrDefault(x => x.DurumId == d.Id)?.Sayi ?? 0
            })
            .OrderBy(x => Array.IndexOf(hedefDurumAdlari, x.DurumAd))
            .ToList();

        return Ok(toplamSonuc);
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] GorevCreateDto dto, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        if (dto.Turu == 1 && (!dto.SorumluKullaniciId.HasValue || dto.SorumluKullaniciId.Value == 0))
            return BadRequest(new { error = "Tür 'Kişilere' seçiliyse Sorumlu Kişi zorunludur." });

        var islemBekliyorId = await _db.Durumlar.Where(x => x.Ad == "İşlem Bekliyor").Select(x => x.Id).FirstOrDefaultAsync(ct);
        if (islemBekliyorId == 0) islemBekliyorId = 1;

        var sorumluId = dto.Turu == 0 ? userId : (dto.SorumluKullaniciId ?? userId);
        var gorev = new Gorev
        {
            GorevGrubuId = dto.GorevGrubuId,
            Ad = dto.Ad,
            Aciklama = dto.Aciklama,
            Turu = dto.Turu,
            OlusturanKullaniciId = userId,
            OlusturmaTarihi = DateTime.UtcNow,
            DurumId = islemBekliyorId,
            SorumluKullaniciId = sorumluId
        };
        _db.Gorevler.Add(gorev);
        await _db.SaveChangesAsync(ct);

        if (dto.AtananKullaniciIds != null)
            foreach (var kid in dto.AtananKullaniciIds)
            {
                _db.GorevAtamalar.Add(new GorevAtama { GorevId = gorev.Id, KullaniciId = kid });
            }
        await _db.SaveChangesAsync(ct);
        return Ok(new { id = gorev.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] GorevUpdateDto dto, CancellationToken ct)
    {
        var gorev = await _db.Gorevler.Include(g => g.Atamalar).FirstOrDefaultAsync(g => g.Id == id, ct);
        if (gorev == null) return NotFound();
        var eskiDurumId = gorev.DurumId;
        gorev.Ad = dto.Ad;
        gorev.Aciklama = dto.Aciklama;
        gorev.DurumId = dto.DurumId;
        gorev.Renk = dto.Renk;
        gorev.Turu = dto.Turu;
        var userId = _currentUser.GetCurrentUserId();
        gorev.SorumluKullaniciId = dto.Turu == 0 ? userId : (dto.SorumluKullaniciId ?? userId);

        // Tür / atamalar: mevcut atamaları kaldır, yenilerini ekle
        if (gorev.Atamalar != null)
        {
            foreach (var a in gorev.Atamalar.ToList())
                _db.GorevAtamalar.Remove(a);
        }
        if (dto.Turu == 1 && dto.AtananKullaniciIds != null)
        {
            foreach (var kid in dto.AtananKullaniciIds)
                _db.GorevAtamalar.Add(new GorevAtama { GorevId = id, KullaniciId = kid });
        }

        // Durum değiştiyse log kaydını aynı transaction içinde ekle (tek SaveChanges ile yazılsın)
        if (eskiDurumId != dto.DurumId)
        {
            _db.GorevDurumLoglar.Add(new GorevDurumLog
            {
                GorevId = id,
                EskiDurumId = eskiDurumId,
                YeniDurumId = dto.DurumId,
                DegistirenKullaniciId = userId,
                DegistirmeTarihi = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var gorev = await _db.Gorevler.FindAsync(new object[] { id }, ct);
        if (gorev == null) return NotFound();
        gorev.IsDeleted = true;
        gorev.DeleteDate = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok();
    }

    /// <summary>Görev durum değişiklik geçmişi (kim, ne zaman, hangi statüye).</summary>
    [HttpGet("{gorevId:int}/durum-log")]
    public async Task<ActionResult<List<GorevDurumLogDto>>> DurumLog(int gorevId, CancellationToken ct)
    {
        try
        {
            var list = await _db.GorevDurumLoglar
                .AsNoTracking()
                .Where(l => l.GorevId == gorevId)
                .OrderByDescending(l => l.DegistirmeTarihi)
                .Include(l => l.DegistirenKullanici)
                .Include(l => l.EskiDurum)
                .Include(l => l.YeniDurum)
                .Select(l => new GorevDurumLogDto
                {
                    Id = l.Id,
                    EskiDurumAd = l.EskiDurum != null ? l.EskiDurum.Ad : null,
                    YeniDurumAd = l.YeniDurum!.Ad,
                    DegistirenAdSoyad = l.DegistirenKullanici.AdSoyad,
                    DegistirenCepTel = l.DegistirenKullanici.CepTel,
                    DegistirmeTarihi = l.DegistirmeTarihi
                })
                .ToListAsync(ct);
            return Ok(list);
        }
        catch
        {
            return Ok(new List<GorevDurumLogDto>());
        }
    }

    [HttpGet("{gorevId:int}/yorumlar")]
    public async Task<ActionResult<List<GorevYorumDto>>> Yorumlar(int gorevId, CancellationToken ct)
    {
        // Yorumları yazan kullanıcı bilgisiyle birlikte getir
        var yorumlar = await (from y in _db.GorevYorumlar.AsNoTracking()
                          where y.GorevId == gorevId
                          orderby y.InsertDate descending
                          join k in _db.Kullanicilar.AsNoTracking().IgnoreQueryFilters() on y.InsertedByUserId equals k.Id into gj
                          from k in gj.DefaultIfEmpty()
                          select new { y.Id, y.YorumMetni, y.InsertDate, YazanAdSoyad = k != null ? k.AdSoyad : null, YazanCepTel = k != null ? k.CepTel : null }
                          ).ToListAsync(ct);
        var yorumIds = yorumlar.Select(x => x.Id).ToList();
        var dosyalarList = await _db.GorevYorumDosyalar.AsNoTracking()
            .Where(f => yorumIds.Contains(f.GorevYorumId))
            .Select(f => new { f.GorevYorumId, f.Id, f.DosyaAdi, f.DosyaYolu })
            .ToListAsync(ct);
        var dosyaDict = dosyalarList.GroupBy(x => x.GorevYorumId).ToDictionary(g => g.Key, g => g.Select(f => new GorevDosyaDto { Id = f.Id, DosyaAdi = f.DosyaAdi, DosyaYolu = f.DosyaYolu }).ToList());
        var list = yorumlar.Select(y => new GorevYorumDto
        {
            Id = y.Id,
            YorumMetni = y.YorumMetni,
            InsertDate = y.InsertDate,
            YazanAdSoyad = y.YazanAdSoyad,
            YazanCepTel = y.YazanCepTel,
            Dosyalar = dosyaDict.GetValueOrDefault(y.Id, new List<GorevDosyaDto>())
        }).ToList();
        return Ok(list);
    }

    [HttpPost("{gorevId:int}/yorumlar")]
    public async Task<ActionResult<object>> YorumEkle(int gorevId, [FromBody] YorumCreateDto dto, CancellationToken ct)
    {
        var gorev = await _db.Gorevler.AnyAsync(g => g.Id == gorevId, ct);
        if (!gorev) return NotFound();
        var entity = new GorevYorum { GorevId = gorevId, YorumMetni = dto.YorumMetni };
        _db.GorevYorumlar.Add(entity);
        await _db.SaveChangesAsync(ct);
        return Ok(new { id = entity.Id });
    }

    [HttpPost("{gorevId:int}/yorumlar/{yorumId:int}/upload")]
    public async Task<ActionResult<object>> YorumDosya(int gorevId, int yorumId, IFormFile file, CancellationToken ct)
    {
        var yorum = await _db.GorevYorumlar.FirstOrDefaultAsync(y => y.Id == yorumId && y.GorevId == gorevId, ct);
        if (yorum == null) return NotFound();
        var uploads = Path.Combine(_env.WebRootPath, "uploads", "gorev");
        Directory.CreateDirectory(uploads);
        var safeName = $"{Guid.NewGuid()}_{file.FileName}";
        var path = Path.Combine(uploads, safeName);
        await using (var stream = System.IO.File.Create(path))
            await file.CopyToAsync(stream, ct);
        var relPath = $"/uploads/gorev/{safeName}";
        _db.GorevYorumDosyalar.Add(new GorevYorumDosya { GorevYorumId = yorumId, DosyaAdi = file.FileName, DosyaYolu = relPath });
        await _db.SaveChangesAsync(ct);
        return Ok(new { path = relPath, name = file.FileName });
    }
}

public class KanbanViewModel
{
    public List<KanbanKolonDto> Durumlar { get; set; } = new();
    public Dictionary<int, List<object>> Kartlar { get; set; } = new();
    public List<object> TamamlananIptalSon50 { get; set; } = new();
    public KanbanMeDto? Me { get; set; }
}
public class KanbanMeDto
{
    public int KullaniciId { get; set; }
    public bool GenelYetkiliMi { get; set; }
}
public class KanbanKolonDto { public int Id { get; set; } public string Ad { get; set; } = ""; }
public class GorevDto
{
    public int Id { get; set; }
    public string Ad { get; set; } = "";
    public string? Aciklama { get; set; }
    public int Turu { get; set; }
    public int DurumId { get; set; }
    public string DurumAd { get; set; } = "";
    public string? Renk { get; set; }
    public DateTime OlusturmaTarihi { get; set; }
    public string? OlusturanAdSoyad { get; set; }
    public List<string> Atananlar { get; set; } = new();
}

public class GorevCreateDto
{
    public int? GorevGrubuId { get; set; }
    public string Ad { get; set; } = "";
    public string? Aciklama { get; set; }
    public int Turu { get; set; }
    public int? SorumluKullaniciId { get; set; }
    public List<int>? AtananKullaniciIds { get; set; }
}
public class GorevUpdateDto
{
    public string Ad { get; set; } = "";
    public string? Aciklama { get; set; }
    public int DurumId { get; set; }
    public int Turu { get; set; }
    public int? SorumluKullaniciId { get; set; }
    public List<int>? AtananKullaniciIds { get; set; }
    public string? Renk { get; set; }
}
public class GorevDurumLogDto
{
    public int Id { get; set; }
    public string? EskiDurumAd { get; set; }
    public string YeniDurumAd { get; set; } = "";
    public string DegistirenAdSoyad { get; set; } = "";
    public string? DegistirenCepTel { get; set; }
    public DateTime DegistirmeTarihi { get; set; }
}
public class GorevYorumDto
{
    public int Id { get; set; }
    public string YorumMetni { get; set; } = "";
    public DateTime? InsertDate { get; set; }
    public string? YazanAdSoyad { get; set; }
    public string? YazanCepTel { get; set; }
    public List<GorevDosyaDto> Dosyalar { get; set; } = new();
}

public class GorevDosyaDto { public int Id { get; set; } public string DosyaAdi { get; set; } = ""; public string DosyaYolu { get; set; } = ""; }
