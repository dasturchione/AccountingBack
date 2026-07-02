using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("sys_audit_log")]
[Index("Action", Name = "idx_sys_audit_log_action")]
[Index("ChangedDate", Name = "idx_sys_audit_log_changed_date")]
[Index("ChangedUserId", Name = "idx_sys_audit_log_changed_user_id")]
[Index("OrganizationId", Name = "idx_sys_audit_log_organization_id")]
[Index("TableName", "RecordId", Name = "idx_sys_audit_log_table_record")]
public partial class AuditLog
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int? OrganizationId { get; set; }

    [Column("schema_name")]
    [StringLength(100)]
    public string SchemaName { get; set; } = null!;

    [Column("table_name")]
    [StringLength(100)]
    public string TableName { get; set; } = null!;

    [Column("record_id")]
    [StringLength(100)]
    public string? RecordId { get; set; }

    [Column("action")]
    [StringLength(10)]
    public string Action { get; set; } = null!;

    [Column("old_data", TypeName = "jsonb")]
    public string? OldData { get; set; }

    [Column("new_data", TypeName = "jsonb")]
    public string? NewData { get; set; }

    [Column("changed_user_id")]
    public int? ChangedUserId { get; set; }

    [Column("request_id")]
    [StringLength(100)]
    public string? RequestId { get; set; }

    [Column("client_addr")]
    public IPAddress? ClientAddr { get; set; }

    [Column("application_name")]
    [StringLength(200)]
    public string? ApplicationName { get; set; }

    [Column("changed_date", TypeName = "timestamp without time zone")]
    public DateTime ChangedDate { get; set; }
}

