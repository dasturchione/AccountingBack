using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.AuditLogs;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Acc.AccountingPeriods;

public class AccountingPeriodService : BaseService, IAccountingPeriodService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly IAccountingPeriodReadRepository _readRepository;
    private readonly IQueryRepository<AccountingPeriod> _query;
    private readonly ICommandRepository<AccountingPeriod> _command;

    public AccountingPeriodService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IAuditLogService auditLogService,
        IAccountingPeriodReadRepository readRepository,
        IQueryRepository<AccountingPeriod> query,
        ICommandRepository<AccountingPeriod> command,
        ILogger<AccountingPeriodService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _auditLogService = auditLogService;
        _readRepository = readRepository;
        _query = query;
        _command = command;
    }

    public Task<Result> CloseAsync(int id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CloseAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var period = await GetPeriodAsync(id, _userContext.OrganizationId.Value, ct);
            if (period is null)
                return Result.Failure(AccountingPeriodErrors.NotFound(id, _userContext.LanguageId));

            if (period.IsClosed)
                return Result.Failure(AccountingPeriodErrors.AlreadyClosed(id, _userContext.LanguageId));

            if (await _readRepository.HasOpenPreviousPeriodsAsync(period.OrganizationId, period.StartDate, ct))
                return Result.Failure(AccountingPeriodErrors.PreviousPeriodsMustBeClosed(_userContext.LanguageId));

            var dateFrom = period.StartDate.ToDateTime(TimeOnly.MinValue);
            var dateTo = period.EndDate.ToDateTime(TimeOnly.MaxValue);

            var draftCount = await _readRepository.CountUnconfirmedDocumentsAsync(period.OrganizationId, dateFrom, dateTo, ct);
            if (draftCount > 0)
                return Result.Failure(AccountingPeriodErrors.DraftDocumentsExist(draftCount, _userContext.LanguageId));

            if (await _readRepository.HasInvalidPostingBatchStateAsync(period.OrganizationId, ct))
                return Result.Failure(AccountingPeriodErrors.InvalidPostingBatchState(_userContext.LanguageId));

            _auditLogService.SetOldValues(MapAuditDto(period));

            period.IsClosed = true;
            period.ClosedAt = DateTime.Now;
            period.ClosedByUserId = _userContext.Id;

            await _command.UpdateAsync(period, ct);

            _auditLogService.SetNewValues(MapAuditDto(period));
            await _auditLogService.CreateAsync(AuditLogTableConst.AccountingPeriod, id.ToString(), AuditLogOperationTypeConst.Update, "Closed");

            return Result.Success();
        }, ct);

    public Task<Result> ReopenAsync(int id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(ReopenAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var period = await GetPeriodAsync(id, _userContext.OrganizationId.Value, ct);
            if (period is null)
                return Result.Failure(AccountingPeriodErrors.NotFound(id, _userContext.LanguageId));

            if (!period.IsClosed)
                return Result.Failure(AccountingPeriodErrors.NotClosed(id, _userContext.LanguageId));

            if (await _readRepository.HasLaterClosedPeriodsAsync(period.OrganizationId, period.StartDate, ct))
                return Result.Failure(AccountingPeriodErrors.LaterClosedPeriodsExist(_userContext.LanguageId));

            _auditLogService.SetOldValues(MapAuditDto(period));

            period.IsClosed = false;
            period.ClosedAt = null;
            period.ClosedByUserId = null;

            await _command.UpdateAsync(period, ct);

            _auditLogService.SetNewValues(MapAuditDto(period));
            await _auditLogService.CreateAsync(AuditLogTableConst.AccountingPeriod, id.ToString(), AuditLogOperationTypeConst.Update, "Reopened");

            return Result.Success();
        }, ct);

    private async Task<AccountingPeriod?> GetPeriodAsync(int id, int organizationId, CancellationToken ct)
    {
        var query = _queryBuilder.For<AccountingPeriod>()
            .Where(x => x.Id == id && x.OrganizationId == organizationId)
            .Build();

        return await _query.GetAsync(query, ct);
    }

    private static AccountingPeriodAuditDto MapAuditDto(AccountingPeriod period) => new()
    {
        Id = period.Id,
        OrganizationId = period.OrganizationId,
        Year = period.Year,
        Month = period.Month,
        StartDate = period.StartDate,
        EndDate = period.EndDate,
        IsClosed = period.IsClosed,
        ClosedAt = period.ClosedAt,
        ClosedByUserId = period.ClosedByUserId
    };

    private sealed class AccountingPeriodAuditDto
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public short Year { get; set; }
        public short Month { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public bool IsClosed { get; set; }
        public DateTime? ClosedAt { get; set; }
        public int? ClosedByUserId { get; set; }
    }
}
