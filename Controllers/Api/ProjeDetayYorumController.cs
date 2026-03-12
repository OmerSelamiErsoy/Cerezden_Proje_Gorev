using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjeGorevYonetimi.Data;
using ProjeGorevYonetimi.Models.Entities;
using ProjeGorevYonetimi.Services;

namespace ProjeGorevYonetimi.Controllers.Api;

[Route("api/projeler/{projeId:int}/detaylar/{detayId:int}/yorumlar")]
[ApiController]
[ServiceFilter(typeof(Filters.ApiKeyAuthFilter))]
public class ProjeDetayYorumController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IYetkiService _yetki;
    private readonly ICurrentUserService _currentUser;
    private readonly IWebHostEnvironment _env;

    public ProjeDetayYorumController(ApplicationDbContext db, IYetkiService yetki, ICurrentUserService currentUser, IWebHostEnvironment env)
    {
        _db = db;
        _yetki = yetki;
        _currentUser = currentUser;
        _env = env;
    }

    [HttpGet]
    public async Task<ActionResult<List<YorumDto>>> List(int projeId, int detayId, CancellationToken ct)
    {
        if (!await _yetki.ProjeGorebilirMiAsync(projeId, ct)) return Forbid();
        var list = await _db.ProjeDetayYorumlar
            .Where(y => y.ProjeDetayId == detayId)
            .OrderByDescending(y => y.InsertDate)
            .Include(y => y.InsertedByUser)
            .Include(y => y.Dosyalar)
            .Select(y => new YorumDto
            {
                Id = y.Id,
                YorumMetni = y.YorumMetni,
                InsertDate = y.InsertDate,
                InsertedByAdSoyad = y.InsertedByUser != null ? y.InsertedByUser.AdSoyad : null,
                Dosyalar = y.Dosyalar!.Select(f => new DosyaDto { Id = f.Id, DosyaAdi = f.DosyaAdi, DosyaYolu = f.DosyaYolu }).ToList()
            })
            .ToListAsync(ct);
        return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create(int projeId, int detayId, [FromBody] YorumCreateDto dto, CancellationToken ct)
    {
        if (!await _yetki.ProjeGorebilirMiAsync(projeId, ct)) return Forbid();
        var detay = await _db.ProjeDetaylar.AnyAsync(d => d.Id == detayId && d.ProjeId == projeId, ct);
        if (!detay) return NotFound();
        var userId = _currentUser.GetCurrentUserId();
        var entity = new ProjeDetayYorum { ProjeDetayId = detayId, YorumMetni = dto.YorumMetni };
        entity.InsertedByUserId = userId;
        _db.ProjeDetayYorumlar.Add(entity);
        await _db.SaveChangesAsync(ct);
        return Ok(new { id = entity.Id });
    }

    [HttpPost("upload")]
    public async Task<ActionResult<object>> Upload(int projeId, int detayId, int yorumId, IFormFile file, CancellationToken ct)
    {
        if (!await _yetki.ProjeGorebilirMiAsync(projeId, ct)) return Forbid();
        var yorum = await _db.ProjeDetayYorumlar.FirstOrDefaultAsync(y => y.Id == yorumId && y.ProjeDetayId == detayId, ct);
        if (yorum == null) return NotFound();
        var uploads = Path.Combine(_env.WebRootPath, "uploads", "proje-detay");
        Directory.CreateDirectory(uploads);
        var safeName = $"{Guid.NewGuid()}_{file.FileName}";
        var path = Path.Combine(uploads, safeName);
        await using (var stream = System.IO.File.Create(path))
            await file.CopyToAsync(stream, ct);
        var relPath = $"/uploads/proje-detay/{safeName}";
        _db.ProjeDetayYorumDosyalar.Add(new ProjeDetayYorumDosya { ProjeDetayYorumId = yorumId, DosyaAdi = file.FileName, DosyaYolu = relPath });
        await _db.SaveChangesAsync(ct);
        return Ok(new { path = relPath, name = file.FileName });
    }
}

public class YorumDto
{
    public int Id { get; set; }
    public string YorumMetni { get; set; } = "";
    public DateTime? InsertDate { get; set; }
    public string? InsertedByAdSoyad { get; set; }
    public List<DosyaDto> Dosyalar { get; set; } = new();
}
public class DosyaDto { public int Id { get; set; } public string DosyaAdi { get; set; } = ""; public string DosyaYolu { get; set; } = ""; }
public class YorumCreateDto { public string YorumMetni { get; set; } = ""; }
