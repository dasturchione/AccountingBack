using SharedKernel.Filters;

namespace Application.Features.Notifications;

public sealed class NotificationQuery : IPaginationFilter
{
    public bool? IsRead { get; set; }
    public short? TypeId { get; set; }
    public int? OrganizationId { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; } = 20;
}
