namespace ProjeGorevYonetimi.Models.Entities;

public class SahaDenetimAdimIlgiliUrun : BaseEntity
{
    public int Id { get; set; }
    public int SahaDenetimAdimId { get; set; }
    public string StokKodu { get; set; } = string.Empty;
    public decimal Adet { get; set; }
    public string? Aciklama { get; set; }
    public int Sira { get; set; }

    public virtual SahaDenetimAdim SahaDenetimAdim { get; set; } = null!;
}

