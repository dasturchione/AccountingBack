using Application.Abstractions;
using Application.Abstractions.Authentication;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Hr.Calendar;

public sealed class HrEmployeeCalendarService : BaseService, IHrEmployeeCalendarService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<PayEmployee> _employeeQuery;
    private readonly IQueryRepository<PayEmployment> _employmentQuery;
    private readonly IQueryRepository<HrEmployeeWorkSchedule> _scheduleQuery;
    private readonly IQueryRepository<HrAbsence> _absenceQuery;

    public HrEmployeeCalendarService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<PayEmployee> employeeQuery,
        IQueryRepository<PayEmployment> employmentQuery,
        IQueryRepository<HrEmployeeWorkSchedule> scheduleQuery,
        IQueryRepository<HrAbsence> absenceQuery,
        ILogger<HrEmployeeCalendarService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _employeeQuery = employeeQuery;
        _employmentQuery = employmentQuery;
        _scheduleQuery = scheduleQuery;
        _absenceQuery = absenceQuery;
    }

    public Task<Result<HrEmployeeCalendarDto>> GetAsync(
        long employeeId,
        DateOnly dateFrom,
        DateOnly dateTo,
        CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAsync), async () =>
        {
            var rangeValidation = ValidateRange(dateFrom, dateTo);
            if (!rangeValidation.IsSuccess)
                return Result.Failure<HrEmployeeCalendarDto>(rangeValidation.Error);

            var sourcesResult = await LoadSourcesAsync([employeeId], dateFrom, dateTo, ct);
            if (!sourcesResult.IsSuccess)
                return Result.Failure<HrEmployeeCalendarDto>(sourcesResult.Error);

            var sources = sourcesResult.Value;
            if (!sources.Employees.TryGetValue(employeeId, out var employee))
                return Result.Failure<HrEmployeeCalendarDto>(HrErrors.NotFound("Employee", employeeId));

            return Result.Success(Build(employee, sources, dateFrom, dateTo));
        });

    public Task<Result<Dictionary<long, HrEmployeeCalendarSummaryDto>>> GetSummariesAsync(
        IReadOnlyCollection<long> employeeIds,
        DateOnly dateFrom,
        DateOnly dateTo,
        CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetSummariesAsync), async () =>
        {
            var rangeValidation = ValidateRange(dateFrom, dateTo);
            if (!rangeValidation.IsSuccess)
                return Result.Failure<Dictionary<long, HrEmployeeCalendarSummaryDto>>(rangeValidation.Error);

            var ids = employeeIds.Distinct().ToList();
            if (ids.Count == 0)
                return Result.Success(new Dictionary<long, HrEmployeeCalendarSummaryDto>());

            var sourcesResult = await LoadSourcesAsync(ids, dateFrom, dateTo, ct);
            if (!sourcesResult.IsSuccess)
                return Result.Failure<Dictionary<long, HrEmployeeCalendarSummaryDto>>(sourcesResult.Error);

            var sources = sourcesResult.Value;
            var missingId = ids.FirstOrDefault(id => !sources.Employees.ContainsKey(id));
            if (missingId > 0)
                return Result.Failure<Dictionary<long, HrEmployeeCalendarSummaryDto>>(HrErrors.NotFound("Employee", missingId));

            var result = ids.ToDictionary(
                id => id,
                id => Build(sources.Employees[id], sources, dateFrom, dateTo).Summary);
            return Result.Success(result);
        });

    private async Task<Result<CalendarSources>> LoadSourcesAsync(
        IReadOnlyCollection<long> employeeIds,
        DateOnly dateFrom,
        DateOnly dateTo,
        CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure<CalendarSources>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var employeeQuery = _queryBuilder.For<PayEmployee>()
            .Where(x =>
                employeeIds.Contains(x.Id) &&
                x.StateId == StateIdConst.ACTIVE)
            .As(x => new EmployeeInfo(
                x.Id,
                x.EmployeeNumber,
                x.LastName + " " + x.FirstName + (x.MiddleName != null ? " " + x.MiddleName : "")))
            .Build();
        var employees = await _employeeQuery.GetAllAsync(employeeQuery, ct);

        var employmentQuery = _queryBuilder.For<PayEmployment>()
            .Where(x =>
                employeeIds.Contains(x.EmployeeId) &&
                x.StateId == StateIdConst.ACTIVE &&
                x.StartDate <= dateTo &&
                (!x.EndDate.HasValue || x.EndDate.Value >= dateFrom))
            .Build();
        var employments = await _employmentQuery.GetAllAsync(employmentQuery, ct);

        var scheduleQuery = _queryBuilder.For<HrEmployeeWorkSchedule>()
            .Where(x =>
                employeeIds.Contains(x.EmployeeId) &&
                x.StateId == StateIdConst.ACTIVE &&
                x.EffectiveFrom <= dateTo &&
                (!x.EffectiveTo.HasValue || x.EffectiveTo.Value >= dateFrom))
            .Build();
        scheduleQuery.AddIncludes(x => x.Include(schedule => schedule.Days));
        var schedules = await _scheduleQuery.GetAllAsync(scheduleQuery, ct);

        var absenceQuery = _queryBuilder.For<HrAbsence>()
            .Where(x =>
                employeeIds.Contains(x.EmployeeId) &&
                x.StateId == StateIdConst.ACTIVE &&
                x.StartDate <= dateTo &&
                x.EndDate >= dateFrom)
            .Build();
        absenceQuery.AddIncludes(x => x.Include(absence => absence.AbsenceType));
        var absences = await _absenceQuery.GetAllAsync(absenceQuery, ct);

        return Result.Success(new CalendarSources(
            employees.ToDictionary(x => x.Id),
            employments.GroupBy(x => x.EmployeeId).ToDictionary(x => x.Key, x => x.ToList()),
            schedules.GroupBy(x => x.EmployeeId).ToDictionary(x => x.Key, x => x.ToList()),
            absences.GroupBy(x => x.EmployeeId).ToDictionary(x => x.Key, x => x.ToList())));
    }

    private static HrEmployeeCalendarDto Build(
        EmployeeInfo employee,
        CalendarSources sources,
        DateOnly dateFrom,
        DateOnly dateTo)
    {
        sources.Employments.TryGetValue(employee.Id, out var employments);
        sources.Schedules.TryGetValue(employee.Id, out var schedules);
        sources.Absences.TryGetValue(employee.Id, out var absences);

        return HrEmployeeCalendarCalculator.Build(
            employee.Id,
            employee.EmployeeNumber,
            employee.Name,
            dateFrom,
            dateTo,
            DateOnly.FromDateTime(DateTime.Today),
            employments ?? [],
            schedules ?? [],
            absences ?? []);
    }

    private static Result ValidateRange(DateOnly dateFrom, DateOnly dateTo)
    {
        if (dateTo < dateFrom)
            return Result.Failure(HrErrors.Business("InvalidCalendarRange", "Tugash sanasi boshlanish sanasidan oldin bo‘lishi mumkin emas."));
        if (dateTo.DayNumber - dateFrom.DayNumber > 731)
            return Result.Failure(HrErrors.Business("CalendarRangeTooLarge", "Kalendar davri ikki yildan oshmasligi kerak."));
        return Result.Success();
    }

    private sealed record EmployeeInfo(long Id, string EmployeeNumber, string Name);

    private sealed record CalendarSources(
        Dictionary<long, EmployeeInfo> Employees,
        Dictionary<long, List<PayEmployment>> Employments,
        Dictionary<long, List<HrEmployeeWorkSchedule>> Schedules,
        Dictionary<long, List<HrAbsence>> Absences);
}
