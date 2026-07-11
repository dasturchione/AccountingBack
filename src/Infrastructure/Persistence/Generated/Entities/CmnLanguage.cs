using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("cmn_language")]
[Index("Code", Name = "idx_cmn_language_code", IsUnique = true)]
[Index("StateId", Name = "idx_cmn_language_state_id")]
public partial class CmnLanguage
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
    public virtual ICollection<AccChartAccountPresetAccountTranslation> AccChartAccountPresetAccountTranslations { get; set; } = new List<AccChartAccountPresetAccountTranslation>();

    [InverseProperty("Language")]
    public virtual ICollection<AccChartAccountPresetTranslation> AccChartAccountPresetTranslations { get; set; } = new List<AccChartAccountPresetTranslation>();
    [InverseProperty("Language")]
    public virtual ICollection<AccSubkontoTypeTranslation> AccSubkontoTypeTranslations { get; set; } = new List<AccSubkontoTypeTranslation>();

    [InverseProperty("Language")]
    public virtual ICollection<CmnProductTypeTranslation> CmnProductTypeTranslations { get; set; } = new List<CmnProductTypeTranslation>();

    [InverseProperty("Language")]
    public virtual ICollection<CmnTranslation> CmnTranslations { get; set; } = new List<CmnTranslation>();

    [InverseProperty("DefaultLanguage")]
    public virtual ICollection<OrgOrganization> OrgOrganizations { get; set; } = new List<OrgOrganization>();

    [ForeignKey("StateId")]
    [InverseProperty("CmnLanguages")]
    public virtual CmnState State { get; set; } = null!;

    [InverseProperty("Language")]
    public virtual ICollection<SysUser> SysUsers { get; set; } = new List<SysUser>();
}
