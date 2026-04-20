using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjeGorevYonetimi.Data;
using ProjeGorevYonetimi.Models.Entities;
using ProjeGorevYonetimi.Services;

namespace ProjeGorevYonetimi.Controllers.Api;

[Route("api/notlar")]
[ApiController]
[ServiceFilter(typeof(Filters.ApiKeyAuthFilter))]
public class NotlarController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IYetkiService _yetki;
    private readonly IWebHostEnvironment _env;

    public NotlarController(ApplicationDbContext db, ICurrentUserService currentUser, IYetkiService yetki, IWebHostEnvironment env)
    {
        _db = db;
        _currentUser = currentUser;
        _yetki = yetki;
        _env = env;
    }

    private async Task<int> EnsureGenelGrupIdAsync(int userId, CancellationToken ct)
    {
        var mevcutId = await _db.NotGruplar
            .AsNoTracking()
            .Where(g => g.OlusturanKullaniciId == userId && g.Ad == "Genel")
            .Select(g => g.Id)
            .FirstOrDefaultAsync(ct);
        if (mevcutId != 0) return mevcutId;

        var maxSira = await _db.NotGruplar
            .Where(g => g.OlusturanKullaniciId == userId)
            .Select(x => (int?)x.Sira)
            .MaxAsync(ct) ?? 0;

        var entity = new NotGrup
        {
            Ad = "Genel",
            Aciklama = null,
            Sira = maxSira + 1,
            YetkiTipi = 0,
            OlusturanKullaniciId = userId
        };
        _db.NotGruplar.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.Id;
    }

    private async Task<Not?> GetVisibleNotAsync(int notId, int userId, bool genelYetkili, CancellationToken ct)
    {
        var query = _db.Notlar
            .Include(n => n.YetkiKullanicilar)
            .Include(n => n.OlusturanKullanici)
            .AsQueryable();

        if (!genelYetkili)
        {
            query = query.Where(n =>
                n.OlusturanKullaniciId == userId ||
                (n.YetkiTipi == 1 && n.YetkiKullanicilar!.Any(y => y.KullaniciId == userId)));
        }

        return await query.FirstOrDefaultAsync(n => n.Id == notId, ct);
    }

    // Liste: sadece not adı
    [HttpGet]
    public async Task<ActionResult<List<NotListDto>>> Liste(CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        var genelYetkili = _yetki.GenelYetkiliMi();
        await EnsureGenelGrupIdAsync(userId, ct);

        var query = _db.Notlar
            .AsNoTracking()
            .Include(n => n.YetkiKullanicilar)
            .AsQueryable();

        if (!genelYetkili)
        {
            query = query.Where(n =>
                n.OlusturanKullaniciId == userId ||
                (n.YetkiTipi == 1 && n.YetkiKullanicilar!.Any(y => y.KullaniciId == userId)));
        }

        var list = await query
            .OrderByDescending(n => n.UpdateDate ?? n.InsertDate)
            .ThenByDescending(n => n.Id)
            .Select(n => new NotListDto
            {
                Id = n.Id,
                Baslik = n.Baslik,
                YetkiTipi = n.YetkiTipi,
                InsertDate = n.InsertDate,
                OlusturanAdSoyad = n.OlusturanKullanici.AdSoyad
            })
            .ToListAsync(ct);

        return Ok(list);
    }

    // Not eklerken: HTML editör içeriği + paylaşım
    [HttpPost]
    public async Task<ActionResult<object>> Olustur([FromBody] NotCreateDto dto, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        var grupId = await EnsureGenelGrupIdAsync(userId, ct);
        var maxSira = await _db.Notlar.Where(x => x.NotGrupId == grupId).Select(x => (int?)x.Sira).MaxAsync(ct) ?? 0;

        var entity = new Not
        {
            NotGrupId = grupId,
            Baslik = dto.Baslik?.Trim() ?? "",
            IcerikHtml = dto.IcerikHtml ?? "",
            Sira = maxSira + 1,
            YetkiTipi = dto.YetkiTipi,
            OlusturanKullaniciId = userId
        };

        if (string.IsNullOrWhiteSpace(entity.Baslik))
            return BadRequest(new { message = "Not adı zorunludur." });

        _db.Notlar.Add(entity);
        await _db.SaveChangesAsync(ct);

        if (dto.YetkiTipi == 1 && dto.YetkiKullaniciIds != null && dto.YetkiKullaniciIds.Count > 0)
        {
            foreach (var kid in dto.YetkiKullaniciIds.Distinct())
            {
                if (kid == userId) continue;
                _db.NotYetkiKullanicilar.Add(new NotYetkiKullanici { NotId = entity.Id, KullaniciId = kid });
            }
            await _db.SaveChangesAsync(ct);
        }

        return Ok(new { id = entity.Id });
    }

    // Detay: içerik + kimlerle paylaşıldı
    [HttpGet("{id:int}")]
    public async Task<ActionResult<NotDetayDto>> Detay(int id, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        var genelYetkili = _yetki.GenelYetkiliMi();
        var n = await GetVisibleNotAsync(id, userId, genelYetkili, ct);
        if (n == null) return Forbid();

        var paylasilanIds = n.YetkiKullanicilar != null ? n.YetkiKullanicilar.Select(y => y.KullaniciId).ToList() : new List<int>();
        var paylasilanKisiler = paylasilanIds.Count == 0
            ? new List<KisiDto>()
            : await _db.Kullanicilar
                .AsNoTracking()
                .Where(k => paylasilanIds.Contains(k.Id))
                .OrderBy(k => k.AdSoyad)
                .Select(k => new KisiDto { Id = k.Id, AdSoyad = k.AdSoyad, CepTel = k.CepTel })
                .ToListAsync(ct);

        return Ok(new NotDetayDto
        {
            Id = n.Id,
            Baslik = n.Baslik,
            IcerikHtml = n.IcerikHtml,
            YetkiTipi = n.YetkiTipi,
            YetkiKullaniciIds = paylasilanIds,
            PaylasilanKisiler = paylasilanKisiler,
            CanDelete = n.OlusturanKullaniciId == userId,
            OlusturanAdSoyad = n.OlusturanKullanici?.AdSoyad,
            InsertDate = n.InsertDate
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Guncelle(int id, [FromBody] NotUpdateDto dto, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        var genelYetkili = _yetki.GenelYetkiliMi();
        var visible = await GetVisibleNotAsync(id, userId, genelYetkili, ct);
        if (visible == null) return Forbid();

        var entity = await _db.Notlar.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity == null) return NotFound();
        if (!genelYetkili && entity.OlusturanKullaniciId != userId) return Forbid();

        entity.Baslik = dto.Baslik?.Trim() ?? "";
        entity.IcerikHtml = dto.IcerikHtml ?? "";
        entity.YetkiTipi = dto.YetkiTipi;

        if (string.IsNullOrWhiteSpace(entity.Baslik))
            return BadRequest(new { message = "Not adı zorunludur." });

        var mevcut = await _db.NotYetkiKullanicilar.Where(y => y.NotId == id).ToListAsync(ct);
        _db.NotYetkiKullanicilar.RemoveRange(mevcut);

        if (dto.YetkiTipi == 1 && dto.YetkiKullaniciIds != null && dto.YetkiKullaniciIds.Count > 0)
        {
            foreach (var kid in dto.YetkiKullaniciIds.Distinct())
            {
                if (kid == userId) continue;
                _db.NotYetkiKullanicilar.Add(new NotYetkiKullanici { NotId = id, KullaniciId = kid });
            }
        }

        await _db.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Sil(int id, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        var entity = await _db.Notlar.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity == null) return NotFound();
        if (entity.OlusturanKullaniciId != userId) return Forbid();

        entity.IsDeleted = true;
        entity.DeleteDate = DateTime.Now;
        entity.DeletedByUserId = userId;
        entity.UpdateDate = DateTime.Now;
        entity.UpdatedByUserId = userId;

        await _db.SaveChangesAsync(ct);
        return Ok();
    }

    // CKEditor image upload
    [HttpPost("{id:int}/gorsel/upload")]
    public async Task<ActionResult<object>> GorselUpload(int id, IFormFile file, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        var genelYetkili = _yetki.GenelYetkiliMi();
        var n = await GetVisibleNotAsync(id, userId, genelYetkili, ct);
        if (n == null) return Forbid();
        if (file == null || file.Length == 0) return BadRequest(new { message = "Dosya seçilmedi." });

        var uploads = Path.Combine(_env.WebRootPath, "uploads", "notlar", "images");
        Directory.CreateDirectory(uploads);
        var safeName = $"{Guid.NewGuid()}_{file.FileName}";
        var path = Path.Combine(uploads, safeName);
        await using (var stream = System.IO.File.Create(path))
            await file.CopyToAsync(stream, ct);
        var relPath = $"/uploads/notlar/images/{safeName}";

        return Ok(new { url = relPath, @default = relPath });
    }

    // Yeni not ekranı için: not-id olmadan görsel upload
    [HttpPost("gorsel/upload")]
    public async Task<ActionResult<object>> GorselUploadGenel(IFormFile file, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        if (userId <= 0) return Forbid();
        if (file == null || file.Length == 0) return BadRequest(new { message = "Dosya seçilmedi." });

        var uploads = Path.Combine(_env.WebRootPath, "uploads", "notlar", "images");
        Directory.CreateDirectory(uploads);
        var safeName = $"{Guid.NewGuid()}_{file.FileName}";
        var path = Path.Combine(uploads, safeName);
        await using (var stream = System.IO.File.Create(path))
            await file.CopyToAsync(stream, ct);
        var relPath = $"/uploads/notlar/images/{safeName}";

        return Ok(new { url = relPath, @default = relPath });
    }

    public class NotListDto
    {
        public int Id { get; set; }
        public string Baslik { get; set; } = "";
        public int YetkiTipi { get; set; }
        public DateTime? InsertDate { get; set; }
        public string? OlusturanAdSoyad { get; set; }
    }

    public class NotCreateDto
    {
        public string? Baslik { get; set; }
        public string? IcerikHtml { get; set; }
        public int YetkiTipi { get; set; } = 0;
        public List<int>? YetkiKullaniciIds { get; set; }
    }

    public class NotUpdateDto
    {
        public string? Baslik { get; set; }
        public string? IcerikHtml { get; set; }
        public int YetkiTipi { get; set; } = 0;
        public List<int>? YetkiKullaniciIds { get; set; }
    }

    public class KisiDto
    {
        public int Id { get; set; }
        public string AdSoyad { get; set; } = "";
        public string? CepTel { get; set; }
    }

    public class NotDetayDto
    {
        public int Id { get; set; }
        public string Baslik { get; set; } = "";
        public string IcerikHtml { get; set; } = "";
        public int YetkiTipi { get; set; }
        public List<int> YetkiKullaniciIds { get; set; } = new();
        public List<KisiDto> PaylasilanKisiler { get; set; } = new();
        public bool CanDelete { get; set; }
        public string? OlusturanAdSoyad { get; set; }
        public DateTime? InsertDate { get; set; }
    }
}

