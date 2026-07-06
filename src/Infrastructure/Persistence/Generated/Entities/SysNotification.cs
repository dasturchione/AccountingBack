using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("sys_notification")]
[Index("OrganizationId", Name = "idx_sys_notification_organization_id")]
[Index("TypeId", Name = "idx_sys_notification_type_id")]
[Index("UserId", "CreatedDate", Name = "idx_sys_notification_user_created_date", IsDescending = new[] { false, true })]
public partial class SysNotification
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int? OrganizationId { get; set; }

    [Column("user_id")]
    public int? UserId { get; set; }

    [Column("type_id")]
    public short TypeId { get; set; }

    [Column("title")]
    [StringLength(300)]
    public string Title { get; set; } = null!;

    [Column("body")]
    public string Body { get; set; } = null!;

    [Column("link")]
    [StringLength(500)]
    public string? Link { get; set; }

    [Column("entity_type")]
    [StringLength(100)]
    public string? EntityType { get; set; }

    [Column("entity_id")]
    public long? EntityId { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }
}
