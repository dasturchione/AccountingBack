using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;

namespace Domain.Entities;

[Table("pay_payroll_recalculation")]
[Index(nameof(OrganizationId), nameof(Status), Name = "idx_pay_payroll_recalculation_org_status")]
[Index(nameof(PayrollDocId), Name = "idx_pay_payroll_recalculation_payroll_doc_id")]
public partial class PayPayrollRecalculation
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("payroll_doc_id")]
    public long PayrollDocId { get; set; }

    [Column("status")]
    [StringLength(20)]
    public string Status { get; set; } = PayrollRecalculationStatusConst.Pending;

    [Column("reason")]
    [StringLength(1000)]
    public string Reason { get; set; } = null!;

    [Column("source_revision")]
    [StringLength(64)]
    public string? SourceRevision { get; set; }

    [Column("requested_date", TypeName = "timestamp without time zone")]
    public DateTime RequestedDate { get; set; }

    [Column("requested_by_user_id")]
    public int? RequestedByUserId { get; set; }

    [Column("completed_date", TypeName = "timestamp without time zone")]
    public DateTime? CompletedDate { get; set; }

    [Column("correction_doc_id")]
    public long? CorrectionDocId { get; set; }

    [Column("error_message")]
    [StringLength(2000)]
    public string? ErrorMessage { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(PayrollDocId))]
    public virtual PayPayrollDoc PayrollDoc { get; set; } = null!;

    [ForeignKey(nameof(CorrectionDocId))]
    public virtual PayPayrollDoc? CorrectionDoc { get; set; }
}
