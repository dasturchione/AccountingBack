using Application.Features.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers.Sys;

[Route("api/notifications")]
[ApiController]
[Authorize]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationsController(INotificationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IResult> GetForCurrentUser([FromQuery] NotificationQuery query, CancellationToken ct = default)
    {
        var result = await _service.GetForUserAsync(query, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("unread-count")]
    public async Task<IResult> GetUnreadCount(CancellationToken ct = default)
    {
        var result = await _service.GetUnreadCountAsync(ct);
        return result.Match(count => Results.Ok(new { count }), CustomResults.Problem);
    }

    [HttpPost("{id:long}/read")]
    public async Task<IResult> MarkAsRead([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.MarkAsReadAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("read-all")]
    public async Task<IResult> MarkAllAsRead(CancellationToken ct = default)
    {
        var result = await _service.MarkAllAsReadAsync(ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
