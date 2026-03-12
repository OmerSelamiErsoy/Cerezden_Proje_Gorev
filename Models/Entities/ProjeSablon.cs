namespace ProjeGorevYonetimi.Models.Entities;

public class ProjeSablon : BaseEntity
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public int? OlusturanKullaniciId { get; set; }

    public virtual Kullanici? OlusturanKullanici { get; set; }
    public virtual ICollection<ProjeSablonDetay>? Detaylar { get; set; }
}
