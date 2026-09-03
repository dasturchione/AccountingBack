using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;
using SharedKernel.Time;

namespace Application.Features.Rnt.RentalAccruals;

public sealed class RentalAccrualService : BaseService, IRentalAccrualService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly IRentalAccrualLifecycleService _lifecycleService;
    private readonly IRentalAccrualGenerationService _generationService;
    private readonly IQueryRepository<RentalAccrualDoc> _query;
    private readonly ICommandRepository<RentalAccrualDoc> _command;
    private readonly IQueryRepository<ChartAccount> _accountQuery;
    private readonly ICommandRepository<RentalContractObject> _contractObjectCommand;

    public RentalAccrualService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IAuditLogService auditLogService,
        IRentalAccrualLifecycleService lifecycleService,
        IRentalAccrualGenerationService generationService,
        IQueryRepository<RentalAccrualDoc> query,
        ICommandRepository<RentalAccrualDoc> command,
        IQueryRepository<ChartAccount> accountQuery,
        ICommandRepository<RentalContractObject> contractObjectCommand,
        ILogger<RentalAccrualService> logger,
        IUnitOfWork unitOfWork) : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _auditLogService = auditLogService;
        _lifecycleService = lifecycleService;
        _generationService = generationService;
        _query = query;
        _command = command;
        _accountQuery = accountQuery;
        _contractObjectCommand = contractObjectCommand;
    }

    public Task<Result<PagedResponse<RentalAccrualDocListDto>>> GetAllAsync(RentalAccrualListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<PagedResponse<RentalAccrualDocListDto>>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));
            var query = _queryBuilder.BuildPaged<RentalAccrualDoc, RentalAccrualDocListDto, RentalAccrualListFilter>(filter);
            var page = await _query.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(page, filter.Page, filter.PageSize));
        });

    public Task<Result<RentalAccrualDocDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var dto = await GetDtoAsync(id, ct);
            return dto is null
                ? Result.Failure<RentalAccrualDocDto>(RentalAccrualErrors.NotFound(id, _userContext.LanguageId))
                : Result.Success(dto);
        });

    public Task<Result> UpdateAsync(long id, RentalAccrualUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            var document = await GetEntityAsync(id, ct);
            if (document is null)
                return Result.Failure(RentalAccrualErrors.NotFound(id, _userContext.LanguageId));
            if (document.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(RentalAccrualErrors.InvalidStatus(id, document.StatusId, _userContext.LanguageId));

            var itemIds = document.Items.Select(x => x.Id).OrderBy(x => x).ToArray();
            var submittedIds = dto.Items.Select(x => x.ItemId).OrderBy(x => x).ToArray();
            if (!itemIds.SequenceEqual(submittedIds))
                return Result.Failure(RentalAccrualErrors.ItemMismatch(_userContext.LanguageId));

            var accountIds = dto.Items.Select(x => x.ExpenseAccountId)
                .Append(dto.LessorPayableAccountId)
                .Append(dto.TaxPayableAccountId)
                .Distinct()
                .ToArray();
            if (!await AccountsAreValidAsync(document.OrganizationId, accountIds, ct))
                return Result.Failure(RentalAccrualErrors.InvalidAccounts(_userContext.LanguageId));
            if (dto.LessorPayableAccountId == dto.TaxPayableAccountId ||
                dto.Items.Any(x => x.ExpenseAccountId == dto.LessorPayableAccountId || x.ExpenseAccountId == dto.TaxPayableAccountId))
                return Result.Failure(RentalAccrualErrors.InvalidAccounts(_userContext.LanguageId));

            var old = await GetDtoAsync(id, ct);
            if (old is not null)
                _auditLogService.SetOldValues(old);

            document.ExchangeRate = dto.ExchangeRate;
            document.LessorPayableAccountId = dto.LessorPayableAccountId;
            document.TaxPayableAccountId = dto.TaxPayableAccountId;
            document.Comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim();
            foreach (var item in document.Items)
                item.ExpenseAccountId = dto.Items.Single(x => x.ItemId == item.Id).ExpenseAccountId;
            document.UpdatedDate = DateTime.Now;
            document.UpdatedByUserId = _userContext.Id;
            await _command.UpdateAsync(document, ct);

            var updated = await GetDtoAsync(id, ct);
            if (updated is not null)
            {
                _auditLogService.SetNewValues(updated);
                await _auditLogService.CreateAsync(AuditLogTableConst.RentalAccrual, id.ToString(), AuditLogOperationTypeConst.Update, dto.Comment);
            }
            return Result.Success();
        }, ct);

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
        {
            var document = await GetEntityAsync(id, ct);
            if (document is null)
                return Result.Failure(RentalAccrualErrors.NotFound(id, _userContext.LanguageId));
            if (document.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(RentalAccrualErrors.InvalidStatus(id, document.StatusId, _userContext.LanguageId));
            var old = await GetDtoAsync(id, ct);
            if (old is not null)
                _auditLogService.SetOldValues(old);
            foreach (var group in document.Items.GroupBy(x => x.ContractObject))
            {
                var earliestDeletedPeriod = group.Min(x => x.PeriodFrom).Date;
                if (earliestDeletedPeriod < group.Key.NextAccrualDate)
                    group.Key.NextAccrualDate = earliestDeletedPeriod;
                group.Key.UpdatedDate = DateTime.Now;
            }
            await _contractObjectCommand.UpdateAsync(document.Items.Select(x => x.ContractObject).Distinct(), ct);
            await _auditLogService.CreateAsync(AuditLogTableConst.RentalAccrual, id.ToString(), AuditLogOperationTypeConst.Delete);
            await _command.DeleteAsync(document, ct);
            return Result.Success();
        }, ct);

    public Task<Result> PostAsync(long id, CancellationToken ct = default) => _lifecycleService.PostAsync(id, ct);
    public Task<Result> CancelAsync(long id, CancellationToken ct = default) => _lifecycleService.CancelAsync(id, ct);

    public Task<Result<RentalAccrualGenerationResult>> GenerateDueAsync(DateTime? asOfDate, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GenerateDueAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<RentalAccrualGenerationResult>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));
            return await _generationService.GenerateDueAsync(asOfDate?.Date ?? TashkentTime.Today, _userContext.OrganizationId.Value, ct);
        });

    private async Task<RentalAccrualDoc?> GetEntityAsync(long id, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return null;
        var organizationId = _userContext.OrganizationId.Value;
        var query = _queryBuilder.For<RentalAccrualDoc>()
            .Where(x => x.Id == id && x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE)
            .Build();
        query.AddIncludes(x => x.Include(d => d.Items).ThenInclude(i => i.ContractObject));
        return await _query.GetAsync(query, ct);
    }

    private async Task<RentalAccrualDocDto?> GetDtoAsync(long id, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return null;
        var organizationId = _userContext.OrganizationId.Value;
        var query = _queryBuilder.For<RentalAccrualDoc>()
            .Where(x => x.Id == id && x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE)
            .As<RentalAccrualDocDto>()
            .Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task<bool> AccountsAreValidAsync(int organizationId, int[] accountIds, CancellationToken ct)
    {
        var query = _queryBuilder.For<ChartAccount>()
            .Where(x => accountIds.Contains(x.Id) && x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE)
            .As(x => x.Id)
            .Build();
        return (await _accountQuery.GetAllAsync(query, ct)).Count == accountIds.Length;
    }
}
