using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("pay_payment_batch")]
[Index(nameof(OrganizationId), nameof(DocNumber), Name = "ux_pay_payment_batch_org_doc_number", IsUnique = true)]
[Index(nameof(PeriodId), Name = "idx_pay_payment_batch_period_id")]
[Index(nameof(PayrollDocId), Name = "idx_pay_payment_batch_payroll_doc_id")]
[Index(nameof(StatusId), Name = "idx_pay_payment_batch_status_id")]
public partial class PayPaymentBatch
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("period_id")]
    public long PeriodId { get; set; }

    [Column("payroll_doc_id")]
    public long? PayrollDocId { get; set; }

    [Column("doc_number")]
    [StringLength(100)]
    public string DocNumber { get; set; } = null!;

    [Column("doc_date", TypeName = "timestamp without time zone")]
    public DateTime DocDate { get; set; }

    [Column("payment_kind")]
    [StringLength(20)]
    public string PaymentKind { get; set; } = null!;

    [Column("source_type")]
    [StringLength(20)]
    public string SourceType { get; set; } = null!;

    [Column("bank_account_id")]
    public int? BankAccountId { get; set; }

    [Column("cash_box_id")]
    public int? CashBoxId { get; set; }

    [Column("source_chart_account_id")]
    public int SourceChartAccountId { get; set; }

    [Column("offset_account_id")]
    public int? OffsetAccountId { get; set; }

    [Column("currency_id")]
    public short CurrencyId { get; set; }

    [Column("total_amount")]
    [Precision(18, 2)]
    public decimal TotalAmount { get; set; }

    [Column("status_id")]
    public short StatusId { get; set; }

    [Column("bank_operation_id")]
    public long? BankOperationId { get; set; }

    [Column("cash_operation_id")]
    public long? CashOperationId { get; set; }

    [Column("note")]
    [StringLength(1000)]
    public string? Note { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("created_by_user_id")]
    public int? CreatedByUserId { get; set; }

    [Column("posted_at", TypeName = "timestamp without time zone")]
    public DateTime? PostedAt { get; set; }

    [Column("posted_by_user_id")]
    public int? PostedByUserId { get; set; }

    [Column("cancelled_at", TypeName = "timestamp without time zone")]
    public DateTime? CancelledAt { get; set; }

    [Column("cancelled_by_user_id")]
    public int? CancelledByUserId { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(PeriodId))]
    public virtual PayPeriod Period { get; set; } = null!;

    [ForeignKey(nameof(PayrollDocId))]
    public virtual PayPayrollDoc? PayrollDoc { get; set; }

    [ForeignKey(nameof(BankAccountId))]
    public virtual BankAccount? BankAccount { get; set; }

    [ForeignKey(nameof(CashBoxId))]
    public virtual CashBox? CashBox { get; set; }

    [ForeignKey(nameof(SourceChartAccountId))]
    public virtual ChartAccount SourceChartAccount { get; set; } = null!;

    [ForeignKey(nameof(OffsetAccountId))]
    public virtual ChartAccount? OffsetAccount { get; set; }

    [ForeignKey(nameof(CurrencyId))]
    public virtual Currency Currency { get; set; } = null!;

    [ForeignKey(nameof(StatusId))]
    public virtual DocumentStatus Status { get; set; } = null!;

    [ForeignKey(nameof(StateId))]
    public virtual State State { get; set; } = null!;

    [ForeignKey(nameof(BankOperationId))]
    public virtual BankOperation? BankOperation { get; set; }

    [ForeignKey(nameof(CashOperationId))]
    public virtual CashOperation? CashOperation { get; set; }

    public virtual ICollection<PayPaymentLine> Lines { get; set; } = new List<PayPaymentLine>();
}
