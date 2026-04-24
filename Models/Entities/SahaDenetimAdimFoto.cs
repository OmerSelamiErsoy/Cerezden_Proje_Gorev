namespace ProjeGorevYonetimi.Models.Entities;

public class SahaDenetimAdimFoto : BaseEntity
{
    public int Id { get; set; }
    public int SahaDenetimAdimId { get; set; }
    public string DosyaYolu { get; set; } = string.Empty; // /uploads/...
    public string? DosyaAdi { get; set; }
    public int Sira { get; set; }

    public virtual SahaDenetimAdim SahaDenetimAdim { get; set; } = null!;
}

