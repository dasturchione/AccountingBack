using Application.Features.Dashboard.DTOs;
using Application.Features.Dashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;

namespace WebApi.Controllers.Tasks;

[Route("api/tasks")]
[ApiController]
[Authorize]
[ReadOnlyModuleAuthorize(PermissionCodeConst.DashboardView)]
public sealed class TaskCalendarController : ControllerBase
{
    private readonly ITaskCalendarService _service;

    public TaskCalendarController(ITaskCalendarService service) => _service = service;

    [HttpGet("calendar")]
    public Task<TaskCalendarDto> GetCalendar([FromQuery] DashboardFilterDto filter, CancellationToken ct = default) =>
        _service.GetAsync(filter, ct);
}
