using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("pay_payment_line")]
[Index("EmployeeId", Name = "idx_pay_payment_line_employee_id")]
[Index("OrganizationId", Name = "idx_pay_payment_line_organization_id")]
[Index("PaymentBatchId", Name = "idx_pay_payment_line_payment_batch_id")]
[Index("PayrollLineId", Name = "idx_pay_payment_line_payroll_line_id")]
[Index("PaymentBatchId", "EmployeeId", Name = "ux_pay_payment_line_employee", IsUnique = true)]
public partial class PayPaymentLine
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("payment_batch_id")]
    public long PaymentBatchId { get; set; }

    [Column("employee_id")]
    public long EmployeeId { get; set; }

    [Column("payroll_line_id")]
    public long? PayrollLineId { get; set; }

    [Column("amount")]
    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Column("note")]
    [StringLength(500)]
    public string? Note { get; set; }

    [ForeignKey("EmployeeId")]
    [InverseProperty("PayPaymentLines")]
    public virtual PayEmployee Employee { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("PayPaymentLines")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("PaymentBatchId")]
    [InverseProperty("PayPaymentLines")]
    public virtual PayPaymentBatch PaymentBatch { get; set; } = null!;

    [ForeignKey("PayrollLineId")]
    [InverseProperty("PayPaymentLines")]
    public virtual PayPayrollLine? PayrollLine { get; set; }
}
