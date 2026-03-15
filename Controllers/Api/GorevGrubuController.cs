using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjeGorevYonetimi.Data;
using ProjeGorevYonetimi.Models.Entities;
using ProjeGorevYonetimi.Services;

namespace ProjeGorevYonetimi.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[ServiceFilter(typeof(Filters.ApiKeyAuthFilter))]
public class GorevGrubuController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IYetkiService _yetki;

    public GorevGrubuController(ApplicationDbContext db, ICurrentUserService currentUser, IYetkiService yetki)
    {
        _db = db;
        _currentUser = currentUser;
        _yetki = yetki;
    }

    /// <summary>
    /// Kullanıcının oluşturduğu gruplar + içinde kendisine atanan görev olan gruplar.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<GorevGrubuListDto>>> List(CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        var benimGruplarim = await _db.GorevGruplar
            .AsNoTracking()
            .Where(g => g.OlusturanKullaniciId == userId)
            .Select(g => new { g.Id, g.Ad, g.Aciklama })
            .ToListAsync(ct);
        var atananGorevIds = await _db.GorevAtamalar.AsNoTracking().Where(a => a.KullaniciId == userId).Select(a => a.GorevId).ToListAsync(ct);
        var banaAtananGorevlerinGrupIds = await _db.Gorevler
            .AsNoTracking()
            .Where(g => atananGorevIds.Contains(g.Id) && g.GorevGrubuId != null)
            .Select(g => g.GorevGrubuId!.Value)
            .Distinct()
            .ToListAsync(ct);
        var digerGruplar = await _db.GorevGruplar
            .AsNoTracking()
            .Where(g => banaAtananGorevlerinGrupIds.Contains(g.Id) && g.OlusturanKullaniciId != userId)
            .Select(g => new { g.Id, g.Ad, g.Aciklama })
            .ToListAsync(ct);

        var tumGrupIds = benimGruplarim.Select(x => x.Id).Union(digerGruplar.Select(x => x.Id)).Distinct().ToList();
        var benimOlusturdugumGorevIds = await _db.Gorevler.AsNoTracking().Where(g => g.OlusturanKullaniciId == userId).Select(g => g.Id).ToListAsync(ct);
        var benimGorevIds = benimOlusturdugumGorevIds.Union(atananGorevIds).Distinct().ToList();
        var gorevSayilari = await _db.Gorevler
            .AsNoTracking()
            .Where(g => g.GorevGrubuId != null && tumGrupIds.Contains(g.GorevGrubuId!.Value) && benimGorevIds.Contains(g.Id))
            .GroupBy(g => g.GorevGrubuId!.Value)
            .Select(x => new { GrupId = x.Key, Sayi = x.Count() })
            .ToListAsync(ct);
        var sayiDict = gorevSayilari.ToDictionary(x => x.GrupId, x => x.Sayi);

        var result = new List<GorevGrubuListDto>();
        foreach (var g in benimGruplarim)
            result.Add(new GorevGrubuListDto { Id = g.Id, Ad = g.Ad, Aciklama = g.Aciklama, GorevSayisi = sayiDict.GetValueOrDefault(g.Id, 0), BenimGrubum = true });
        foreach (var g in digerGruplar)
        {
            if (result.Any(x => x.Id == g.Id)) continue;
            result.Add(new GorevGrubuListDto { Id = g.Id, Ad = g.Ad, Aciklama = g.Aciklama, GorevSayisi = sayiDict.GetValueOrDefault(g.Id, 0), BenimGrubum = false });
        }
        var list = result.OrderByDescending(x => x.BenimGrubum).ThenBy(x => x.Ad).ToList();
        return Ok(new { list, genelYetkiliMi = _yetki.GenelYetkiliMi() });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GorevGrubuDetayDto>> Get(int id, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        var g = await _db.GorevGruplar
            .AsNoTracking()
            .Include(x => x.OlusturanKullanici)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (g == null) return NotFound();
        var benimGrubum = g.OlusturanKullaniciId == userId;
        var gorevSayisi = await _db.Gorevler.AsNoTracking().CountAsync(x => x.GorevGrubuId == id, ct);
        return Ok(new GorevGrubuDetayDto
        {
            Id = g.Id,
            Ad = g.Ad,
            Aciklama = g.Aciklama,
            OlusturanAdSoyad = g.OlusturanKullanici?.AdSoyad,
            BenimGrubum = benimGrubum,
            GorevSayisi = gorevSayisi
        });
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] GorevGrubuCreateDto dto, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        var g = new GorevGrubu { Ad = dto.Ad, Aciklama = dto.Aciklama, OlusturanKullaniciId = userId };
        _db.GorevGruplar.Add(g);
        await _db.SaveChangesAsync(ct);
        return Ok(new { id = g.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] GorevGrubuCreateDto dto, CancellationToken ct)
    {
        var g = await _db.GorevGruplar.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (g == null) return NotFound();
        if (g.OlusturanKullaniciId != _currentUser.GetCurrentUserId()) return Forbid();
        g.Ad = dto.Ad;
        g.Aciklama = dto.Aciklama;
        await _db.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var g = await _db.GorevGruplar.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (g == null) return NotFound();
        var userId = _currentUser.GetCurrentUserId();
        if (g.OlusturanKullaniciId != userId && !_yetki.GenelYetkiliMi()) return Forbid();
        g.IsDeleted = true;
        g.DeleteDate = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok();
    }
}

public class GorevGrubuListDto
{
    public int Id { get; set; }
    public string Ad { get; set; } = "";
    public string? Aciklama { get; set; }
    public int GorevSayisi { get; set; }
    public bool BenimGrubum { get; set; }
}
public class GorevGrubuDetayDto
{
    public int Id { get; set; }
    public string Ad { get; set; } = "";
    public string? Aciklama { get; set; }
    public string? OlusturanAdSoyad { get; set; }
    public bool BenimGrubum { get; set; }
    public int GorevSayisi { get; set; }
}
public class GorevGrubuCreateDto
{
    public string Ad { get; set; } = "";
    public string? Aciklama { get; set; }
}
