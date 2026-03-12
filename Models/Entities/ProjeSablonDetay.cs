namespace ProjeGorevYonetimi.Models.Entities;

public class ProjeSablonDetay : BaseEntity
{
    public int Id { get; set; }
    public int ProjeSablonId { get; set; }
    public int? KategoriId { get; set; }
    public string AdimAdi { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public int Sira { get; set; }

    public virtual ProjeSablon ProjeSablon { get; set; } = null!;
    public virtual ProjeDetayKategori? Kategori { get; set; }
}
