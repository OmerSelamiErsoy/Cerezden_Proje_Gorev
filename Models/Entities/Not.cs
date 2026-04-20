namespace ProjeGorevYonetimi.Models.Entities;

public class Not : BaseEntity
{
    public int Id { get; set; }
    public int NotGrupId { get; set; }
    public string Baslik { get; set; } = string.Empty;
    public string IcerikHtml { get; set; } = string.Empty;
    public int Sira { get; set; }
    /// <summary>0 = Sadece Ben, 1 = Kişi bazlı</summary>
    public int YetkiTipi { get; set; }
    public int OlusturanKullaniciId { get; set; }

    public virtual NotGrup NotGrup { get; set; } = null!;
    public virtual Kullanici OlusturanKullanici { get; set; } = null!;
    public virtual ICollection<NotDosya>? Dosyalar { get; set; }
    public virtual ICollection<NotYetkiKullanici>? YetkiKullanicilar { get; set; }
}

