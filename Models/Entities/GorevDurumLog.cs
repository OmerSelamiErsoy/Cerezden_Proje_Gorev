namespace ProjeGorevYonetimi.Models.Entities;

/// <summary>Görev durum değişiklik geçmişi.</summary>
public class GorevDurumLog
{
    public int Id { get; set; }
    public int GorevId { get; set; }
    public int? EskiDurumId { get; set; }
    public int YeniDurumId { get; set; }
    public int DegistirenKullaniciId { get; set; }
    public DateTime DegistirmeTarihi { get; set; }

    public virtual Gorev Gorev { get; set; } = null!;
    public virtual Kullanici DegistirenKullanici { get; set; } = null!;
    public virtual Durum? EskiDurum { get; set; }
    public virtual Durum YeniDurum { get; set; } = null!;
}
