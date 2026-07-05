using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("sys_notification_delivery")]
[Index("NotificationId", Name = "idx_sys_notification_delivery_notification_id")]
[Index("Status", Name = "idx_sys_notification_delivery_status")]
public partial class NotificationDelivery
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("notification_id")]
    public long NotificationId { get; set; }

    // 1=InApp, 2=Email, 3=Push, 4=Sms
    [Column("channel")]
    public short Channel { get; set; }

    // 0=Pending, 1=Sent, 2=Failed
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
