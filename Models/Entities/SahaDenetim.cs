namespace ProjeGorevYonetimi.Models.Entities;

public class SahaDenetim : BaseEntity
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public DateTime KayitTarihi { get; set; }
    public int OlusturanKullaniciId { get; set; }
    public int? LokasyonId { get; set; } // LokasyonMagazaKodu int olarak
    public string? LokasyonAdi { get; set; }
    public bool KapaliMi { get; set; }

    public int? KapatanKullaniciId { get; set; }
    public DateTime? KapatmaTarihi { get; set; }
    public int? GeriAcanKullaniciId { get; set; }
    public DateTime? GeriAcmaTarihi { get; set; }

    public virtual Kullanici OlusturanKullanici { get; set; } = null!;
    public virtual Kullanici? KapatanKullanici { get; set; }
    public virtual Kullanici? GeriAcanKullanici { get; set; }
    public virtual ICollection<SahaDenetimAdim>? Adimlar { get; set; }
}

