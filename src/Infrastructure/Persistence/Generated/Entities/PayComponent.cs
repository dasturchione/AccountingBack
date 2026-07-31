using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("pay_component")]
[Index("EffectiveFrom", "EffectiveTo", Name = "idx_pay_component_effective_dates")]
[Index("OrganizationId", Name = "idx_pay_component_organization_id")]
[Index("StateId", Name = "idx_pay_component_state_id")]
[Index("ComponentType", Name = "idx_pay_component_type")]
[Index("OrganizationId", "Code", "EffectiveFrom", Name = "ux_pay_component_org_code_from", IsUnique = true)]
public partial class PayComponent
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("component_type")]
    [StringLength(30)]
    public string ComponentType { get; set; } = null!;

    [Column("calculation_method")]
    [StringLength(30)]
    public string CalculationMethod { get; set; } = null!;

    [Column("default_amount")]
    [Precision(18, 2)]
    public decimal? DefaultAmount { get; set; }

    [Column("default_rate")]
    [Precision(9, 4)]
    public decimal? DefaultRate { get; set; }

    [Column("is_mandatory")]
    public bool IsMandatory { get; set; }

    [Column("expense_account_id")]
    public int? ExpenseAccountId { get; set; }

    [Column("liability_account_id")]
    public int? LiabilityAccountId { get; set; }

    [Column("effective_from")]
    public DateOnly EffectiveFrom { get; set; }

    [Column("effective_to")]
    public DateOnly? EffectiveTo { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    [ForeignKey("ExpenseAccountId")]
    [InverseProperty("PayComponentExpenseAccounts")]
    public virtual AccChartAccount? ExpenseAccount { get; set; }

    [ForeignKey("LiabilityAccountId")]
    [InverseProperty("PayComponentLiabilityAccounts")]
    public virtual AccChartAccount? LiabilityAccount { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("PayComponents")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [InverseProperty("Component")]
    public virtual ICollection<PayEmployeeComponent> PayEmployeeComponents { get; set; } = new List<PayEmployeeComponent>();

    [InverseProperty("Component")]
    public virtual ICollection<PayPayrollCalcLine> PayPayrollCalcLines { get; set; } = new List<PayPayrollCalcLine>();

    [ForeignKey("StateId")]
    [InverseProperty("PayComponents")]
    public virtual CmnState State { get; set; } = null!;
}
