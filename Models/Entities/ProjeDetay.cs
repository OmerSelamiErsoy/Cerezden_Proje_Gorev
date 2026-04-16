namespace ProjeGorevYonetimi.Models.Entities;

public class ProjeDetay : BaseEntity
{
    public int Id { get; set; }
    public int ProjeId { get; set; }
    public int? KategoriId { get; set; }
    public string AdimAdi { get; set; } = string.Empty;
    public int DurumId { get; set; }
    public int? SorumluKullaniciId { get; set; }
    public string? Aciklama { get; set; }
    public decimal? Adet { get; set; }
    public int? AdetBirimiId { get; set; }
    public decimal? MaliyetFiyati { get; set; }
    public decimal? CikisFiyati { get; set; }
    public int Sira { get; set; }

    public virtual Proje Proje { get; set; } = null!;
    public virtual ProjeDetayKategori? Kategori { get; set; }
    public virtual Durum Durum { get; set; } = null!;
    public virtual Kullanici? SorumluKullanici { get; set; }
    public virtual AdetBirimi? AdetBirimi { get; set; }
    public virtual ICollection<ProjeDetayYorum>? Yorumlar { get; set; }
}
