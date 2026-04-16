namespace ProjeGorevYonetimi.Models.Entities;

/// <summary>
/// Parametrik adet birimi (Adet, Metre, Kilogram vb.)
/// </summary>
public class AdetBirimi : BaseEntity
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public int Sira { get; set; }
}

