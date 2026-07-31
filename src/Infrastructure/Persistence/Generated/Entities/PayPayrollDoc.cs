using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("pay_payroll_doc")]
[Index("CorrectionOfDocId", Name = "idx_pay_payroll_doc_correction_of")]
[Index("OrganizationId", Name = "idx_pay_payroll_doc_organization_id")]
[Index("PeriodId", Name = "idx_pay_payroll_doc_period_id")]
[Index("StatusId", Name = "idx_pay_payroll_doc_status_id")]
[Index("OrganizationId", "DocNumber", Name = "ux_pay_payroll_doc_org_doc_number", IsUnique = true)]
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

    [ForeignKey("CancelledByUserId")]
    [InverseProperty("PayPayrollDocCancelledByUsers")]
    public virtual SysUser? CancelledByUser { get; set; }

    [ForeignKey("CorrectionOfDocId")]
    [InverseProperty("InverseCorrectionOfDoc")]
    public virtual PayPayrollDoc? CorrectionOfDoc { get; set; }

    [ForeignKey("CreatedByUserId")]
    [InverseProperty("PayPayrollDocCreatedByUsers")]
    public virtual SysUser? CreatedByUser { get; set; }

    [ForeignKey("CurrencyId")]
    [InverseProperty("PayPayrollDocs")]
    public virtual CmnCurrency Currency { get; set; } = null!;

    [InverseProperty("CorrectionOfDoc")]
    public virtual ICollection<PayPayrollDoc> InverseCorrectionOfDoc { get; set; } = new List<PayPayrollDoc>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("PayPayrollDocs")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [InverseProperty("PayrollDoc")]
    public virtual ICollection<PayPaymentBatch> PayPaymentBatches { get; set; } = new List<PayPaymentBatch>();

    [InverseProperty("PayrollDoc")]
    public virtual ICollection<PayPayrollLine> PayPayrollLines { get; set; } = new List<PayPayrollLine>();

    [ForeignKey("PeriodId")]
    [InverseProperty("PayPayrollDocs")]
    public virtual PayPeriod Period { get; set; } = null!;

    [ForeignKey("PostedByUserId")]
    [InverseProperty("PayPayrollDocPostedByUsers")]
    public virtual SysUser? PostedByUser { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("PayPayrollDocs")]
    public virtual CmnState State { get; set; } = null!;

    [ForeignKey("StatusId")]
    [InverseProperty("PayPayrollDocs")]
    public virtual CmnDocumentStatus Status { get; set; } = null!;

    [ForeignKey("UpdatedByUserId")]
    [InverseProperty("PayPayrollDocUpdatedByUsers")]
    public virtual SysUser? UpdatedByUser { get; set; }
}
