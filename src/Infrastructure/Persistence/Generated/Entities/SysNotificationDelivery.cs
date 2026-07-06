using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("sys_notification_delivery")]
[Index("NotificationId", Name = "idx_sys_notification_delivery_notification_id")]
[Index("Status", Name = "idx_sys_notification_delivery_status")]
public partial class SysNotificationDelivery
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("notification_id")]
    public long NotificationId { get; set; }

    [Column("channel")]
    public short Channel { get; set; }

    [Column("status")]
    public short Status { get; set; }

    [Column("error")]
    [StringLength(1000)]
    public string? Error { get; set; }

    [Column("sent_at", TypeName = "timestamp without time zone")]
    public DateTime? SentAt { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }
}
