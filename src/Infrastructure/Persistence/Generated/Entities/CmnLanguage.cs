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
    public virtual ICollection<AccAccountTypeTranslation> AccAccountTypeTranslations { get; set; } = new List<AccAccountTypeTranslation>();

    [InverseProperty("Language")]
    public virtual ICollection<AccChartAccountPresetAccountTranslation> AccChartAccountPresetAccountTranslations { get; set; } = new List<AccChartAccountPresetAccountTranslation>();

    [InverseProperty("Language")]
    public virtual ICollection<AccChartAccountPresetTranslation> AccChartAccountPresetTranslations { get; set; } = new List<AccChartAccountPresetTranslation>();

    [InverseProperty("Language")]
    public virtual ICollection<AccDocumentAccountRoleTranslation> AccDocumentAccountRoleTranslations { get; set; } = new List<AccDocumentAccountRoleTranslation>();

    [InverseProperty("Language")]
    public virtual ICollection<AccDocumentAccountTypeTranslation> AccDocumentAccountTypeTranslations { get; set; } = new List<AccDocumentAccountTypeTranslation>();

    [InverseProperty("Language")]
    public virtual ICollection<AccSubkontoTypeTranslation> AccSubkontoTypeTranslations { get; set; } = new List<AccSubkontoTypeTranslation>();

    [InverseProperty("Language")]
    public virtual ICollection<CmnContractTypeTranslation> CmnContractTypeTranslations { get; set; } = new List<CmnContractTypeTranslation>();

    [InverseProperty("Language")]
    public virtual ICollection<CmnCostingMethodTranslation> CmnCostingMethodTranslations { get; set; } = new List<CmnCostingMethodTranslation>();

    [InverseProperty("Language")]
    public virtual ICollection<CmnCounterpartyTypeTranslation> CmnCounterpartyTypeTranslations { get; set; } = new List<CmnCounterpartyTypeTranslation>();

    [InverseProperty("Language")]
    public virtual ICollection<CmnCurrencyTranslation> CmnCurrencyTranslations { get; set; } = new List<CmnCurrencyTranslation>();

    [InverseProperty("Language")]
    public virtual ICollection<CmnDocumentStatusTranslation> CmnDocumentStatusTranslations { get; set; } = new List<CmnDocumentStatusTranslation>();

    [InverseProperty("Language")]
    public virtual ICollection<CmnDocumentTypeTranslation> CmnDocumentTypeTranslations { get; set; } = new List<CmnDocumentTypeTranslation>();

    [InverseProperty("Language")]
    public virtual ICollection<CmnMovementDirectionTranslation> CmnMovementDirectionTranslations { get; set; } = new List<CmnMovementDirectionTranslation>();

    [InverseProperty("Language")]
    public virtual ICollection<CmnOperationTypeTranslation> CmnOperationTypeTranslations { get; set; } = new List<CmnOperationTypeTranslation>();

    [InverseProperty("Language")]
    public virtual ICollection<CmnPaymentTypeTranslation> CmnPaymentTypeTranslations { get; set; } = new List<CmnPaymentTypeTranslation>();

    [InverseProperty("Language")]
    public virtual ICollection<CmnTranslation> CmnTranslations { get; set; } = new List<CmnTranslation>();

    [InverseProperty("Language")]
    public virtual ICollection<FaDisposalTypeTranslation> FaDisposalTypeTranslations { get; set; } = new List<FaDisposalTypeTranslation>();

    [InverseProperty("Language")]
    public virtual ICollection<FaReceiptTypeTranslation> FaReceiptTypeTranslations { get; set; } = new List<FaReceiptTypeTranslation>();

    [InverseProperty("Language")]
    public virtual ICollection<FiscalCashRegisterTypeTranslation> FiscalCashRegisterTypeTranslations { get; set; } = new List<FiscalCashRegisterTypeTranslation>();

    [InverseProperty("Language")]
    public virtual ICollection<InvProductGroupTranslation> InvProductGroupTranslations { get; set; } = new List<InvProductGroupTranslation>();

    [InverseProperty("DefaultLanguage")]
    public virtual ICollection<OrgOrganization> OrgOrganizations { get; set; } = new List<OrgOrganization>();

    [InverseProperty("Language")]
    public virtual ICollection<RtlPaymentMethodTranslation> RtlPaymentMethodTranslations { get; set; } = new List<RtlPaymentMethodTranslation>();

    [ForeignKey("StateId")]
    [InverseProperty("CmnLanguages")]
    public virtual CmnState State { get; set; } = null!;

    [InverseProperty("Language")]
    public virtual ICollection<SysUserKindTranslation> SysUserKindTranslations { get; set; } = new List<SysUserKindTranslation>();

    [InverseProperty("Language")]
    public virtual ICollection<SysUser> SysUsers { get; set; } = new List<SysUser>();
}
