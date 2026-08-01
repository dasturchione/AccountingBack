using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("hr_absence_attachment")]
[Index("AbsenceId", Name = "idx_hr_absence_attachment_absence")]
[Index("OrganizationId", Name = "idx_hr_absence_attachment_organization")]
public partial class HrAbsenceAttachment
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("absence_id")]
    public long AbsenceId { get; set; }

    [Column("file_path")]
    [StringLength(500)]
    public string FilePath { get; set; } = null!;

    [Column("original_file_name")]
    [StringLength(255)]
    public string OriginalFileName { get; set; } = null!;

    [Column("content_type")]
    [StringLength(150)]
    public string ContentType { get; set; } = null!;

    [Column("file_size")]
    public long FileSize { get; set; }

    [Column("telegram_file_id")]
    [StringLength(300)]
    public string TelegramFileId { get; set; } = null!;

    [Column("telegram_message_id")]
    public int TelegramMessageId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("created_by_user_id")]
    public int? CreatedByUserId { get; set; }

    [ForeignKey("AbsenceId")]
    [InverseProperty("HrAbsenceAttachments")]
    public virtual HrAbsence Absence { get; set; } = null!;

    [ForeignKey("CreatedByUserId")]
    [InverseProperty("HrAbsenceAttachments")]
    public virtual SysUser? CreatedByUser { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("HrAbsenceAttachments")]
    public virtual OrgOrganization Organization { get; set; } = null!;
}
