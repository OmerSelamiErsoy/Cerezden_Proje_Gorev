namespace ProjeGorevYonetimi.Models.Entities;

public class ProjeYetkiKullanici : BaseEntity
{
    public int Id { get; set; }
    public int ProjeId { get; set; }
    public int KullaniciId { get; set; }

    public virtual Proje Proje { get; set; } = null!;
    public virtual Kullanici Kullanici { get; set; } = null!;
}
