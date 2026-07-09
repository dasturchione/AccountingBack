using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("acc_chart_account_preset_account")]
public partial class ChartAccountPresetAccount
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

    [ForeignKey("PresetId, ParentPresetAccountId")]
    [InverseProperty("InverseAccChartAccountPresetAccountNavigation")]
    public virtual ChartAccountPresetAccount? ChartAccountPresetAccountNavigation { get; set; }

    [InverseProperty("PresetAccount")]
    public virtual ICollection<ChartAccountPresetAccountSubkonto> ChartAccountPresetAccountSubkontos { get; set; } = new List<ChartAccountPresetAccountSubkonto>();

    [InverseProperty("PresetAccount")]
    public virtual ICollection<ChartAccountPresetAccountTranslation> ChartAccountPresetAccountTranslations { get; set; } = new List<ChartAccountPresetAccountTranslation>();

    [ForeignKey("AccountTypeId")]
    [InverseProperty("ChartAccountPresetAccounts")]
    public virtual AccountType AccountType { get; set; } = null!;

    [InverseProperty("ChartAccountPresetAccountNavigation")]
    public virtual ICollection<ChartAccountPresetAccount> InverseChartAccountPresetAccountNavigation { get; set; } = new List<ChartAccountPresetAccount>();

    [ForeignKey("PresetId")]
    [InverseProperty("ChartAccountPresetAccounts")]
    public virtual ChartAccountPreset Preset { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty(nameof(State.ChartAccountPresetAccounts))]
    public virtual State State { get; set; } = null!;
}
