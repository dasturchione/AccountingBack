using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("pay_component")]
[Index(nameof(OrganizationId), Name = "idx_pay_component_organization_id")]
[Index(nameof(ComponentType), Name = "idx_pay_component_type")]
[Index(nameof(EffectiveFrom), nameof(EffectiveTo), Name = "idx_pay_component_effective_dates")]
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

    [Column("proration_basis")]
    [StringLength(10)]
    public string ProrationBasis { get; set; } = "DAYS";

    [Column("default_amount")]
    [Precision(18, 2)]
    public decimal? DefaultAmount { get; set; }

    [Column("default_rate")]
    [Precision(9, 4)]
    public decimal? DefaultRate { get; set; }

    [Column("depends_on_component_id")]
    public int? DependsOnComponentId { get; set; }

    [Column("minimum_amount")]
    [Precision(18, 2)]
    public decimal? MinimumAmount { get; set; }

    [Column("maximum_amount")]
    [Precision(18, 2)]
    public decimal? MaximumAmount { get; set; }

    [Column("is_taxable")]
    public bool IsTaxable { get; set; } = true;

    [Column("is_mandatory")]
    public bool IsMandatory { get; set; }

    [Column("expense_account_id")]
    public int? ExpenseAccountId { get; set; }

    [Column("liability_account_id")]
    public int? LiabilityAccountId { get; set; }

    [Column("effective_from", TypeName = "date")]
    public DateOnly EffectiveFrom { get; set; }

    [Column("effective_to", TypeName = "date")]
    public DateOnly? EffectiveTo { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(ExpenseAccountId))]
    public virtual ChartAccount? ExpenseAccount { get; set; }

    [ForeignKey(nameof(LiabilityAccountId))]
    public virtual ChartAccount? LiabilityAccount { get; set; }

    [ForeignKey(nameof(DependsOnComponentId))]
    public virtual PayComponent? DependsOnComponent { get; set; }

    [ForeignKey(nameof(StateId))]
    public virtual State State { get; set; } = null!;

    public virtual ICollection<PayEmployeeComponent> EmployeeComponents { get; set; } = new List<PayEmployeeComponent>();
    public virtual ICollection<PayPayrollCalcLine> PayrollCalcLines { get; set; } = new List<PayPayrollCalcLine>();
}
