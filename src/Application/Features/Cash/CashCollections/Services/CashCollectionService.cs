using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Application.Features.DocumentNumbers;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.CashCollections;

public sealed class CashCollectionService : BaseService, ICashCollectionService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly ICashCollectionLifecycleService _lifecycleService;
    private readonly IQueryRepository<CashCollectionDoc> _query;
    private readonly ICommandRepository<CashCollectionDoc> _command;

    public CashCollectionService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IAuditLogService auditLogService,
        IDocumentNumberService documentNumberService,
        ICashCollectionLifecycleService lifecycleService,
        IQueryRepository<CashCollectionDoc> query,
        ICommandRepository<CashCollectionDoc> command,
        ILogger<CashCollectionService> logger,
        IUnitOfWork unitOfWork) : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _auditLogService = auditLogService;
        _documentNumberService = documentNumberService;
        _lifecycleService = lifecycleService;
        _query = query;
        _command = command;
    }

    public Task<Result<PagedResponse<CashCollectionListDto>>> GetAllAsync(CashCollectionListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var query = _queryBuilder.BuildPaged<CashCollectionDoc, CashCollectionListDto, CashCollectionListFilter>(filter);
            var page = await _query.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(page, filter.Page, filter.PageSize));
        });

    public Task<Result<CashCollectionDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var dto = await GetDtoAsync(id, ct);
            return dto is null
                ? Result.Failure<CashCollectionDto>(CashCollectionErrors.NotFound(id))
                : Result.Success(dto);
        });

    public Task<Result<List<CashCollectionInTransitDto>>> GetInTransitAsync(int? bankAccountId, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetInTransitAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<List<CashCollectionInTransitDto>>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var organizationId = _userContext.OrganizationId.Value;
            var query = _queryBuilder.For<CashCollectionDoc>()
                .Where(x => x.OrganizationId == organizationId &&
                            x.StateId == StateIdConst.ACTIVE &&
                            x.StatusId == DocumentStatusIdConst.IN_TRANSIT &&
                            (!bankAccountId.HasValue || x.BankAccountId == bankAccountId.Value) &&
                            !x.BankOperations.Any(operation =>
                                operation.StateId == StateIdConst.ACTIVE &&
                                operation.StatusId != DocumentStatusIdConst.CANCELLED))
                .As<CashCollectionInTransitDto>()
                .OrderBy(items => items.OrderBy(x => x.DocDate).ThenBy(x => x.Id))
                .Build();
            return Result.Success(await _query.GetAllAsync(query, ct));
        });

    public Task<Result<long>> CreateAsync(CashCollectionCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var number = await _documentNumberService.GetNextAsync(
                _userContext.OrganizationId.Value,
                DocumentTypeIdConst.CASHCOLLECTION,
                dto.DocDate,
                ct);
            if (!number.IsSuccess)
                return Result.Failure<long>(number.Error);

            var entity = new CashCollectionDoc
            {
                OrganizationId = _userContext.OrganizationId.Value,
                DocNumber = number.Value.DocumentNumber,
                StatusId = DocumentStatusIdConst.DRAFT,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.Now
            };
            Apply(dto, entity);
            await _command.CreateAsync(entity, ct);

            var created = await GetDtoAsync(entity.Id, ct);
            if (created is not null)
            {
                _auditLogService.SetNewValues(created);
                await _auditLogService.CreateAsync(AuditLogTableConst.CashCollection, entity.Id.ToString(), AuditLogOperationTypeConst.Create);
            }

            return Result.Success(entity.Id);
        }, ct);

    public Task<Result> UpdateAsync(long id, CashCollectionUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            var entity = await GetEntityAsync(id, ct);
            if (entity is null)
                return Result.Failure(CashCollectionErrors.NotFound(id));
            if (entity.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(CashCollectionErrors.InvalidStatus(id, entity.StatusId));

            var old = await GetDtoAsync(id, ct);
            if (old is not null)
                _auditLogService.SetOldValues(old);

            Apply(dto, entity);
            await _command.UpdateAsync(entity, ct);

            var updated = await GetDtoAsync(id, ct);
            if (updated is not null)
            {
                _auditLogService.SetNewValues(updated);
                await _auditLogService.CreateAsync(AuditLogTableConst.CashCollection, id.ToString(), AuditLogOperationTypeConst.Update, dto.Comment);
            }

            return Result.Success();
        }, ct);

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
        {
            var entity = await GetEntityAsync(id, ct);
            if (entity is null)
                return Result.Failure(CashCollectionErrors.NotFound(id));
            if (entity.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(CashCollectionErrors.InvalidStatus(id, entity.StatusId));

            entity.StateId = StateIdConst.PASSIVE;
            await _command.UpdateAsync(entity, ct);
            await _auditLogService.CreateAsync(AuditLogTableConst.CashCollection, id.ToString(), AuditLogOperationTypeConst.Delete);
            return Result.Success();
        }, ct);

    public Task<Result> SendToBankAsync(long id, CancellationToken ct = default) =>
        _lifecycleService.SendToBankAsync(id, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        _lifecycleService.CancelAsync(id, ct);

    private async Task<CashCollectionDoc?> GetEntityAsync(long id, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return null;

        var organizationId = _userContext.OrganizationId.Value;
        var query = _queryBuilder.For<CashCollectionDoc>()
            .Where(x => x.Id == id && x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE)
            .Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task<CashCollectionDto?> GetDtoAsync(long id, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return null;

        var organizationId = _userContext.OrganizationId.Value;
        var query = _queryBuilder.For<CashCollectionDoc>()
            .Where(x => x.Id == id && x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE)
            .As<CashCollectionDto>()
            .Build();
        return await _query.GetAsync(query, ct);
    }

    private static void Apply(CashCollectionBaseDto dto, CashCollectionDoc entity)
    {
        entity.CashBoxId = dto.CashBoxId;
        entity.BankAccountId = dto.BankAccountId;
        entity.DocDate = dto.DocDate;
        entity.CurrencyId = dto.CurrencyId;
        entity.Amount = dto.Amount;
        entity.ExchangeRate = dto.ExchangeRate;
        entity.CashChartAccountId = dto.CashChartAccountId;
        entity.CashInTransitAccountId = dto.CashInTransitAccountId;
        entity.BankChartAccountId = dto.BankChartAccountId;
        entity.Comment = dto.Comment;
    }
}
