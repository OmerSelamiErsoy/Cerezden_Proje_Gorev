using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjeGorevYonetimi.Data;
using ProjeGorevYonetimi.Models.Entities;

namespace ProjeGorevYonetimi.Controllers.Api;

[Route("api/[controller]")]
[Route("api/KullaniciApi")]
[ApiController]
[ServiceFilter(typeof(Filters.ApiKeyAuthFilter))]
public class KullanicilarController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public KullanicilarController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<KullaniciDto>>> List([FromQuery] bool? sadeceAktif = true, CancellationToken ct = default)
    {
        var query = _db.Kullanicilar.AsQueryable();
        if (sadeceAktif == true)
            query = query.Where(k => k.AktifMi);
        var list = await query
            .AsNoTracking()
            .OrderBy(k => k.AdSoyad)
            .Select(k => new KullaniciDto
            {
                Id = k.Id,
                CerezdenKullaniciId = k.CerezdenKullaniciId,
                AdSoyad = k.AdSoyad,
                CepTel = k.CepTel,
                SirketKodu = k.SirketKodu,
                AktifMi = k.AktifMi
            })
            .ToListAsync(ct);
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<KullaniciDto>> Get(int id, CancellationToken ct)
    {
        var k = await _db.Kullanicilar
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new KullaniciDto
            {
                Id = x.Id,
                CerezdenKullaniciId = x.CerezdenKullaniciId,
                AdSoyad = x.AdSoyad,
                CepTel = x.CepTel,
                SirketKodu = x.SirketKodu,
                AktifMi = x.AktifMi
            })
            .FirstOrDefaultAsync(ct);
        if (k == null) return NotFound();
        return Ok(k);
    }

    [HttpPost]
    public async Task<ActionResult<KullaniciDto>> Create([FromBody] KullaniciCreateUpdateRequest request, CancellationToken ct)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.AdSoyad))
            return BadRequest("AdSoyad zorunludur.");

        var entity = new Kullanici
        {
            CerezdenKullaniciId = request.CerezdenKullaniciId,
            AdSoyad = request.AdSoyad.Trim(),
            CepTel = request.CepTel?.Trim(),
            SirketKodu = request.SirketKodu?.Trim(),
            AktifMi = request.AktifMi
        };
        _db.Kullanicilar.Add(entity);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(Get), new { id = entity.Id }, new KullaniciDto
        {
            Id = entity.Id,
            CerezdenKullaniciId = entity.CerezdenKullaniciId,
            AdSoyad = entity.AdSoyad,
            CepTel = entity.CepTel,
            SirketKodu = entity.SirketKodu,
            AktifMi = entity.AktifMi
        });
    }

    [HttpPut("{cerezdenKullaniciId:int}")]
    public async Task<ActionResult<KullaniciDto>> Update(int cerezdenKullaniciId, [FromBody] KullaniciCreateUpdateRequest request, CancellationToken ct)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.AdSoyad))
            return BadRequest("AdSoyad zorunludur.");

        var entity = await _db.Kullanicilar.FirstOrDefaultAsync(k => k.CerezdenKullaniciId == cerezdenKullaniciId, ct);
        if (entity == null) return NotFound();

        entity.CerezdenKullaniciId = request.CerezdenKullaniciId;
        entity.AdSoyad = request.AdSoyad.Trim();
        entity.CepTel = request.CepTel?.Trim();
        entity.SirketKodu = request.SirketKodu?.Trim();
        entity.AktifMi = request.AktifMi;
        await _db.SaveChangesAsync(ct);

        return Ok(new KullaniciDto
        {
            Id = entity.Id,
            CerezdenKullaniciId = entity.CerezdenKullaniciId,
            AdSoyad = entity.AdSoyad,
            CepTel = entity.CepTel,
            SirketKodu = entity.SirketKodu,
            AktifMi = entity.AktifMi
        });
    }

    [HttpDelete("{cerezdenKullaniciId:int}")]
    public async Task<ActionResult> Delete(int cerezdenKullaniciId, CancellationToken ct)
    {
        var entity = await _db.Kullanicilar.FirstOrDefaultAsync(k => k.CerezdenKullaniciId == cerezdenKullaniciId, ct);
        if (entity == null) return NotFound();

        entity.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}

public class KullaniciDto
{
    public int Id { get; set; }
    public int CerezdenKullaniciId { get; set; }
    public string AdSoyad { get; set; } = "";
    public string? CepTel { get; set; }
    public string? SirketKodu { get; set; }
    public bool AktifMi { get; set; }
}

public class KullaniciCreateUpdateRequest
{
    public int CerezdenKullaniciId { get; set; }
    public string AdSoyad { get; set; } = "";
    public string? CepTel { get; set; }
    public string? SirketKodu { get; set; }
    public bool AktifMi { get; set; } = true;
}
