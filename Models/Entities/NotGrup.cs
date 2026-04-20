namespace ProjeGorevYonetimi.Models.Entities;

public class NotGrup : BaseEntity
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public int Sira { get; set; }
    /// <summary>0 = Sadece Ben, 1 = Kişi bazlı</summary>
    public int YetkiTipi { get; set; }
    public int OlusturanKullaniciId { get; set; }

    public virtual Kullanici OlusturanKullanici { get; set; } = null!;
    public virtual ICollection<NotGrupYetkiKullanici>? YetkiKullanicilar { get; set; }
    public virtual ICollection<Not>? Notlar { get; set; }
}

