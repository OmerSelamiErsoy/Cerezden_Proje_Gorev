namespace ProjeGorevYonetimi.Models.Entities;

/// <summary>
/// Parametrik durum tablosu (Proje ve Detay adım durumları): İşlem Bekliyor, Beklemede, İşleme Alındı, İptal Edildi, Tamamlandı
/// </summary>
public class Durum : BaseEntity
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public int Sira { get; set; }

    public virtual ICollection<Proje>? Projeler { get; set; }
    public virtual ICollection<ProjeDetay>? ProjeDetaylar { get; set; }
    public virtual ICollection<Gorev>? Gorevler { get; set; }
}
