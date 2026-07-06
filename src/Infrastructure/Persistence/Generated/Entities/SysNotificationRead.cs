using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("sys_notification_read")]
[Index("UserId", "NotificationId", Name = "idx_sys_notification_read_user_id_notification_id")]
[Index("NotificationId", "UserId", Name = "sys_notification_read_notification_id_user_id_key", IsUnique = true)]
public partial class SysNotificationRead
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("notification_id")]
    public long NotificationId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("read_at", TypeName = "timestamp without time zone")]
    public DateTime ReadAt { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }
}
