using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("pay_timesheet")]
[Index("OrganizationId", Name = "idx_pay_timesheet_organization_id")]
[Index("PeriodId", Name = "idx_pay_timesheet_period_id")]
[Index("StateId", Name = "idx_pay_timesheet_state_id")]
[Index("StatusId", Name = "idx_pay_timesheet_status_id")]
[Index("OrganizationId", "DocNumber", Name = "ux_pay_timesheet_org_doc_number", IsUnique = true)]
public partial class PayTimesheet
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

    [Column("status_id")]
    public short StatusId { get; set; }

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
    [InverseProperty("PayTimesheetCancelledByUsers")]
    public virtual SysUser? CancelledByUser { get; set; }

    [ForeignKey("CreatedByUserId")]
    [InverseProperty("PayTimesheetCreatedByUsers")]
    public virtual SysUser? CreatedByUser { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("PayTimesheets")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [InverseProperty("Timesheet")]
    public virtual ICollection<PayTimesheetLine> PayTimesheetLines { get; set; } = new List<PayTimesheetLine>();

    [ForeignKey("PeriodId")]
    [InverseProperty("PayTimesheets")]
    public virtual PayPeriod Period { get; set; } = null!;

    [ForeignKey("PostedByUserId")]
    [InverseProperty("PayTimesheetPostedByUsers")]
    public virtual SysUser? PostedByUser { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("PayTimesheets")]
    public virtual CmnState State { get; set; } = null!;

    [ForeignKey("StatusId")]
    [InverseProperty("PayTimesheets")]
    public virtual CmnDocumentStatus Status { get; set; } = null!;

    [ForeignKey("UpdatedByUserId")]
    [InverseProperty("PayTimesheetUpdatedByUsers")]
    public virtual SysUser? UpdatedByUser { get; set; }
}
