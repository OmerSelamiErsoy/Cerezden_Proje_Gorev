namespace ProjeGorevYonetimi.Models.Entities;

public class SahaDenetimYetkiKullanici : BaseEntity
{
    public int Id { get; set; }
    public int SahaDenetimId { get; set; }
    public int KullaniciId { get; set; }

    public virtual SahaDenetim SahaDenetim { get; set; } = null!;
    public virtual Kullanici Kullanici { get; set; } = null!;
}

