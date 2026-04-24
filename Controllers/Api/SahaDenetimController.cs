using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjeGorevYonetimi.Data;
using ProjeGorevYonetimi.Models.Entities;
using ProjeGorevYonetimi.Services;
using System.Text.Json;

namespace ProjeGorevYonetimi.Controllers.Api;

[Route("api/saha-denetim")]
[ApiController]
[ServiceFilter(typeof(Filters.ApiKeyAuthFilter))]
public class SahaDenetimController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IYetkiService _yetki;
    private readonly IWebHostEnvironment _env;
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _config;

    public SahaDenetimController(ApplicationDbContext db, ICurrentUserService currentUser, IYetkiService yetki, IWebHostEnvironment env, IHttpClientFactory http, IConfiguration config)
    {
        _db = db;
        _currentUser = currentUser;
        _yetki = yetki;
        _env = env;
        _http = http;
        _config = config;
    }

    [HttpGet("lokasyonlar")]
    public async Task<ActionResult<List<LokasyonDto>>> Lokasyonlar(CancellationToken ct)
    {
        var apiKey = _config["personelApiAuthGuid"];
        var baseUrl = _config["personelLokasyonApiUrl"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return Ok(new List<LokasyonDto>());
        if (string.IsNullOrWhiteSpace(baseUrl))
            return Ok(new List<LokasyonDto>());

        var url = $"{baseUrl}{Uri.EscapeDataString(apiKey)}";
        var client = _http.CreateClient();
        var resp = await client.GetAsync(url, ct);
        if (!resp.IsSuccessStatusCode)
            return Ok(new List<LokasyonDto>());

        var json = await resp.Content.ReadAsStringAsync(ct);
        var list = JsonSerializer.Deserialize<List<LokasyonApiDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                   ?? new List<LokasyonApiDto>();

        var result = list
            .Where(x => !string.IsNullOrWhiteSpace(x.LokasyonAdi) && !string.IsNullOrWhiteSpace(x.LokasyonMagazaKodu))
            .Select(x => new LokasyonDto
            {
                LokasyonAdi = x.LokasyonAdi!.Trim(),
                LokasyonMagazaKodu = x.LokasyonMagazaKodu!.Trim()
            })
            .OrderBy(x => x.LokasyonAdi)
            .ToList();

        return Ok(result);
    }

    private async Task<string?> ResolveLokasyonAdiAsync(int? lokasyonId, CancellationToken ct)
    {
        if (!lokasyonId.HasValue) return null;
        var apiKey = _config["personelApiAuthGuid"];
        var baseUrl = _config["personelLokasyonApiUrl"];
        if (string.IsNullOrWhiteSpace(apiKey)) return null;
        if (string.IsNullOrWhiteSpace(baseUrl)) return null;

        var url = $"{baseUrl}{Uri.EscapeDataString(apiKey)}";
        var client = _http.CreateClient();
        var resp = await client.GetAsync(url, ct);
        if (!resp.IsSuccessStatusCode) return null;

        var json = await resp.Content.ReadAsStringAsync(ct);
        var list = JsonSerializer.Deserialize<List<LokasyonApiDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                   ?? new List<LokasyonApiDto>();

        var hit = list.FirstOrDefault(x => int.TryParse((x.LokasyonMagazaKodu ?? "").Trim(), out var n) && n == lokasyonId.Value);
        return hit?.LokasyonAdi?.Trim();
    }

    [HttpGet]
    public async Task<ActionResult<List<DenetimListDto>>> Liste(CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        var genelYetkili = _yetki.GenelYetkiliMi();

        var q = _db.SahaDenetimler.AsNoTracking().Include(x => x.OlusturanKullanici).AsQueryable();
        if (!genelYetkili)
            q = q.Where(x => x.OlusturanKullaniciId == userId);

        var list = await q
            .OrderByDescending(x => x.KayitTarihi)
            .ThenByDescending(x => x.Id)
            .Select(x => new DenetimListDto
            {
                Id = x.Id,
                Ad = x.Ad,
                KayitTarihi = x.KayitTarihi,
                OlusturanAdSoyad = x.OlusturanKullanici.AdSoyad,
                KapaliMi = x.KapaliMi,
                LokasyonId = x.LokasyonId,
                LokasyonAdi = x.LokasyonAdi,
                AdimSayisi = _db.SahaDenetimAdimlar.Count(a => a.SahaDenetimId == x.Id)
            })
            .ToListAsync(ct);

        return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult<object>> Olustur([FromBody] DenetimCreateDto dto, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(dto.Ad))
            return BadRequest(new { message = "Denetim adı zorunludur." });

        int? lokasyonId = null;
        if (!string.IsNullOrWhiteSpace(dto.LokasyonMagazaKodu) && int.TryParse(dto.LokasyonMagazaKodu.Trim(), out var lid))
            lokasyonId = lid;
        var lokasyonAdi = await ResolveLokasyonAdiAsync(lokasyonId, ct);

        var entity = new SahaDenetim
        {
            Ad = dto.Ad.Trim(),
            KayitTarihi = dto.KayitTarihi ?? DateTime.Now,
            OlusturanKullaniciId = userId,
            KapaliMi = false,
            LokasyonId = lokasyonId,
            LokasyonAdi = lokasyonAdi
        };
        _db.SahaDenetimler.Add(entity);
        await _db.SaveChangesAsync(ct);
        return Ok(new { id = entity.Id });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DenetimDetayDto>> Detay(int id, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        var genelYetkili = _yetki.GenelYetkiliMi();

        var q = _db.SahaDenetimler
            .AsNoTracking()
            .Include(x => x.OlusturanKullanici)
            .Include(x => x.KapatanKullanici)
            .Include(x => x.GeriAcanKullanici)
            .Include(x => x.Adimlar!)
                .ThenInclude(a => a.Kategori)
            .Include(x => x.Adimlar!)
                .ThenInclude(a => a.IlgiliKisiler)
            .Include(x => x.Adimlar!)
                .ThenInclude(a => a.IlgiliUrunler)
            .Include(x => x.Adimlar!)
                .ThenInclude(a => a.Fotograflar)
            .AsQueryable();
        if (!genelYetkili)
            q = q.Where(x => x.OlusturanKullaniciId == userId);

        var denetim = await q.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (denetim == null) return Forbid();

        var adimlar = (denetim.Adimlar ?? new List<SahaDenetimAdim>())
            .OrderBy(a => a.SahaDenetimKategoriId.HasValue ? 0 : 1)
            .ThenBy(a => a.Kategori != null ? a.Kategori.Ad : "")
            .ThenBy(a => a.Sira)
            .ThenBy(a => a.Id)
            .Select(a => new DenetimAdimDto
            {
                Id = a.Id,
                Ad = a.Ad,
                Aciklama = a.Aciklama,
                KategoriId = a.SahaDenetimKategoriId,
                KategoriAd = a.Kategori?.Ad,
                Tipler = a.Tipler,
                Sira = a.Sira,
                YapildiMi = a.YapildiMi,
                Puan = a.Puan,
                PersonelYorum = a.PersonelYorum,
                Not = a.Not,
                IlgiliKisiler = (a.IlgiliKisiler ?? new List<SahaDenetimAdimIlgiliKisi>())
                    .OrderBy(x => x.Sira).ThenBy(x => x.Id)
                    .Select(x => new IlgiliKisiDto { Id = x.Id, AdSoyad = x.AdSoyad, Aciklama = x.Aciklama, Sira = x.Sira }).ToList(),
                IlgiliUrunler = (a.IlgiliUrunler ?? new List<SahaDenetimAdimIlgiliUrun>())
                    .OrderBy(x => x.Sira).ThenBy(x => x.Id)
                    .Select(x => new IlgiliUrunDto { Id = x.Id, StokKodu = x.StokKodu, Adet = x.Adet, Aciklama = x.Aciklama, Sira = x.Sira }).ToList(),
                Fotograflar = (a.Fotograflar ?? new List<SahaDenetimAdimFoto>())
                    .OrderBy(x => x.Sira).ThenBy(x => x.Id)
                    .Select(x => new FotoDto { Id = x.Id, Url = x.DosyaYolu, DosyaAdi = x.DosyaAdi, Sira = x.Sira })
                    .ToList(),
            })
            .ToList();

        return Ok(new DenetimDetayDto
        {
            Id = denetim.Id,
            Ad = denetim.Ad,
            KayitTarihi = denetim.KayitTarihi,
            OlusturanAdSoyad = denetim.OlusturanKullanici.AdSoyad,
            CanDelete = denetim.OlusturanKullaniciId == userId,
            KapaliMi = denetim.KapaliMi,
            KapatanAdSoyad = denetim.KapatanKullanici?.AdSoyad,
            KapatmaTarihi = denetim.KapatmaTarihi,
            GeriAcanAdSoyad = denetim.GeriAcanKullanici?.AdSoyad,
            GeriAcmaTarihi = denetim.GeriAcmaTarihi,
            LokasyonId = denetim.LokasyonId,
            LokasyonAdi = denetim.LokasyonAdi,
            Adimlar = adimlar
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Guncelle(int id, [FromBody] DenetimUpdateDto dto, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        var genelYetkili = _yetki.GenelYetkiliMi();

        var entity = await _db.SahaDenetimler.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity == null) return NotFound();
        if (!genelYetkili && entity.OlusturanKullaniciId != userId) return Forbid();
        if (string.IsNullOrWhiteSpace(dto.Ad))
            return BadRequest(new { message = "Denetim adı zorunludur." });

        int? lokasyonId = null;
        if (!string.IsNullOrWhiteSpace(dto.LokasyonMagazaKodu) && int.TryParse(dto.LokasyonMagazaKodu.Trim(), out var lid))
            lokasyonId = lid;
        var lokasyonAdi = await ResolveLokasyonAdiAsync(lokasyonId, ct);

        entity.Ad = dto.Ad.Trim();
        entity.KayitTarihi = dto.KayitTarihi ?? entity.KayitTarihi;
        entity.LokasyonId = lokasyonId;
        entity.LokasyonAdi = lokasyonAdi;
        await _db.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Sil(int id, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        var entity = await _db.SahaDenetimler.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity == null) return NotFound();
        if (entity.OlusturanKullaniciId != userId) return Forbid();

        entity.IsDeleted = true;
        entity.DeleteDate = DateTime.Now;
        entity.DeletedByUserId = userId;
        await _db.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpPost("{id:int}/kapat")]
    public async Task<IActionResult> Kapat(int id, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        var genelYetkili = _yetki.GenelYetkiliMi();

        var entity = await _db.SahaDenetimler.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity == null) return NotFound();
        if (!genelYetkili && entity.OlusturanKullaniciId != userId) return Forbid();
        if (entity.KapaliMi) return Ok();

        entity.KapaliMi = true;
        entity.KapatanKullaniciId = userId;
        entity.KapatmaTarihi = DateTime.Now;
        await _db.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpPost("{id:int}/geri-ac")]
    public async Task<IActionResult> GeriAc(int id, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        var genelYetkili = _yetki.GenelYetkiliMi();

        var entity = await _db.SahaDenetimler.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity == null) return NotFound();
        if (!genelYetkili && entity.OlusturanKullaniciId != userId) return Forbid();
        if (!entity.KapaliMi) return Ok();

        entity.KapaliMi = false;
        entity.GeriAcanKullaniciId = userId;
        entity.GeriAcmaTarihi = DateTime.Now;
        await _db.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpPost("{id:int}/klonla")]
    public async Task<ActionResult<object>> Klonla(int id, [FromBody] DenetimKlonlaDto dto, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        var genelYetkili = _yetki.GenelYetkiliMi();
        if (dto == null || string.IsNullOrWhiteSpace(dto.Ad))
            return BadRequest(new { message = "Denetim adı zorunludur." });

        var kaynak = await _db.SahaDenetimler
            .AsNoTracking()
            .Include(x => x.Adimlar)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (kaynak == null) return NotFound();
        if (!genelYetkili && kaynak.OlusturanKullaniciId != userId) return Forbid();

        var yeni = new SahaDenetim
        {
            Ad = dto.Ad.Trim(),
            KayitTarihi = DateTime.Now,
            OlusturanKullaniciId = userId,
            KapaliMi = false
        };
        _db.SahaDenetimler.Add(yeni);
        await _db.SaveChangesAsync(ct);

        var adimlar = (kaynak.Adimlar ?? new List<SahaDenetimAdim>())
            .OrderBy(a => a.Sira)
            .ThenBy(a => a.Id)
            .ToList();

        var sira = 1;
        foreach (var a in adimlar)
        {
            _db.SahaDenetimAdimlar.Add(new SahaDenetimAdim
            {
                SahaDenetimId = yeni.Id,
                Ad = a.Ad,
                Aciklama = a.Aciklama,
                SahaDenetimKategoriId = a.SahaDenetimKategoriId,
                Tipler = a.Tipler,
                Sira = sira++,
                YapildiMi = false,
                Puan = null,
                PersonelYorum = null,
                Not = null
            });
        }

        await _db.SaveChangesAsync(ct);
        return Ok(new { id = yeni.Id });
    }

    [HttpPost("{denetimId:int}/adim")]
    public async Task<ActionResult<object>> AdimEkle(int denetimId, [FromBody] AdimCreateDto dto, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        var genelYetkili = _yetki.GenelYetkiliMi();
        var denetim = await _db.SahaDenetimler.AsNoTracking().FirstOrDefaultAsync(x => x.Id == denetimId, ct);
        if (denetim == null) return NotFound();
        if (!genelYetkili && denetim.OlusturanKullaniciId != userId) return Forbid();
        if (string.IsNullOrWhiteSpace(dto.Ad))
            return BadRequest(new { message = "Adım adı zorunludur." });

        var maxSira = await _db.SahaDenetimAdimlar.Where(x => x.SahaDenetimId == denetimId).Select(x => (int?)x.Sira).MaxAsync(ct) ?? 0;
        var entity = new SahaDenetimAdim
        {
            SahaDenetimId = denetimId,
            Ad = dto.Ad.Trim(),
            Aciklama = string.IsNullOrWhiteSpace(dto.Aciklama) ? null : dto.Aciklama.Trim(),
            SahaDenetimKategoriId = dto.KategoriId,
            Tipler = dto.Tipler,
            Sira = dto.Sira > 0 ? dto.Sira : maxSira + 1
        };
        _db.SahaDenetimAdimlar.Add(entity);
        await _db.SaveChangesAsync(ct);
        return Ok(new { id = entity.Id });
    }

    [HttpPut("adim/{id:int}")]
    public async Task<IActionResult> AdimGuncelle(int id, [FromBody] AdimUpdateDto dto, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        var genelYetkili = _yetki.GenelYetkiliMi();

        var entity = await _db.SahaDenetimAdimlar.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity == null) return NotFound();
        var denetim = await _db.SahaDenetimler.AsNoTracking().FirstOrDefaultAsync(x => x.Id == entity.SahaDenetimId, ct);
        if (denetim == null) return NotFound();
        if (!genelYetkili && denetim.OlusturanKullaniciId != userId) return Forbid();
        if (string.IsNullOrWhiteSpace(dto.Ad))
            return BadRequest(new { message = "Adım adı zorunludur." });

        entity.Ad = dto.Ad.Trim();
        entity.Aciklama = string.IsNullOrWhiteSpace(dto.Aciklama) ? null : dto.Aciklama.Trim();
        entity.SahaDenetimKategoriId = dto.KategoriId;
        entity.Tipler = dto.Tipler;
        entity.Sira = dto.Sira;
        await _db.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpDelete("adim/{id:int}")]
    public async Task<IActionResult> AdimSil(int id, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        var genelYetkili = _yetki.GenelYetkiliMi();

        var entity = await _db.SahaDenetimAdimlar.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity == null) return NotFound();
        var denetim = await _db.SahaDenetimler.AsNoTracking().FirstOrDefaultAsync(x => x.Id == entity.SahaDenetimId, ct);
        if (denetim == null) return NotFound();
        if (!genelYetkili && denetim.OlusturanKullaniciId != userId) return Forbid();

        entity.IsDeleted = true;
        entity.DeleteDate = DateTime.Now;
        entity.DeletedByUserId = userId;
        await _db.SaveChangesAsync(ct);
        return Ok();
    }

    // Personel doldurma
    [HttpPut("adim/{id:int}/doldur")]
    public async Task<IActionResult> AdimDoldur(int id, [FromBody] AdimDoldurDto dto, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        var genelYetkili = _yetki.GenelYetkiliMi();

        var entity = await _db.SahaDenetimAdimlar
            .Include(x => x.IlgiliKisiler)
            .Include(x => x.IlgiliUrunler)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity == null) return NotFound();
        var denetim = await _db.SahaDenetimler.AsNoTracking().FirstOrDefaultAsync(x => x.Id == entity.SahaDenetimId, ct);
        if (denetim == null) return NotFound();
        if (!genelYetkili && denetim.OlusturanKullaniciId != userId) return Forbid();

        entity.YapildiMi = dto.YapildiMi;
        entity.Puan = dto.Puan;
        entity.PersonelYorum = string.IsNullOrWhiteSpace(dto.PersonelYorum) ? null : dto.PersonelYorum.Trim();
        entity.Not = string.IsNullOrWhiteSpace(dto.Not) ? null : dto.Not.Trim();

        // kişiler
        _db.SahaDenetimAdimIlgiliKisiler.RemoveRange(entity.IlgiliKisiler ?? new List<SahaDenetimAdimIlgiliKisi>());
        if (dto.IlgiliKisiler != null)
        {
            var sira = 1;
            foreach (var k in dto.IlgiliKisiler.Where(x => x != null))
            {
                var adSoyad = (k!.AdSoyad ?? "").Trim();
                if (string.IsNullOrWhiteSpace(adSoyad)) continue;
                var aciklama = string.IsNullOrWhiteSpace(k.Aciklama) ? null : k.Aciklama.Trim();
                _db.SahaDenetimAdimIlgiliKisiler.Add(new SahaDenetimAdimIlgiliKisi
                {
                    SahaDenetimAdimId = id,
                    AdSoyad = adSoyad,
                    Aciklama = aciklama,
                    Sira = sira++
                });
            }
        }

        // ürünler
        _db.SahaDenetimAdimIlgiliUrunler.RemoveRange(entity.IlgiliUrunler ?? new List<SahaDenetimAdimIlgiliUrun>());
        if (dto.IlgiliUrunler != null)
        {
            var sira = 1;
            foreach (var u in dto.IlgiliUrunler.Where(x => x != null))
            {
                var stok = (u!.StokKodu ?? "").Trim();
                if (string.IsNullOrWhiteSpace(stok)) continue;
                var aciklama = string.IsNullOrWhiteSpace(u.Aciklama) ? null : u.Aciklama.Trim();
                _db.SahaDenetimAdimIlgiliUrunler.Add(new SahaDenetimAdimIlgiliUrun
                {
                    SahaDenetimAdimId = id,
                    StokKodu = stok,
                    Adet = u.Adet,
                    Aciklama = aciklama,
                    Sira = sira++
                });
            }
        }

        await _db.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpPost("adim/{id:int}/foto")]
    public async Task<ActionResult<object>> AdimFotoUpload(int id, List<IFormFile> files, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        var genelYetkili = _yetki.GenelYetkiliMi();

        var adim = await _db.SahaDenetimAdimlar
            .Include(x => x.Fotograflar)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (adim == null) return NotFound();
        var denetim = await _db.SahaDenetimler.AsNoTracking().FirstOrDefaultAsync(x => x.Id == adim.SahaDenetimId, ct);
        if (denetim == null) return NotFound();
        if (denetim.KapaliMi) return BadRequest(new { message = "Denetim kapalı olduğu için fotoğraf eklenemez." });
        if (!genelYetkili && denetim.OlusturanKullaniciId != userId) return Forbid();

        if (files == null || files.Count == 0) return BadRequest(new { message = "Dosya seçilmedi." });

        var uploads = Path.Combine(_env.WebRootPath, "uploads", "saha-denetim", "adim", id.ToString());
        Directory.CreateDirectory(uploads);
        var maxSira = (adim.Fotograflar ?? new List<SahaDenetimAdimFoto>()).Select(x => (int?)x.Sira).Max() ?? 0;

        foreach (var file in files)
        {
            if (file == null || file.Length == 0) continue;
            var safeName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var path = Path.Combine(uploads, safeName);
            await using (var stream = System.IO.File.Create(path))
                await file.CopyToAsync(stream, ct);
            var relPath = $"/uploads/saha-denetim/adim/{id}/{safeName}";
            _db.SahaDenetimAdimFotolar.Add(new SahaDenetimAdimFoto
            {
                SahaDenetimAdimId = id,
                DosyaYolu = relPath,
                DosyaAdi = file.FileName,
                Sira = ++maxSira
            });
        }

        await _db.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpDelete("adim/foto/{fotoId:int}")]
    public async Task<IActionResult> AdimFotoSil(int fotoId, CancellationToken ct)
    {
        var userId = _currentUser.GetCurrentUserId();
        var genelYetkili = _yetki.GenelYetkiliMi();

        var foto = await _db.SahaDenetimAdimFotolar.FirstOrDefaultAsync(x => x.Id == fotoId, ct);
        if (foto == null) return NotFound();
        var adim = await _db.SahaDenetimAdimlar.AsNoTracking().FirstOrDefaultAsync(x => x.Id == foto.SahaDenetimAdimId, ct);
        if (adim == null) return NotFound();
        var denetim = await _db.SahaDenetimler.FirstOrDefaultAsync(x => x.Id == adim.SahaDenetimId, ct);
        if (denetim == null) return NotFound();
        if (denetim.KapaliMi) return BadRequest(new { message = "Denetim kapalı olduğu için fotoğraf silinemez." });
        if (!genelYetkili && denetim.OlusturanKullaniciId != userId) return Forbid();

        foto.IsDeleted = true;
        foto.DeleteDate = DateTime.Now;
        foto.DeletedByUserId = userId;
        await _db.SaveChangesAsync(ct);
        return Ok();
    }

    public class DenetimListDto
    {
        public int Id { get; set; }
        public string Ad { get; set; } = "";
        public DateTime KayitTarihi { get; set; }
        public string? OlusturanAdSoyad { get; set; }
        public bool KapaliMi { get; set; }
        public int? LokasyonId { get; set; }
        public string? LokasyonAdi { get; set; }
        public int AdimSayisi { get; set; }
    }

    public class DenetimCreateDto
    {
        public string? Ad { get; set; }
        public DateTime? KayitTarihi { get; set; }
        public string? LokasyonMagazaKodu { get; set; }
    }

    public class DenetimUpdateDto
    {
        public string? Ad { get; set; }
        public DateTime? KayitTarihi { get; set; }
        public string? LokasyonMagazaKodu { get; set; }
    }

    public class DenetimDetayDto
    {
        public int Id { get; set; }
        public string Ad { get; set; } = "";
        public DateTime KayitTarihi { get; set; }
        public string? OlusturanAdSoyad { get; set; }
        public bool CanDelete { get; set; }
        public bool KapaliMi { get; set; }
        public string? KapatanAdSoyad { get; set; }
        public DateTime? KapatmaTarihi { get; set; }
        public string? GeriAcanAdSoyad { get; set; }
        public DateTime? GeriAcmaTarihi { get; set; }
        public int? LokasyonId { get; set; }
        public string? LokasyonAdi { get; set; }
        public List<DenetimAdimDto> Adimlar { get; set; } = new();
    }

    public class LokasyonDto
    {
        public string LokasyonAdi { get; set; } = "";
        public string LokasyonMagazaKodu { get; set; } = "";
    }

    private class LokasyonApiDto
    {
        public string? LokasyonAdi { get; set; }
        public string? LokasyonMagazaKodu { get; set; }
    }

    public class DenetimKlonlaDto
    {
        public string? Ad { get; set; }
    }

    public class DenetimAdimDto
    {
        public int Id { get; set; }
        public string Ad { get; set; } = "";
        public string? Aciklama { get; set; }
        public int? KategoriId { get; set; }
        public string? KategoriAd { get; set; }
        public int Tipler { get; set; }
        public int Sira { get; set; }
        public bool YapildiMi { get; set; }
        public int? Puan { get; set; }
        public string? PersonelYorum { get; set; }
        public string? Not { get; set; }
        public List<IlgiliKisiDto> IlgiliKisiler { get; set; } = new();
        public List<IlgiliUrunDto> IlgiliUrunler { get; set; } = new();
        public List<FotoDto> Fotograflar { get; set; } = new();
    }

    public class FotoDto
    {
        public int Id { get; set; }
        public string Url { get; set; } = "";
        public string? DosyaAdi { get; set; }
        public int Sira { get; set; }
    }

    public class IlgiliKisiDto
    {
        public int Id { get; set; }
        public string AdSoyad { get; set; } = "";
        public string? Aciklama { get; set; }
        public int Sira { get; set; }
    }

    public class IlgiliUrunDto
    {
        public int Id { get; set; }
        public string StokKodu { get; set; } = "";
        public decimal Adet { get; set; }
        public string? Aciklama { get; set; }
        public int Sira { get; set; }
    }

    public class AdimCreateDto
    {
        public string? Ad { get; set; }
        public string? Aciklama { get; set; }
        public int? KategoriId { get; set; }
        public int Tipler { get; set; }
        public int Sira { get; set; }
    }

    public class AdimUpdateDto
    {
        public string? Ad { get; set; }
        public string? Aciklama { get; set; }
        public int? KategoriId { get; set; }
        public int Tipler { get; set; }
        public int Sira { get; set; }
    }

    public class AdimDoldurDto
    {
        public bool YapildiMi { get; set; }
        public int? Puan { get; set; }
        public string? PersonelYorum { get; set; }
        public string? Not { get; set; }
        public List<IlgiliKisiCreateDto?>? IlgiliKisiler { get; set; }
        public List<IlgiliUrunCreateDto?>? IlgiliUrunler { get; set; }
    }

    public class IlgiliKisiCreateDto
    {
        public string? AdSoyad { get; set; }
        public string? Aciklama { get; set; }
    }

    public class IlgiliUrunCreateDto
    {
        public string? StokKodu { get; set; }
        public decimal Adet { get; set; }
        public string? Aciklama { get; set; }
    }
}

