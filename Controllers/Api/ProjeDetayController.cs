using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjeGorevYonetimi.Data;
using ProjeGorevYonetimi.Models.Entities;
using ProjeGorevYonetimi.Services;

namespace ProjeGorevYonetimi.Controllers.Api;

[Route("api/projeler/{projeId:int}/[controller]")]
[ApiController]
[ServiceFilter(typeof(Filters.ApiKeyAuthFilter))]
public class ProjeDetayController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IYetkiService _yetki;
    private readonly ICurrentUserService _currentUser;

    public ProjeDetayController(ApplicationDbContext db, IYetkiService yetki, ICurrentUserService currentUser)
    {
        _db = db;
        _yetki = yetki;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProjeDetayItemDto>>> List(int projeId, CancellationToken ct)
    {
        if (!await _yetki.ProjeGorebilirMiAsync(projeId, ct)) return Forbid();
        var list = await _db.ProjeDetaylar
            .Where(d => d.ProjeId == projeId)
            .OrderBy(d => d.Sira)
            .Include(d => d.Durum)
            .Include(d => d.Kategori)
            .Include(d => d.SorumluKullanici)
            .Include(d => d.Yorumlar)
            .Select(d => new ProjeDetayItemDto
            {
                Id = d.Id,
                KategoriId = d.KategoriId,
                KategoriAd = d.Kategori != null ? d.Kategori.Ad : null,
                AdimAdi = d.AdimAdi,
                DurumId = d.DurumId,
                DurumAd = d.Durum.Ad,
                SorumluKullaniciId = d.SorumluKullaniciId,
                SorumluAdSoyad = d.SorumluKullanici != null ? d.SorumluKullanici.AdSoyad : null,
                Aciklama = d.Aciklama,
                Sira = d.Sira,
                YorumSayisi = d.Yorumlar!.Count
            })
            .ToListAsync(ct);
        return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create(int projeId, [FromBody] ProjeDetayCreateDto dto, CancellationToken ct)
    {
        if (!await _yetki.ProjeGorebilirMiAsync(projeId, ct)) return Forbid();
        var beklemedeId = await _db.Durumlar.Where(x => x.Ad == "Beklemede").Select(x => x.Id).FirstOrDefaultAsync(ct);
        if (beklemedeId == 0) beklemedeId = 2;
        var maxSira = await _db.ProjeDetaylar.Where(d => d.ProjeId == projeId).Select(d => (int?)d.Sira).MaxAsync(ct) ?? 0;
        var entity = new ProjeDetay
        {
            ProjeId = projeId,
            KategoriId = dto.KategoriId,
            AdimAdi = dto.AdimAdi,
            DurumId = beklemedeId,
            SorumluKullaniciId = dto.SorumluKullaniciId,
            Aciklama = dto.Aciklama,
            Sira = maxSira + 1
        };
        _db.ProjeDetaylar.Add(entity);
        await _db.SaveChangesAsync(ct);

        var proje = await _db.Projeler.FindAsync(new object[] { projeId }, ct);
        if (proje != null)
        {
            var islemeAlindiId = await _db.Durumlar.Where(x => x.Ad == "İşleme Alındı").Select(x => x.Id).FirstOrDefaultAsync(ct);
            if (islemeAlindiId != 0) proje.ProjeDurumId = islemeAlindiId;
            await _db.SaveChangesAsync(ct);
        }

        return Ok(new { id = entity.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int projeId, int id, [FromBody] ProjeDetayUpdateDto dto, CancellationToken ct)
    {
        if (!await _yetki.ProjeGorebilirMiAsync(projeId, ct)) return Forbid();
        var entity = await _db.ProjeDetaylar.FirstOrDefaultAsync(d => d.Id == id && d.ProjeId == projeId, ct);
        if (entity == null) return NotFound();
        entity.KategoriId = dto.KategoriId;
        entity.AdimAdi = dto.AdimAdi;
        entity.DurumId = dto.DurumId;
        entity.SorumluKullaniciId = dto.SorumluKullaniciId;
        entity.Aciklama = dto.Aciklama;
        entity.Sira = dto.Sira;

        await _db.SaveChangesAsync(ct);

        var detaylar = await _db.ProjeDetaylar.Where(d => d.ProjeId == projeId).ToListAsync(ct);
        var tamamlandiId = await _db.Durumlar.Where(x => x.Ad == "Tamamlandı").Select(x => x.Id).FirstOrDefaultAsync(ct);
        if (tamamlandiId != 0 && detaylar.Count > 0 && detaylar.All(d => d.DurumId == 5 || d.DurumId == 4))
        {
            var proje = await _db.Projeler.FindAsync(new object[] { projeId }, ct);
            if (proje != null) { proje.ProjeDurumId = tamamlandiId; await _db.SaveChangesAsync(ct); }
        }
        return Ok();
    }

    /// <summary>Seçilen adımların durumunu toplu günceller.</summary>
    [HttpPut("durum-guncelle")]
    public async Task<IActionResult> TopluDurumGuncelle(int projeId, [FromBody] TopluDurumGuncelleDto dto, CancellationToken ct)
    {
        if (!await _yetki.ProjeGorebilirMiAsync(projeId, ct)) return Forbid();
        if (dto.DetayIds == null || dto.DetayIds.Count == 0) return BadRequest("En az bir adım seçin.");
        var entities = await _db.ProjeDetaylar
            .Where(d => d.ProjeId == projeId && dto.DetayIds.Contains(d.Id))
            .ToListAsync(ct);
        foreach (var e in entities)
            e.DurumId = dto.DurumId;
        await _db.SaveChangesAsync(ct);

        var detaylar = await _db.ProjeDetaylar.Where(d => d.ProjeId == projeId).ToListAsync(ct);
        var tamamlandiId = await _db.Durumlar.Where(x => x.Ad == "Tamamlandı").Select(x => x.Id).FirstOrDefaultAsync(ct);
        if (tamamlandiId != 0 && detaylar.Count > 0 && detaylar.All(d => d.DurumId == 5 || d.DurumId == 4))
        {
            var proje = await _db.Projeler.FindAsync(new object[] { projeId }, ct);
            if (proje != null) { proje.ProjeDurumId = tamamlandiId; await _db.SaveChangesAsync(ct); }
        }
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int projeId, int id, CancellationToken ct)
    {
        if (!await _yetki.ProjeGorebilirMiAsync(projeId, ct)) return Forbid();
        var entity = await _db.ProjeDetaylar.FirstOrDefaultAsync(d => d.Id == id && d.ProjeId == projeId, ct);
        if (entity == null) return NotFound();
        entity.IsDeleted = true;
        entity.DeleteDate = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok();
    }
}

public class ProjeDetayCreateDto
{
    public int? KategoriId { get; set; }
    public string AdimAdi { get; set; } = "";
    public int? SorumluKullaniciId { get; set; }
    public string? Aciklama { get; set; }
}

public class ProjeDetayUpdateDto
{
    public int? KategoriId { get; set; }
    public string AdimAdi { get; set; } = "";
    public int DurumId { get; set; }
    public int? SorumluKullaniciId { get; set; }
    public string? Aciklama { get; set; }
    public int Sira { get; set; }
}

public class TopluDurumGuncelleDto
{
    public List<int> DetayIds { get; set; } = new();
    public int DurumId { get; set; }
}
