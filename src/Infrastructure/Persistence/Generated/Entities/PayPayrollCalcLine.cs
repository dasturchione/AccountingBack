using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("pay_payroll_calc_line")]
[Index("ComponentId", Name = "idx_pay_payroll_calc_line_component_id")]
[Index("OrganizationId", Name = "idx_pay_payroll_calc_line_organization_id")]
[Index("PayrollLineId", Name = "idx_pay_payroll_calc_line_payroll_line_id")]
[Index("PayrollLineId", "ComponentId", Name = "ux_pay_payroll_calc_line_component", IsUnique = true)]
public partial class PayPayrollCalcLine
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("payroll_line_id")]
    public long PayrollLineId { get; set; }

    [Column("component_id")]
    public int ComponentId { get; set; }

    [Column("base_amount")]
    [Precision(18, 2)]
    public decimal BaseAmount { get; set; }

    [Column("quantity")]
    [Precision(12, 4)]
    public decimal? Quantity { get; set; }

    [Column("rate")]
    [Precision(9, 4)]
    public decimal? Rate { get; set; }

    [Column("amount")]
    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Column("is_manual")]
    public bool IsManual { get; set; }

    [Column("note")]
    [StringLength(500)]
    public string? Note { get; set; }

    [ForeignKey("ComponentId")]
    [InverseProperty("PayPayrollCalcLines")]
    public virtual PayComponent Component { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("PayPayrollCalcLines")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("PayrollLineId")]
    [InverseProperty("PayPayrollCalcLines")]
    public virtual PayPayrollLine PayrollLine { get; set; } = null!;
}
