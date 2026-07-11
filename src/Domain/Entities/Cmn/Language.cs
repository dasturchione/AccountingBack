using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_language")]
[Index("Code", Name = "idx_cmn_language_code", IsUnique = true)]
[Index("StateId", Name = "idx_cmn_language_state_id")]
public partial class Language
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(10)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("native_name")]
    [StringLength(100)]
    public string NativeName { get; set; } = null!;

    [Column("is_default")]
    public bool IsDefault { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [InverseProperty("Language")]
    public virtual ICollection<Translation> Translations { get; set; } = new List<Translation>();

    [InverseProperty("DefaultLanguage")]
    public virtual ICollection<Organization> Organizations { get; set; } = new List<Organization>();

    [InverseProperty(nameof(SubkontoTypeTranslation.Language))]
    public virtual ICollection<SubkontoTypeTranslation> SubkontoTypeTranslations { get; set; } = new List<SubkontoTypeTranslation>();

    [InverseProperty("Language")]
    public virtual ICollection<PostingAliasTranslation> PostingAliasTranslations { get; set; } = new List<PostingAliasTranslation>();

    [InverseProperty("Language")]
    public virtual ICollection<ProductTypeTranslation> ProductTypeTranslations { get; set; } = new List<ProductTypeTranslation>();

    [InverseProperty(nameof(ChartAccountPresetTranslation.Language))]
    public virtual ICollection<ChartAccountPresetTranslation> ChartAccountPresetTranslations { get; set; } = new List<ChartAccountPresetTranslation>();

    [InverseProperty(nameof(ChartAccountPresetAccountTranslation.Language))]
    public virtual ICollection<ChartAccountPresetAccountTranslation> ChartAccountPresetAccountTranslations { get; set; } = new List<ChartAccountPresetAccountTranslation>();

    [InverseProperty("Language")]
    public virtual ICollection<PaymentPurposeTranslation> PaymentPurposeTranslations { get; set; } = new List<PaymentPurposeTranslation>();

    [ForeignKey("StateId")]
    [InverseProperty("Languages")]
    public virtual State State { get; set; } = null!;

    [InverseProperty("Language")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
