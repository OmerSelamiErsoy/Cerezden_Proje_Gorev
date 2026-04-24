namespace ProjeGorevYonetimi.Models.Entities;

[Flags]
public enum SahaDenetimAdimTipleri
{
    None = 0,
    Puanlama = 1,
    IlgiliKisiler = 2,
    IlgiliUrunler = 4
}

public class SahaDenetimAdim : BaseEntity
{
    public int Id { get; set; }
    public int SahaDenetimId { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public int? SahaDenetimKategoriId { get; set; }
    public int Tipler { get; set; }
    public int Sira { get; set; }

    // Personel alanları (denetim sırasında doldurulur)
    public bool YapildiMi { get; set; }
    public int? Puan { get; set; } // 1..10
    public string? PersonelYorum { get; set; }
    public string? Not { get; set; }

    public virtual SahaDenetim SahaDenetim { get; set; } = null!;
    public virtual SahaDenetimKategori? Kategori { get; set; }
    public virtual ICollection<SahaDenetimAdimIlgiliKisi>? IlgiliKisiler { get; set; }
    public virtual ICollection<SahaDenetimAdimIlgiliUrun>? IlgiliUrunler { get; set; }
    public virtual ICollection<SahaDenetimAdimFoto>? Fotograflar { get; set; }
}

