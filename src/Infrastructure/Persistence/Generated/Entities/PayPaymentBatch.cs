using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("pay_payment_batch")]
[Index("BankOperationId", Name = "idx_pay_payment_batch_bank_operation_id")]
[Index("CashOperationId", Name = "idx_pay_payment_batch_cash_operation_id")]
[Index("OrganizationId", Name = "idx_pay_payment_batch_organization_id")]
[Index("PayrollDocId", Name = "idx_pay_payment_batch_payroll_doc_id")]
[Index("PeriodId", Name = "idx_pay_payment_batch_period_id")]
[Index("StatusId", Name = "idx_pay_payment_batch_status_id")]
[Index("OrganizationId", "DocNumber", Name = "ux_pay_payment_batch_org_doc_number", IsUnique = true)]
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

    [ForeignKey("BankAccountId")]
    [InverseProperty("PayPaymentBatches")]
    public virtual OrgBankAccount? BankAccount { get; set; }

    [ForeignKey("BankOperationId")]
    [InverseProperty("PayPaymentBatch")]
    public virtual BankOperation? BankOperation { get; set; }

    [ForeignKey("CancelledByUserId")]
    [InverseProperty("PayPaymentBatchCancelledByUsers")]
    public virtual SysUser? CancelledByUser { get; set; }

    [ForeignKey("CashBoxId")]
    [InverseProperty("PayPaymentBatches")]
    public virtual CashBox? CashBox { get; set; }

    [ForeignKey("CashOperationId")]
    [InverseProperty("PayPaymentBatch")]
    public virtual CashOperation? CashOperation { get; set; }

    [ForeignKey("CreatedByUserId")]
    [InverseProperty("PayPaymentBatchCreatedByUsers")]
    public virtual SysUser? CreatedByUser { get; set; }

    [ForeignKey("CurrencyId")]
    [InverseProperty("PayPaymentBatches")]
    public virtual CmnCurrency Currency { get; set; } = null!;

    [ForeignKey("OffsetAccountId")]
    [InverseProperty("PayPaymentBatchOffsetAccounts")]
    public virtual AccChartAccount? OffsetAccount { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("PayPaymentBatches")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [InverseProperty("PaymentBatch")]
    public virtual ICollection<PayPaymentLine> PayPaymentLines { get; set; } = new List<PayPaymentLine>();

    [ForeignKey("PayrollDocId")]
    [InverseProperty("PayPaymentBatches")]
    public virtual PayPayrollDoc? PayrollDoc { get; set; }

    [ForeignKey("PeriodId")]
    [InverseProperty("PayPaymentBatches")]
    public virtual PayPeriod Period { get; set; } = null!;

    [ForeignKey("PostedByUserId")]
    [InverseProperty("PayPaymentBatchPostedByUsers")]
    public virtual SysUser? PostedByUser { get; set; }

    [ForeignKey("SourceChartAccountId")]
    [InverseProperty("PayPaymentBatchSourceChartAccounts")]
    public virtual AccChartAccount SourceChartAccount { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("PayPaymentBatches")]
    public virtual CmnState State { get; set; } = null!;

    [ForeignKey("StatusId")]
    [InverseProperty("PayPaymentBatches")]
    public virtual CmnDocumentStatus Status { get; set; } = null!;
}
