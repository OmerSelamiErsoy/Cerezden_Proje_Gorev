namespace ProjeGorevYonetimi.Models.Entities;

public class SahaDenetimAdimIlgiliKisi : BaseEntity
{
    public int Id { get; set; }
    public int SahaDenetimAdimId { get; set; }
    public string AdSoyad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public int Sira { get; set; }

    public virtual SahaDenetimAdim SahaDenetimAdim { get; set; } = null!;
}

