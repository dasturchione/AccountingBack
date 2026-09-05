using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("pay_payroll_doc")]
[Index(nameof(PeriodId), Name = "idx_pay_payroll_doc_period_id")]
[Index(nameof(StatusId), Name = "idx_pay_payroll_doc_status_id")]
public partial class PayPayrollDoc
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("period_id")]
    public long PeriodId { get; set; }

    [Column("doc_number")]
    [StringLength(100)]
    public string DocNumber { get; set; } = null!;

    [Column("doc_date", TypeName = "timestamp without time zone")]
    public DateTime DocDate { get; set; }

    [Column("document_kind")]
    [StringLength(20)]
    public string DocumentKind { get; set; } = null!;

    [Column("correction_of_doc_id")]
    public long? CorrectionOfDocId { get; set; }

    [Column("currency_id")]
    public short CurrencyId { get; set; }

    [Column("status_id")]
    public short StatusId { get; set; }

    [Column("salary_expense_account_id")]
    public int? SalaryExpenseAccountId { get; set; }

    [Column("salary_payable_account_id")]
    public int? SalaryPayableAccountId { get; set; }

    [Column("deduction_payable_account_id")]
    public int? DeductionPayableAccountId { get; set; }

    [Column("employer_tax_expense_account_id")]
    public int? EmployerTaxExpenseAccountId { get; set; }

    [Column("employer_tax_payable_account_id")]
    public int? EmployerTaxPayableAccountId { get; set; }

    [Column("advance_receivable_account_id")]
    public int? AdvanceReceivableAccountId { get; set; }

    [Column("gross_amount")]
    [Precision(18, 2)]
    public decimal GrossAmount { get; set; }

    [Column("deduction_amount")]
    [Precision(18, 2)]
    public decimal DeductionAmount { get; set; }

    [Column("employer_tax_amount")]
    [Precision(18, 2)]
    public decimal EmployerTaxAmount { get; set; }

    [Column("advance_amount")]
    [Precision(18, 2)]
    public decimal AdvanceAmount { get; set; }

    [Column("net_amount")]
    [Precision(18, 2)]
    public decimal NetAmount { get; set; }

    [Column("payable_amount")]
    [Precision(18, 2)]
    public decimal PayableAmount { get; set; }

    [Column("note")]
    [StringLength(1000)]
    public string? Note { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("created_by_user_id")]
    public int? CreatedByUserId { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    [Column("updated_by_user_id")]
    public int? UpdatedByUserId { get; set; }

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

    [ForeignKey(nameof(CorrectionOfDocId))]
    public virtual PayPayrollDoc? CorrectionOfDoc { get; set; }

    [InverseProperty(nameof(CorrectionOfDoc))]
    public virtual ICollection<PayPayrollDoc> Corrections { get; set; } = new List<PayPayrollDoc>();

    [ForeignKey(nameof(CurrencyId))]
    public virtual Currency Currency { get; set; } = null!;

    [ForeignKey(nameof(StatusId))]
    public virtual DocumentStatus Status { get; set; } = null!;

    [ForeignKey(nameof(SalaryExpenseAccountId))]
    public virtual ChartAccount? SalaryExpenseAccount { get; set; }

    [ForeignKey(nameof(SalaryPayableAccountId))]
    public virtual ChartAccount? SalaryPayableAccount { get; set; }

    [ForeignKey(nameof(DeductionPayableAccountId))]
    public virtual ChartAccount? DeductionPayableAccount { get; set; }

    [ForeignKey(nameof(EmployerTaxExpenseAccountId))]
    public virtual ChartAccount? EmployerTaxExpenseAccount { get; set; }

    [ForeignKey(nameof(EmployerTaxPayableAccountId))]
    public virtual ChartAccount? EmployerTaxPayableAccount { get; set; }

    [ForeignKey(nameof(AdvanceReceivableAccountId))]
    public virtual ChartAccount? AdvanceReceivableAccount { get; set; }

    [ForeignKey(nameof(StateId))]
    public virtual State State { get; set; } = null!;

    public virtual ICollection<PayPayrollLine> Lines { get; set; } = new List<PayPayrollLine>();
    public virtual ICollection<PayPaymentBatch> PaymentBatches { get; set; } = new List<PayPaymentBatch>();
}
