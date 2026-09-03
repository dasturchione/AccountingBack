using Application.Features.Dashboard.DTOs;

namespace Application.Features.Dashboard.Services;

public sealed class TaskCalendarService : ITaskCalendarService
{
    public Task<TaskCalendarDto> GetAsync(TaskCalendarFilterDto filter, CancellationToken ct = default) =>
        Task.FromResult(new TaskCalendarDto());
}
