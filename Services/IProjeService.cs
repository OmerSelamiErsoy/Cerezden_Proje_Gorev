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
    public bool AktifMi { get; set; }
    public int ProgressYuzde { get; set; }
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
    public int Sira { get; set; }
    public int YorumSayisi { get; set; }
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
}
