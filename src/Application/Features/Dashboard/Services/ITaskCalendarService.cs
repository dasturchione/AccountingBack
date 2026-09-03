using Application.Features.Dashboard.DTOs;

namespace Application.Features.Dashboard.Services;

public interface ITaskCalendarService
{
    Task<TaskCalendarDto> GetAsync(DashboardFilterDto filter, CancellationToken ct = default);
}
