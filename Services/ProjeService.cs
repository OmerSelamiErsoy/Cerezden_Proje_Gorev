using Microsoft.EntityFrameworkCore;
using ProjeGorevYonetimi.Data;
using ProjeGorevYonetimi.Models.Entities;

namespace ProjeGorevYonetimi.Services;

public class ProjeService : IProjeService
{
    private readonly ApplicationDbContext _db;
    private readonly IYetkiService _yetki;
    private readonly ICurrentUserService _currentUser;

    public ProjeService(ApplicationDbContext db, IYetkiService yetki, ICurrentUserService currentUser)
    {
        _db = db;
        _yetki = yetki;
        _currentUser = currentUser;
    }

    public async Task<List<ProjeListDto>> GetVisibleProjelerAsync(CancellationToken ct = default)
    {
        var genelYetkili = _yetki.GenelYetkiliMi();
        var userId = _currentUser.GetCurrentUserId();
        var query = _db.Projeler
            .AsNoTracking()
            .Include(p => p.ProjeDurum)
            .Include(p => p.SorumluKullanici)
            .Include(p => p.Detaylar);
        List<Proje> list;
        if (genelYetkili)
            list = await query.OrderByDescending(p => p.Id).ToListAsync(ct);
        else
        {
            var kisiProjeler = await _db.ProjeYetkiKullanicilar
                .Where(y => y.KullaniciId == userId)
                .Select(y => y.ProjeId)
                .ToListAsync(ct);
            list = await query
                .Where(p => p.YetkiTipi == 0 || kisiProjeler.Contains(p.Id))
                .OrderByDescending(p => p.Id)
                .ToListAsync(ct);
        }
        var result = new List<ProjeListDto>();
        foreach (var p in list)
        {
            var toplam = p.Detaylar?.Count ?? 0;
            var tamamlanan = p.Detaylar?.Count(d => d.DurumId == 4 || d.DurumId == 5) ?? 0; // İptal=4, Tamamlandı=5
            var yuzde = toplam == 0 ? 0 : (int)Math.Round(100.0 * tamamlanan / toplam);
            result.Add(new ProjeListDto
            {
                Id = p.Id,
                Ad = p.Ad,
                ProjeDurumAd = p.ProjeDurum?.Ad ?? "",
                ProgressYuzde = yuzde,
                AktifMi = p.AktifMi,
                PlanlananBitisTarihi = p.PlanlananBitisTarihi,
                SorumluAdSoyad = p.SorumluKullanici?.AdSoyad,
                SilinebilirMi = genelYetkili || p.OlusturanKullaniciId == userId
            });
        }
        return result;
    }

    public async Task<ProjeDetayDto?> GetProjeDetayAsync(int id, CancellationToken ct = default)
    {
        if (!await _yetki.ProjeGorebilirMiAsync(id, ct)) return null;
        var p = await _db.Projeler
            .AsNoTracking()
            .Include(x => x.ProjeDurum)
            .Include(x => x.SorumluKullanici)
            .Include(x => x.OlusturanKullanici)
            .Include(x => x.Detaylar!)
                .ThenInclude(d => d.Durum)
            .Include(x => x.Detaylar!)
                .ThenInclude(d => d.Kategori)
            .Include(x => x.Detaylar!)
                .ThenInclude(d => d.AdetBirimi)
            .Include(x => x.Detaylar!)
                .ThenInclude(d => d.SorumluKullanici)
            .Include(x => x.Detaylar!)
                .ThenInclude(d => d.Yorumlar)
            .Include(x => x.YetkiKullanicilar)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p == null) return null;
        var userId = _currentUser.GetCurrentUserId();
        var genelYetkili = _yetki.GenelYetkiliMi();
        var toplam = p.Detaylar?.Count ?? 0;
        var tamamlanan = p.Detaylar?.Count(d => d.DurumId == 4 || d.DurumId == 5) ?? 0;
        var yuzde = toplam == 0 ? 0 : (int)Math.Round(100.0 * tamamlanan / toplam);
        return new ProjeDetayDto
        {
            Id = p.Id,
            Ad = p.Ad,
            ProjeDurumId = p.ProjeDurumId,
            ProjeDurumAd = p.ProjeDurum?.Ad ?? "",
            Aciklama = p.Aciklama,
            BaslangicTarihi = p.BaslangicTarihi,
            PlanlananBitisTarihi = p.PlanlananBitisTarihi,
            BitisTarihi = p.BitisTarihi,
            SorumluKullaniciId = p.SorumluKullaniciId,
            SorumluAdSoyad = p.SorumluKullanici?.AdSoyad,
            OlusturanKullaniciId = p.OlusturanKullaniciId,
            OlusturanAdSoyad = p.OlusturanKullanici?.AdSoyad,
            AktifMi = p.AktifMi,
            ProgressYuzde = yuzde,
            MeKullaniciId = userId,
            MeGenelYetkiliMi = genelYetkili,
            YetkiTipi = p.YetkiTipi,
            YetkiKullaniciIds = p.YetkiKullanicilar?.Select(y => y.KullaniciId).ToList() ?? new List<int>(),
            Detaylar = (p.Detaylar ?? Array.Empty<ProjeDetay>())
                .OrderBy(d => d.Sira)
                .Select(d => new ProjeDetayItemDto
                {
                    Id = d.Id,
                    KategoriId = d.KategoriId,
                    KategoriAd = d.Kategori?.Ad,
                    AdimAdi = d.AdimAdi,
                    DurumId = d.DurumId,
                    DurumAd = d.Durum?.Ad ?? "",
                    SorumluKullaniciId = d.SorumluKullaniciId,
                    SorumluAdSoyad = d.SorumluKullanici?.AdSoyad,
                    Aciklama = d.Aciklama,
                    Adet = d.Adet,
                    AdetBirimiId = d.AdetBirimiId,
                    AdetBirimiAd = d.AdetBirimi != null ? d.AdetBirimi.Ad : null,
                    MaliyetFiyati = d.MaliyetFiyati,
                    CikisFiyati = d.CikisFiyati,
                    Sira = d.Sira,
                    YorumSayisi = d.Yorumlar?.Count ?? 0,
                    InsertedByUserId = d.InsertedByUserId
                }).ToList()
        };
    }

    public async Task<Proje?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _db.Projeler.Include(p => p.Detaylar).FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<int> ProgressYuzdeAsync(int projeId, CancellationToken ct = default)
    {
        var detaylar = await _db.ProjeDetaylar.Where(d => d.ProjeId == projeId).ToListAsync(ct);
        var toplam = detaylar.Count;
        if (toplam == 0) return 0;
        var tamamlanan = detaylar.Count(d => d.DurumId == 4 || d.DurumId == 5);
        return (int)Math.Round(100.0 * tamamlanan / toplam);
    }

    public async Task<int> CreateAsync(ProjeCreateDto dto, int olusturanUserId, CancellationToken ct = default)
    {
        var islemBekliyorDurumId = await _db.Durumlar.Where(x => x.Ad == "İşlem Bekliyor").Select(x => x.Id).FirstOrDefaultAsync(ct);
        if (islemBekliyorDurumId == 0) islemBekliyorDurumId = 1;
        var beklemedeDurumId = await _db.Durumlar.Where(x => x.Ad == "Beklemede").Select(x => x.Id).FirstOrDefaultAsync(ct);
        if (beklemedeDurumId == 0) beklemedeDurumId = 2;

        var proje = new Proje
        {
            Ad = dto.Ad,
            ProjeDurumId = islemBekliyorDurumId,
            Aciklama = dto.Aciklama,
            BaslangicTarihi = dto.BaslangicTarihi,
            PlanlananBitisTarihi = dto.PlanlananBitisTarihi,
            SorumluKullaniciId = dto.SorumluKullaniciId,
            AktifMi = dto.AktifMi,
            YetkiTipi = dto.YetkiTipi,
            OlusturanKullaniciId = olusturanUserId
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

        if (dto.Detaylar != null)
        {
            var sira = 0;
            foreach (var item in dto.Detaylar)
            {
                _db.ProjeDetaylar.Add(new ProjeDetay
                {
                    ProjeId = proje.Id,
                    KategoriId = item.KategoriId,
                    AdimAdi = item.AdimAdi,
                    DurumId = beklemedeDurumId,
                    Aciklama = item.Aciklama,
                    Adet = item.Adet,
                    AdetBirimiId = item.AdetBirimiId,
                    MaliyetFiyati = item.MaliyetFiyati,
                    CikisFiyati = item.CikisFiyati,
                    Sira = ++sira
                });
            }
            await _db.SaveChangesAsync(ct);
        }

        return proje.Id;
    }

    public async Task UpdateAsync(int id, ProjeUpdateDto dto, CancellationToken ct = default)
    {
        var proje = await _db.Projeler.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (proje == null) return;
        proje.Ad = dto.Ad;
        proje.ProjeDurumId = dto.ProjeDurumId;
        proje.Aciklama = dto.Aciklama;
        proje.BaslangicTarihi = dto.BaslangicTarihi;
        proje.PlanlananBitisTarihi = dto.PlanlananBitisTarihi;
        proje.BitisTarihi = dto.BitisTarihi;
        proje.SorumluKullaniciId = dto.SorumluKullaniciId;
        proje.AktifMi = dto.AktifMi;
        proje.YetkiTipi = dto.YetkiTipi;

        var mevcutYetkiler = await _db.ProjeYetkiKullanicilar.Where(y => y.ProjeId == id).ToListAsync(ct);
        _db.ProjeYetkiKullanicilar.RemoveRange(mevcutYetkiler);
        if (dto.YetkiTipi == 1 && dto.YetkiKullaniciIds != null && dto.YetkiKullaniciIds.Count > 0)
        {
            foreach (var kid in dto.YetkiKullaniciIds)
                _db.ProjeYetkiKullanicilar.Add(new ProjeYetkiKullanici { ProjeId = id, KullaniciId = kid });
        }
        await _db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(int id, int userId, CancellationToken ct = default)
    {
        var proje = await _db.Projeler.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (proje == null) return;
        proje.IsDeleted = true;
        proje.DeleteDate = DateTime.UtcNow;
        proje.DeletedByUserId = userId;
        await _db.SaveChangesAsync(ct);
    }
}
