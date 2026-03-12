namespace ProjeGorevYonetimi.Models.Entities;

public class Proje : BaseEntity
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public int ProjeDurumId { get; set; }
    public string? Aciklama { get; set; }
    public DateTime? BaslangicTarihi { get; set; }
    public DateTime? PlanlananBitisTarihi { get; set; }
    public DateTime? BitisTarihi { get; set; }
    public int? SorumluKullaniciId { get; set; }
    public bool AktifMi { get; set; }
    /// <summary>0 = Genel (tüm kullanıcılar), 1 = Kişi bazlı</summary>
    public int YetkiTipi { get; set; }
    public int? OlusturanKullaniciId { get; set; }

    public virtual Durum ProjeDurum { get; set; } = null!;
    public virtual Kullanici? SorumluKullanici { get; set; }
    public virtual Kullanici? OlusturanKullanici { get; set; }
    public virtual ICollection<ProjeYetkiKullanici>? YetkiKullanicilar { get; set; }
    public virtual ICollection<ProjeDetay>? Detaylar { get; set; }
}
