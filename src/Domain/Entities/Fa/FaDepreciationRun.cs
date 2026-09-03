using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("fa_depreciation_run")]
[Index("StateId", Name = "idx_fa_depreciation_run_state_id")]
[Index("StatusId", Name = "idx_fa_depreciation_run_status_id")]
[Index("PeriodMonth", Name = "idx_fa_depreciation_run_period_month")]
public partial class FaDepreciationRun
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("doc_number")]
    [StringLength(100)]
    public string DocNumber { get; set; } = null!;

    [Column("period_month", TypeName = "date")]
    public DateTime PeriodMonth { get; set; }

    [Column("status_id")]
    public short StatusId { get; set; }

    [Column("note")]
    [StringLength(500)]
    public string? Note { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("created_by_user_id")]
    public int? CreatedByUserId { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime UpdatedDate { get; set; }

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

    [ForeignKey("CreatedByUserId")]
    public virtual User? CreatedByUser { get; set; }

    [InverseProperty("DepreciationRun")]
    public virtual ICollection<FaDepreciationRunLine> Lines { get; set; } = new List<FaDepreciationRunLine>();

    [ForeignKey("OrganizationId")]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey("PostedByUserId")]
    public virtual User? PostedByUser { get; set; }

    [ForeignKey("CancelledByUserId")]
    public virtual User? CancelledByUser { get; set; }

    [ForeignKey("StateId")]
    public virtual State State { get; set; } = null!;

    [ForeignKey("StatusId")]
    public virtual DocumentStatus Status { get; set; } = null!;

    [ForeignKey("UpdatedByUserId")]
    public virtual User? UpdatedByUser { get; set; }
}
