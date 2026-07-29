using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("pay_payment_line")]
[Index(nameof(OrganizationId), Name = "idx_pay_payment_line_organization_id")]
[Index(nameof(PaymentBatchId), Name = "idx_pay_payment_line_payment_batch_id")]
[Index(nameof(EmployeeId), Name = "idx_pay_payment_line_employee_id")]
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

    [ForeignKey(nameof(OrganizationId))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(PaymentBatchId))]
    public virtual PayPaymentBatch PaymentBatch { get; set; } = null!;

    [ForeignKey(nameof(EmployeeId))]
    public virtual PayEmployee Employee { get; set; } = null!;

    [ForeignKey(nameof(PayrollLineId))]
    public virtual PayPayrollLine? PayrollLine { get; set; }
}
