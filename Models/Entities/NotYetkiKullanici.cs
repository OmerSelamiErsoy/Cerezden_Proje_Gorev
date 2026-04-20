namespace ProjeGorevYonetimi.Models.Entities;

public class NotYetkiKullanici : BaseEntity
{
    public int Id { get; set; }
    public int NotId { get; set; }
    public int KullaniciId { get; set; }

    public virtual Not Not { get; set; } = null!;
    public virtual Kullanici Kullanici { get; set; } = null!;
}

