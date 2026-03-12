namespace ProjeGorevYonetimi.Models.Entities;

/// <summary>
/// Parametrik proje detay kategorisi.
/// </summary>
public class ProjeDetayKategori : BaseEntity
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public int Sira { get; set; }

    public virtual ICollection<ProjeDetay>? ProjeDetaylar { get; set; }
    public virtual ICollection<ProjeSablonDetay>? SablonDetaylar { get; set; }
}
