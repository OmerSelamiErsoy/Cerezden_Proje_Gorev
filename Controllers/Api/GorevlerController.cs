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
    private readonly IWebHostEnvironment _env;

    public GorevlerController(ApplicationDbContext db, ICurrentUserService currentUser, IWebHostEnvironment env)
    {
        _db = db;
        _currentUser = currentUser;
        _env = env;
    }

    /// <summary>Görevlerim: bana atanan veya benim oluşturduğum görevler. grupId verilirse sadece o gruptaki görevler. Kanban: son 50 tamamlanan/iptal hariç hepsi.</summary>
    [HttpGet("kanban")]
    public async Task<ActionResult<KanbanViewModel>> Kanban([FromQuery] int? grupId, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        var durumlar = await _db.Durumlar.OrderBy(d => d.Sira).Select(d => new { d.Id, d.Ad }).ToListAsync(ct);

        var gorevIds = await _db.Gorevler.Where(g => g.OlusturanKullaniciId == userId).Select(g => g.Id).ToListAsync(ct);
        var atananIds = await _db.GorevAtamalar.Where(a => a.KullaniciId == userId).Select(a => a.GorevId).ToListAsync(ct);
        var tumIds = gorevIds.Union(atananIds).Distinct().ToList();

        var aktifGorevler = await _db.Gorevler
            .Where(g => tumIds.Contains(g.Id) && g.DurumId != 4 && g.DurumId != 5 && (grupId == null || g.GorevGrubuId == grupId))
            .Include(g => g.Durum)
            .Include(g => g.OlusturanKullanici)
            .Include(g => g.Atamalar!).ThenInclude(a => a.Kullanici)
            .Include(g => g.Yorumlar)
            .OrderBy(g => g.OlusturmaTarihi)
            .ToListAsync(ct);

        var tamamlananIptal = await _db.Gorevler
            .Where(g => tumIds.Contains(g.Id) && (g.DurumId == 4 || g.DurumId == 5) && (grupId == null || g.GorevGrubuId == grupId))
            .OrderByDescending(g => g.UpdateDate ?? g.OlusturmaTarihi)
            .Take(50)
            .Include(g => g.Durum)
            .Include(g => g.OlusturanKullanici)
            .Include(g => g.Atamalar!).ThenInclude(a => a.Kullanici)
            .Include(g => g.Yorumlar)
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
            OlusturanAdSoyad = g.OlusturanKullanici?.AdSoyad,
            Atananlar = g.Atamalar?.Select(a => a.Kullanici?.AdSoyad).ToList(),
            YorumSayisi = g.Yorumlar?.Count ?? 0
        };

        var columns = durumlar.Select(d => new { d.Id, d.Ad }).ToList();
        var aktifByDurum = aktifGorevler.GroupBy(g => g.DurumId).ToDictionary(g => g.Key, g => g.Select(Map).ToList());
        var tamamlananIptalList = tamamlananIptal.Select(Map).ToList();

        return Ok(new KanbanViewModel
        {
            Durumlar = columns.Select(c => new KanbanKolonDto { Id = c.Id, Ad = c.Ad }).ToList(),
            Kartlar = durumlar.Select(d => new { d.Id, Kartlar = aktifByDurum.GetValueOrDefault(d.Id, new List<object>()) }).ToDictionary(x => x.Id, x => x.Kartlar),
            TamamlananIptalSon50 = tamamlananIptalList
        });
    }

    [HttpGet]
    public async Task<ActionResult<List<GorevDto>>> List(CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        var gorevIds = await _db.Gorevler.Where(g => g.OlusturanKullaniciId == userId).Select(g => g.Id).ToListAsync(ct);
        var atananIds = await _db.GorevAtamalar.Where(a => a.KullaniciId == userId).Select(a => a.GorevId).ToListAsync(ct);
        var tumIds = gorevIds.Union(atananIds).Distinct().ToList();
        var list = await _db.Gorevler
            .Where(g => tumIds.Contains(g.Id))
            .OrderByDescending(g => g.OlusturmaTarihi)
            .Include(g => g.Durum)
            .Include(g => g.OlusturanKullanici)
            .Include(g => g.Atamalar!).ThenInclude(a => a.Kullanici)
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

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] GorevCreateDto dto, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        var islemBekliyorId = await _db.Durumlar.Where(x => x.Ad == "İşlem Bekliyor").Select(x => x.Id).FirstOrDefaultAsync(ct);
        if (islemBekliyorId == 0) islemBekliyorId = 1;

        var gorev = new Gorev
        {
            GorevGrubuId = dto.GorevGrubuId,
            Ad = dto.Ad,
            Aciklama = dto.Aciklama,
            Turu = dto.Turu,
            OlusturanKullaniciId = userId,
            OlusturmaTarihi = DateTime.UtcNow,
            DurumId = islemBekliyorId
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
        var gorev = await _db.Gorevler.FindAsync(new object[] { id }, ct);
        if (gorev == null) return NotFound();
        var eskiDurumId = gorev.DurumId;
        gorev.Ad = dto.Ad;
        gorev.Aciklama = dto.Aciklama;
        gorev.DurumId = dto.DurumId;
        gorev.Renk = dto.Renk;

        await _db.SaveChangesAsync(ct);

        if (eskiDurumId != dto.DurumId)
        {
            try
            {
                var userId = _currentUser.GetCurrentUserId();
                _db.GorevDurumLoglar.Add(new GorevDurumLog
                {
                    GorevId = id,
                    EskiDurumId = eskiDurumId,
                    YeniDurumId = dto.DurumId,
                    DegistirenKullaniciId = userId,
                    DegistirmeTarihi = DateTime.UtcNow
                });
                await _db.SaveChangesAsync(ct);
            }
            catch
            {
                // Log tablosu yoksa veya log yazılamazsa görev güncellemesi yine başarılı sayılır
            }
        }

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
        var list = await _db.GorevYorumlar
            .Where(y => y.GorevId == gorevId)
            .OrderByDescending(y => y.InsertDate)
            .Include(y => y.Dosyalar)
            .Select(y => new GorevYorumDto
            {
                Id = y.Id,
                YorumMetni = y.YorumMetni,
                InsertDate = y.InsertDate,
                Dosyalar = y.Dosyalar!.Select(f => new GorevDosyaDto { Id = f.Id, DosyaAdi = f.DosyaAdi, DosyaYolu = f.DosyaYolu }).ToList()
            })
            .ToListAsync(ct);
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
    public List<int>? AtananKullaniciIds { get; set; }
}
public class GorevUpdateDto
{
    public string Ad { get; set; } = "";
    public string? Aciklama { get; set; }
    public int DurumId { get; set; }
    public string? Renk { get; set; }
}
public class GorevDurumLogDto
{
    public int Id { get; set; }
    public string? EskiDurumAd { get; set; }
    public string YeniDurumAd { get; set; } = "";
    public string DegistirenAdSoyad { get; set; } = "";
    public DateTime DegistirmeTarihi { get; set; }
}
public class GorevYorumDto
{
    public int Id { get; set; }
    public string YorumMetni { get; set; } = "";
    public DateTime? InsertDate { get; set; }
    public List<GorevDosyaDto> Dosyalar { get; set; } = new();
}

public class GorevDosyaDto { public int Id { get; set; } public string DosyaAdi { get; set; } = ""; public string DosyaYolu { get; set; } = ""; }
