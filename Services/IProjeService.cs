using ProjeGorevYonetimi.Models.Entities;

namespace ProjeGorevYonetimi.Services;

public interface IProjeService
{
    Task<List<ProjeListDto>> GetVisibleProjelerAsync(CancellationToken ct = default);
    Task<ProjeDetayDto?> GetProjeDetayAsync(int id, CancellationToken ct = default);
    Task<Proje?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<int> CreateAsync(ProjeCreateDto dto, int olusturanUserId, CancellationToken ct = default);
    Task UpdateAsync(int id, ProjeUpdateDto dto, CancellationToken ct = default);
    Task SoftDeleteAsync(int id, int userId, CancellationToken ct = default);
    Task<int> ProgressYuzdeAsync(int projeId, CancellationToken ct = default);
}

public class ProjeListDto
{
    public int Id { get; set; }
    public string Ad { get; set; } = "";
    public string ProjeDurumAd { get; set; } = "";
    public int ProgressYuzde { get; set; }
    public bool AktifMi { get; set; }
    public DateTime? PlanlananBitisTarihi { get; set; }
    public string? SorumluAdSoyad { get; set; }
    /// <summary>Genel yetkili veya projeyi oluşturan kullanıcı silebilir.</summary>
    public bool SilinebilirMi { get; set; }
}

public class ProjeDetayDto
{
    public int Id { get; set; }
    public string Ad { get; set; } = "";
    public int ProjeDurumId { get; set; }
    public string ProjeDurumAd { get; set; } = "";
    public string? Aciklama { get; set; }
    public DateTime? BaslangicTarihi { get; set; }
    public DateTime? PlanlananBitisTarihi { get; set; }
    public DateTime? BitisTarihi { get; set; }
    public int? SorumluKullaniciId { get; set; }
    public string? SorumluAdSoyad { get; set; }
    public int? OlusturanKullaniciId { get; set; }
    public string? OlusturanAdSoyad { get; set; }
    public bool AktifMi { get; set; }
    public int ProgressYuzde { get; set; }
    /// <summary>Oturum açan kullanıcının Kullanici.Id değeri.</summary>
    public int? MeKullaniciId { get; set; }
    /// <summary>Oturum açan kullanıcı GeneralAuthority listesinde mi (genel yetkili).</summary>
    public bool MeGenelYetkiliMi { get; set; }
    public int YetkiTipi { get; set; }
    public List<int> YetkiKullaniciIds { get; set; } = new();
    public List<ProjeDetayItemDto> Detaylar { get; set; } = new();
}

public class ProjeDetayItemDto
{
    public int Id { get; set; }
    public int? KategoriId { get; set; }
    public string? KategoriAd { get; set; }
    public string AdimAdi { get; set; } = "";
    public int DurumId { get; set; }
    public string DurumAd { get; set; } = "";
    public int? SorumluKullaniciId { get; set; }
    public string? SorumluAdSoyad { get; set; }
    public string? Aciklama { get; set; }
    public decimal? Adet { get; set; }
    public int? AdetBirimiId { get; set; }
    public string? AdetBirimiAd { get; set; }
    public decimal? MaliyetFiyati { get; set; }
    public decimal? CikisFiyati { get; set; }
    public int Sira { get; set; }
    public int YorumSayisi { get; set; }
    /// <summary>Adımı ekleyen kullanıcının Kullanici.Id değeri.</summary>
    public int? InsertedByUserId { get; set; }
}

public class ProjeCreateDto
{
    public string Ad { get; set; } = "";
    public string? Aciklama { get; set; }
    public DateTime? BaslangicTarihi { get; set; }
    public DateTime? PlanlananBitisTarihi { get; set; }
    public int? SorumluKullaniciId { get; set; }
    public bool AktifMi { get; set; } = true;
    public int YetkiTipi { get; set; }
    public List<int>? YetkiKullaniciIds { get; set; }
    public List<ProjeDetayCreateItemDto>? Detaylar { get; set; }
}

public class ProjeDetayCreateItemDto
{
    public int? KategoriId { get; set; }
    public string AdimAdi { get; set; } = "";
    public string? Aciklama { get; set; }
    public decimal? Adet { get; set; }
    public int? AdetBirimiId { get; set; }
    public decimal? MaliyetFiyati { get; set; }
    public decimal? CikisFiyati { get; set; }
    public int Sira { get; set; }
}

public class ProjeUpdateDto
{
    public string Ad { get; set; } = "";
    public int ProjeDurumId { get; set; }
    public string? Aciklama { get; set; }
    public DateTime? BaslangicTarihi { get; set; }
    public DateTime? PlanlananBitisTarihi { get; set; }
    public DateTime? BitisTarihi { get; set; }
    public int? SorumluKullaniciId { get; set; }
    public bool AktifMi { get; set; }
    public int YetkiTipi { get; set; }
    public List<int>? YetkiKullaniciIds { get; set; }
}
