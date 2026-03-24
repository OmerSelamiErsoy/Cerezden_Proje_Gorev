namespace ProjeGorevYonetimi.Models.Entities;

/// <summary>
/// Proje adım (ProjeDetay) durum değişiklik geçmişi.
/// </summary>
public class ProjeDetayDurumLog
{
    public int Id { get; set; }

    public int ProjeDetayId { get; set; }
    public int? EskiDurumId { get; set; }
    public int YeniDurumId { get; set; }

    public int DegistirenKullaniciId { get; set; }
    public DateTime DegistirmeTarihi { get; set; }

    public virtual ProjeDetay ProjeDetay { get; set; } = null!;
    public virtual Durum? EskiDurum { get; set; }
    public virtual Durum YeniDurum { get; set; } = null!;
    public virtual Kullanici DegistirenKullanici { get; set; } = null!;
}

