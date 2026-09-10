using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("pay_payroll_tax_line")]
[Index(nameof(OrganizationId), Name = "idx_pay_payroll_tax_line_organization_id")]
[Index(nameof(PayrollLineId), Name = "idx_pay_payroll_tax_line_payroll_line_id")]
[Index(nameof(TaxDefinitionId), Name = "idx_pay_payroll_tax_line_tax_definition_id")]
public partial class PayPayrollTaxLine
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("payroll_line_id")]
    public long PayrollLineId { get; set; }

    [Column("tax_definition_id")]
    public int TaxDefinitionId { get; set; }

    [Column("base_amount")]
    [Precision(18, 2)]
    public decimal BaseAmount { get; set; }

    [Column("exemption_amount")]
    [Precision(18, 2)]
    public decimal ExemptionAmount { get; set; }

    [Column("taxable_base")]
    [Precision(18, 2)]
    public decimal TaxableBase { get; set; }

    [Column("rate")]
    [Precision(9, 4)]
    public decimal Rate { get; set; }

    [Column("amount")]
    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Column("liability_account_id")]
    public int LiabilityAccountId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(PayrollLineId))]
    public virtual PayPayrollLine PayrollLine { get; set; } = null!;

    [ForeignKey(nameof(TaxDefinitionId))]
    public virtual PayTaxDefinition TaxDefinition { get; set; } = null!;

    [ForeignKey(nameof(LiabilityAccountId))]
    public virtual ChartAccount LiabilityAccount { get; set; } = null!;
}
