using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.AuditLogs;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Hr.Schedules;

public sealed class HrWorkScheduleService : BaseService, IHrWorkScheduleService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly IQueryRepository<PayEmployee> _employeeQuery;
    private readonly IQueryRepository<PayEmployment> _employmentQuery;
    private readonly IQueryRepository<HrEmployeeWorkSchedule> _scheduleQuery;
    private readonly ICommandRepository<HrEmployeeWorkSchedule> _scheduleCommand;
    private readonly ICommandRepository<HrEmployeeWorkScheduleDay> _dayCommand;
    private readonly IQueryRepository<PayTimesheetLine> _timesheetLineQuery;

    public HrWorkScheduleService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IAuditLogService auditLogService,
        IQueryRepository<PayEmployee> employeeQuery,
        IQueryRepository<PayEmployment> employmentQuery,
        IQueryRepository<HrEmployeeWorkSchedule> scheduleQuery,
        ICommandRepository<HrEmployeeWorkSchedule> scheduleCommand,
        ICommandRepository<HrEmployeeWorkScheduleDay> dayCommand,
        IQueryRepository<PayTimesheetLine> timesheetLineQuery,
        ILogger<HrWorkScheduleService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _auditLogService = auditLogService;
        _employeeQuery = employeeQuery;
        _employmentQuery = employmentQuery;
        _scheduleQuery = scheduleQuery;
        _scheduleCommand = scheduleCommand;
        _dayCommand = dayCommand;
        _timesheetLineQuery = timesheetLineQuery;
    }

    public Task<Result<List<HrWorkScheduleDto>>> GetAllAsync(long employeeId, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            if (!await _employeeQuery.AnyAsync(x => x.Id == employeeId, ct))
                return Result.Failure<List<HrWorkScheduleDto>>(HrErrors.NotFound("Employee", employeeId));

            var query = _queryBuilder.For<HrEmployeeWorkSchedule>()
                .Where(x => x.EmployeeId == employeeId && x.StateId == StateIdConst.ACTIVE)
                .As(x => new HrWorkScheduleDto
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    Name = x.Name,
                    EffectiveFrom = x.EffectiveFrom,
                    EffectiveTo = x.EffectiveTo,
                    StateId = x.StateId,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate,
                    WeeklyHours = x.Days.Sum(day => day.WorkHours),
                    Days = x.Days
                        .OrderBy(day => day.DayOfWeek)
                        .Select(day => new HrWorkScheduleDaySaveDto
                        {
                            DayOfWeek = day.DayOfWeek,
                            WorkHours = day.WorkHours
                        })
                        .ToList()
                })
                .Build();

            var schedules = await _scheduleQuery.GetAllAsync(query, ct);
            return Result.Success(schedules.OrderByDescending(x => x.EffectiveFrom).ToList());
        });

    public Task<Result<long>> CreateAsync(
        long employeeId,
        HrWorkScheduleSaveDto dto,
        CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            var validation = await ValidateAsync(employeeId, dto, null, ct);
            if (!validation.IsSuccess)
                return Result.Failure<long>(validation.Error);

            var organizationId = _userContext.OrganizationId!.Value;
            var entity = new HrEmployeeWorkSchedule
            {
                OrganizationId = organizationId,
                EmployeeId = employeeId,
                Name = dto.Name.Trim(),
                EffectiveFrom = dto.EffectiveFrom,
                EffectiveTo = dto.EffectiveTo,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.Now,
                CreatedByUserId = _userContext.Id,
                Days = BuildDays(organizationId, dto.Days)
            };

            await _scheduleCommand.CreateAsync(entity, ct);
            _auditLogService.SetNewValues(Map(entity));
            await _auditLogService.CreateAsync(
                AuditLogTableConst.HrEmployeeWorkSchedule,
                entity.Id.ToString(),
                AuditLogOperationTypeConst.Create);
            return Result.Success(entity.Id);
        }, ct);

    public Task<Result> UpdateAsync(
        long employeeId,
        long scheduleId,
        HrWorkScheduleSaveDto dto,
        CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            var query = _queryBuilder.For<HrEmployeeWorkSchedule>()
                .Where(x => x.Id == scheduleId && x.EmployeeId == employeeId && x.StateId == StateIdConst.ACTIVE)
                .Build();
            var entity = await _scheduleQuery.GetAsync(query, ct);
            if (entity is null)
                return Result.Failure(HrErrors.NotFound("WorkSchedule", scheduleId));

            if (await IsLockedByPostedTimesheetAsync(employeeId, entity.EffectiveFrom, entity.EffectiveTo, ct))
                return Result.Failure(HrErrors.Conflict(
                    "ScheduleLocked",
                    "The work schedule is used by a posted timesheet. Close it and create a new effective schedule."));

            var validation = await ValidateAsync(employeeId, dto, scheduleId, ct);
            if (!validation.IsSuccess)
                return validation;

            _auditLogService.SetOldValues(Map(entity));
            entity.Name = dto.Name.Trim();
            entity.EffectiveFrom = dto.EffectiveFrom;
            entity.EffectiveTo = dto.EffectiveTo;
            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedByUserId = _userContext.Id;

            await _dayCommand.DeleteAsync(x => x.ScheduleId == scheduleId, ct);
            var days = BuildDays(entity.OrganizationId, dto.Days);
            foreach (var day in days)
                day.ScheduleId = scheduleId;
            await _dayCommand.CreateAsync(days, ct);
            await _scheduleCommand.UpdateAsync(entity, ct);

            var updated = Map(entity);
            updated.Days = dto.Days.OrderBy(x => x.DayOfWeek).ToList();
            updated.WeeklyHours = dto.Days.Sum(x => x.WorkHours);
            _auditLogService.SetNewValues(updated);
            await _auditLogService.CreateAsync(
                AuditLogTableConst.HrEmployeeWorkSchedule,
                scheduleId.ToString(),
                AuditLogOperationTypeConst.Update);
            return Result.Success();
        }, ct);

    public Task<Result> DeleteAsync(long employeeId, long scheduleId, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
        {
            var query = _queryBuilder.For<HrEmployeeWorkSchedule>()
                .Where(x => x.Id == scheduleId && x.EmployeeId == employeeId && x.StateId == StateIdConst.ACTIVE)
                .Build();
            var entity = await _scheduleQuery.GetAsync(query, ct);
            if (entity is null)
                return Result.Failure(HrErrors.NotFound("WorkSchedule", scheduleId));

            if (await IsLockedByPostedTimesheetAsync(employeeId, entity.EffectiveFrom, entity.EffectiveTo, ct))
                return Result.Failure(HrErrors.Conflict(
                    "ScheduleLocked",
                    "The work schedule is used by a posted timesheet and cannot be deleted."));

            _auditLogService.SetOldValues(Map(entity));
            await _scheduleCommand.DeleteAsync(entity, ct);
            await _auditLogService.CreateAsync(
                AuditLogTableConst.HrEmployeeWorkSchedule,
                scheduleId.ToString(),
                AuditLogOperationTypeConst.Delete);
            return Result.Success();
        }, ct);

    private async Task<Result> ValidateAsync(
        long employeeId,
        HrWorkScheduleSaveDto dto,
        long? currentId,
        CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        if (!await _employeeQuery.AnyAsync(x =>
                x.Id == employeeId &&
                x.OrganizationId == _userContext.OrganizationId.Value &&
                x.StateId == StateIdConst.ACTIVE, ct))
            return Result.Failure(HrErrors.NotFound("Employee", employeeId));

        if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Trim().Length > 200)
            return Result.Failure(HrErrors.Business("InvalidScheduleName", "Schedule name is required and must not exceed 200 characters."));
        if (dto.EffectiveTo.HasValue && dto.EffectiveTo.Value < dto.EffectiveFrom)
            return Result.Failure(HrErrors.Business("InvalidScheduleDates", "EffectiveTo must be on or after EffectiveFrom."));
        if (dto.Days.Count == 0)
            return Result.Failure(HrErrors.Business("EmptySchedule", "At least one working day is required."));
        if (dto.Days.Any(x => x.DayOfWeek is < 1 or > 7 || x.WorkHours is <= 0m or > 24m))
            return Result.Failure(HrErrors.Business("InvalidScheduleDay", "DayOfWeek must be 1..7 and WorkHours must be greater than 0 and not exceed 24."));
        if (dto.Days.GroupBy(x => x.DayOfWeek).Any(group => group.Count() > 1))
            return Result.Failure(HrErrors.Conflict("DuplicateScheduleDay", "A day of week can occur only once in a schedule."));
        if (dto.Days.Sum(x => x.WorkHours) > 168m)
            return Result.Failure(HrErrors.Business("InvalidWeeklyHours", "Weekly work hours must not exceed 168."));

        var endDate = dto.EffectiveTo ?? DateOnly.MaxValue;
        if (await _scheduleQuery.AnyAsync(x =>
                x.EmployeeId == employeeId &&
                x.StateId == StateIdConst.ACTIVE &&
                (!currentId.HasValue || x.Id != currentId.Value) &&
                x.EffectiveFrom <= endDate &&
                (!x.EffectiveTo.HasValue || x.EffectiveTo.Value >= dto.EffectiveFrom), ct))
            return Result.Failure(HrErrors.Conflict(
                "ScheduleOverlap",
                "The employee already has a work schedule covering part of this period."));

        var employmentsQuery = _queryBuilder.For<PayEmployment>()
            .Where(x =>
                x.EmployeeId == employeeId &&
                x.StateId == StateIdConst.ACTIVE &&
                x.StartDate <= endDate &&
                (!x.EndDate.HasValue || x.EndDate.Value >= dto.EffectiveFrom))
            .As(x => x.WeeklyHours)
            .Build();
        var employmentWeeklyHours = await _employmentQuery.GetAllAsync(employmentsQuery, ct);
        if (employmentWeeklyHours.Count == 0)
            return Result.Failure(HrErrors.Business(
                "NoEmployment",
                "The employee has no active employment covering the schedule period."));
        var scheduleWeeklyHours = dto.Days.Sum(x => x.WorkHours);
        if (employmentWeeklyHours.Any(hours => Math.Abs(hours - scheduleWeeklyHours) > 0.01m))
            return Result.Failure(HrErrors.Business(
                "WeeklyHoursMismatch",
                $"Schedule weekly hours ({scheduleWeeklyHours}) must match employment weekly hours."));

        return Result.Success();
    }

    private Task<bool> IsLockedByPostedTimesheetAsync(
        long employeeId,
        DateOnly dateFrom,
        DateOnly? dateTo,
        CancellationToken ct) =>
        _timesheetLineQuery.AnyAsync(x =>
            x.EmployeeId == employeeId &&
            x.Timesheet.StatusId == DocumentStatusIdConst.POSTED &&
            x.Timesheet.Period.StartDate <= (dateTo ?? DateOnly.MaxValue) &&
            x.Timesheet.Period.EndDate >= dateFrom, ct);

    private static List<HrEmployeeWorkScheduleDay> BuildDays(
        int organizationId,
        IEnumerable<HrWorkScheduleDaySaveDto> days) =>
        days.Select(x => new HrEmployeeWorkScheduleDay
        {
            OrganizationId = organizationId,
            DayOfWeek = x.DayOfWeek,
            WorkHours = x.WorkHours
        }).ToList();

    private static HrWorkScheduleDto Map(HrEmployeeWorkSchedule entity) =>
        new()
        {
            Id = entity.Id,
            EmployeeId = entity.EmployeeId,
            Name = entity.Name,
            EffectiveFrom = entity.EffectiveFrom,
            EffectiveTo = entity.EffectiveTo,
            StateId = entity.StateId,
            CreatedDate = entity.CreatedDate,
            UpdatedDate = entity.UpdatedDate,
            Days = entity.Days
                .OrderBy(x => x.DayOfWeek)
                .Select(x => new HrWorkScheduleDaySaveDto
                {
                    DayOfWeek = x.DayOfWeek,
                    WorkHours = x.WorkHours
                })
                .ToList(),
            WeeklyHours = entity.Days.Sum(x => x.WorkHours)
        };
}
