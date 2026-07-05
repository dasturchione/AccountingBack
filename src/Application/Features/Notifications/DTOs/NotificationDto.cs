namespace Application.Features.Notifications;

public sealed class NotificationDto
{
    public long Id { get; set; }
    public string TypeCode { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? Link { get; set; }
    public DateTime CreatedDate { get; set; }
}
