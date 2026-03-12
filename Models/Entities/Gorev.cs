namespace ProjeGorevYonetimi.Models.Entities;

/// <summary>Görev türü: 0 = Kendime, 1 = Kişilere</summary>
public class Gorev : BaseEntity
{
    public int Id { get; set; }
    public int? GorevGrubuId { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public int Turu { get; set; }
    public int OlusturanKullaniciId { get; set; }
    public DateTime OlusturmaTarihi { get; set; }
    public int DurumId { get; set; }
    public string? Renk { get; set; }

    public virtual GorevGrubu? GorevGrubu { get; set; }
    public virtual Kullanici OlusturanKullanici { get; set; } = null!;
    public virtual Durum Durum { get; set; } = null!;
    public virtual ICollection<GorevAtama>? Atamalar { get; set; }
    public virtual ICollection<GorevYorum>? Yorumlar { get; set; }
}
