namespace Application.Features.Notifications;

public sealed class CreateNotificationRequest
{
    public short? TypeId { get; set; }
    public string? TypeCode { get; set; }
    public List<NotificationChannel>? Channels { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public int? OrganizationId { get; set; }
    public string? Link { get; set; }
    public string? EntityType { get; set; }
    public long? EntityId { get; set; }
}
