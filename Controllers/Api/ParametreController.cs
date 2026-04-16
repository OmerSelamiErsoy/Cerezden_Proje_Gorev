using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjeGorevYonetimi.Data;
using ProjeGorevYonetimi.Models.Entities;
using ProjeGorevYonetimi.Services;

namespace ProjeGorevYonetimi.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[ServiceFilter(typeof(Filters.ApiKeyAuthFilter))]
public class ParametreController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IYetkiService _yetkiService;

    public ParametreController(ApplicationDbContext db, IYetkiService yetkiService)
    {
        _db = db;
        _yetkiService = yetkiService;
    }

    [HttpGet("durumlar")]
    public async Task<ActionResult<List<DurumDto>>> GetDurumlar(CancellationToken ct)
    {
        var list = await _db.Durumlar.AsNoTracking().OrderBy(d => d.Sira).Select(d => new DurumDto { Id = d.Id, Ad = d.Ad, Sira = d.Sira }).ToListAsync(ct);
        return Ok(list);
    }

    [HttpPost("durumlar")]
    public async Task<ActionResult<DurumDto>> PostDurum([FromBody] DurumDto? dto, CancellationToken ct)
    {
        if (!_yetkiService.GenelYetkiliMi())
            return Forbid();
        if (dto == null || string.IsNullOrWhiteSpace(dto.Ad))
            return BadRequest(new { message = "Ad alanı gereklidir." });
        var maxSira = await _db.Durumlar.Select(d => (int?)d.Sira).MaxAsync(ct) ?? 0;
        var entity = new Durum { Ad = dto.Ad.Trim(), Sira = dto.Sira > 0 ? dto.Sira : maxSira + 1 };
        _db.Durumlar.Add(entity);
        await _db.SaveChangesAsync(ct);
        return Ok(new DurumDto { Id = entity.Id, Ad = entity.Ad, Sira = entity.Sira });
    }

    [HttpPut("durumlar/{id}")]
    public async Task<IActionResult> PutDurum(int id, [FromBody] DurumDto dto, CancellationToken ct)
    {
        if (!_yetkiService.GenelYetkiliMi())
            return Forbid();
        var entity = await _db.Durumlar.FindAsync(new object[] { id }, ct);
        if (entity == null) return NotFound();
        entity.Ad = dto.Ad;
        entity.Sira = dto.Sira;
        await _db.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpDelete("durumlar/{id}")]
    public async Task<IActionResult> DeleteDurum(int id, CancellationToken ct)
    {
        if (!_yetkiService.GenelYetkiliMi())
            return Forbid();
        var entity = await _db.Durumlar.FindAsync(new object[] { id }, ct);
        if (entity == null) return NotFound();
        entity.IsDeleted = true;
        entity.DeleteDate = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpGet("kategoriler")]
    public async Task<ActionResult<List<KategoriDto>>> GetKategoriler(CancellationToken ct)
    {
        var list = await _db.ProjeDetayKategoriler.AsNoTracking().OrderBy(k => k.Sira).Select(k => new KategoriDto { Id = k.Id, Ad = k.Ad, Sira = k.Sira }).ToListAsync(ct);
        return Ok(list);
    }

    [HttpPost("kategoriler")]
    public async Task<ActionResult<KategoriDto>> PostKategori([FromBody] KategoriDto? dto, CancellationToken ct)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Ad))
            return BadRequest(new { message = "Ad alanı gereklidir." });
        var maxSira = await _db.ProjeDetayKategoriler.Select(k => (int?)k.Sira).MaxAsync(ct) ?? 0;
        var entity = new ProjeDetayKategori { Ad = dto.Ad.Trim(), Sira = dto.Sira > 0 ? dto.Sira : maxSira + 1 };
        _db.ProjeDetayKategoriler.Add(entity);
        await _db.SaveChangesAsync(ct);
        return Ok(new KategoriDto { Id = entity.Id, Ad = entity.Ad, Sira = entity.Sira });
    }

    [HttpPut("kategoriler/{id}")]
    public async Task<IActionResult> PutKategori(int id, [FromBody] KategoriDto dto, CancellationToken ct)
    {
        var entity = await _db.ProjeDetayKategoriler.FindAsync(new object[] { id }, ct);
        if (entity == null) return NotFound();
        entity.Ad = dto.Ad;
        entity.Sira = dto.Sira;
        await _db.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpDelete("kategoriler/{id}")]
    public async Task<IActionResult> DeleteKategori(int id, CancellationToken ct)
    {
        var entity = await _db.ProjeDetayKategoriler.FindAsync(new object[] { id }, ct);
        if (entity == null) return NotFound();
        entity.IsDeleted = true;
        entity.DeleteDate = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpGet("adet-birimleri")]
    public async Task<ActionResult<List<AdetBirimiDto>>> GetAdetBirimleri(CancellationToken ct)
    {
        var list = await _db.AdetBirimleri.AsNoTracking()
            .OrderBy(a => a.Sira)
            .Select(a => new AdetBirimiDto { Id = a.Id, Ad = a.Ad, Sira = a.Sira })
            .ToListAsync(ct);
        return Ok(list);
    }

    [HttpPost("adet-birimleri")]
    public async Task<ActionResult<AdetBirimiDto>> PostAdetBirimi([FromBody] AdetBirimiDto? dto, CancellationToken ct)
    {
        if (!_yetkiService.GenelYetkiliMi())
            return Forbid();
        if (dto == null || string.IsNullOrWhiteSpace(dto.Ad))
            return BadRequest(new { message = "Ad alanı gereklidir." });

        var maxSira = await _db.AdetBirimleri.Select(a => (int?)a.Sira).MaxAsync(ct) ?? 0;
        var entity = new AdetBirimi { Ad = dto.Ad.Trim(), Sira = dto.Sira > 0 ? dto.Sira : maxSira + 1 };
        _db.AdetBirimleri.Add(entity);
        await _db.SaveChangesAsync(ct);
        return Ok(new AdetBirimiDto { Id = entity.Id, Ad = entity.Ad, Sira = entity.Sira });
    }

    [HttpPut("adet-birimleri/{id}")]
    public async Task<IActionResult> PutAdetBirimi(int id, [FromBody] AdetBirimiDto dto, CancellationToken ct)
    {
        if (!_yetkiService.GenelYetkiliMi())
            return Forbid();
        var entity = await _db.AdetBirimleri.FindAsync(new object[] { id }, ct);
        if (entity == null) return NotFound();
        entity.Ad = dto.Ad;
        entity.Sira = dto.Sira;
        await _db.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpDelete("adet-birimleri/{id}")]
    public async Task<IActionResult> DeleteAdetBirimi(int id, CancellationToken ct)
    {
        if (!_yetkiService.GenelYetkiliMi())
            return Forbid();
        var entity = await _db.AdetBirimleri.FindAsync(new object[] { id }, ct);
        if (entity == null) return NotFound();
        entity.IsDeleted = true;
        entity.DeleteDate = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok();
    }
}

public class DurumDto
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("ad")] public string Ad { get; set; } = "";
    [JsonPropertyName("sira")] public int Sira { get; set; }
}
public class KategoriDto
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("ad")] public string Ad { get; set; } = "";
    [JsonPropertyName("sira")] public int Sira { get; set; }
}

public class AdetBirimiDto
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("ad")] public string Ad { get; set; } = "";
    [JsonPropertyName("sira")] public int Sira { get; set; }
}
