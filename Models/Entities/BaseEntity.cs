namespace ProjeGorevYonetimi.Models.Entities;

/// <summary>
/// Tüm tablolarda soft delete ve audit alanları için temel sınıf.
/// </summary>
public abstract class BaseEntity
{
    public DateTime? InsertDate { get; set; }
    public DateTime? UpdateDate { get; set; }
    public DateTime? DeleteDate { get; set; }
    public int? InsertedByUserId { get; set; }
    public int? UpdatedByUserId { get; set; }
    public int? DeletedByUserId { get; set; }
    public bool IsDeleted { get; set; }
}
