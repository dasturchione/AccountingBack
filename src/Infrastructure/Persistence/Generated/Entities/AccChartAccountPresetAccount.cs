using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("acc_chart_account_preset_account")]
[Index("AccountTypeId", Name = "idx_acc_chart_account_preset_account_account_type_id")]
[Index("ParentPresetAccountId", Name = "idx_acc_chart_account_preset_account_parent_id")]
[Index("PresetId", Name = "idx_acc_chart_account_preset_account_preset_id")]
[Index("StateId", Name = "idx_acc_chart_account_preset_account_state_id")]
[Index("PresetId", "Id", Name = "uq_acc_chart_account_preset_account_preset_id", IsUnique = true)]
[Index("PresetId", "Number", Name = "uq_acc_chart_account_preset_account_preset_number", IsUnique = true)]
public partial class AccChartAccountPresetAccount
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("preset_id")]
    public short PresetId { get; set; }

    [Column("code")]
    [StringLength(20)]
    public string? Code { get; set; }

    [Column("number")]
    [StringLength(20)]
    public string Number { get; set; } = null!;

    [Column("parent_preset_account_id")]
    public int? ParentPresetAccountId { get; set; }

    [Column("account_type_id")]
    public short AccountTypeId { get; set; }

    [Column("is_currency")]
    public bool IsCurrency { get; set; }

    [Column("is_quantity")]
    public bool IsQuantity { get; set; }

    [Column("is_department")]
    public bool IsDepartment { get; set; }

    [Column("is_tax_accounting")]
    public bool IsTaxAccounting { get; set; }

    [Column("is_off_balance")]
    public bool IsOffBalance { get; set; }

    [Column("display_order")]
    public int DisplayOrder { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("is_group")]
    public bool IsGroup { get; set; }

    [ForeignKey("PresetId, ParentPresetAccountId")]
    [InverseProperty("InverseAccChartAccountPresetAccountNavigation")]
    public virtual AccChartAccountPresetAccount? AccChartAccountPresetAccountNavigation { get; set; }

    [InverseProperty("PresetAccount")]
    public virtual ICollection<AccChartAccountPresetAccountSubkonto> AccChartAccountPresetAccountSubkontos { get; set; } = new List<AccChartAccountPresetAccountSubkonto>();

    [InverseProperty("PresetAccount")]
    public virtual ICollection<AccChartAccountPresetAccountTranslation> AccChartAccountPresetAccountTranslations { get; set; } = new List<AccChartAccountPresetAccountTranslation>();

    [ForeignKey("AccountTypeId")]
    [InverseProperty("AccChartAccountPresetAccounts")]
    public virtual AccAccountType AccountType { get; set; } = null!;

    [InverseProperty("AccChartAccountPresetAccountNavigation")]
    public virtual ICollection<AccChartAccountPresetAccount> InverseAccChartAccountPresetAccountNavigation { get; set; } = new List<AccChartAccountPresetAccount>();

    [ForeignKey("PresetId")]
    [InverseProperty("AccChartAccountPresetAccounts")]
    public virtual AccChartAccountPreset Preset { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("AccChartAccountPresetAccounts")]
    public virtual CmnState State { get; set; } = null!;
}
