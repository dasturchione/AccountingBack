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

namespace Application.Features.CashFiscalTransfers;

public sealed class CashFiscalTransferService : BaseService, ICashFiscalTransferService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly ICashFiscalTransferLifecycleService _lifecycleService;
    private readonly IQueryRepository<CashFiscalTransferDoc> _query;
    private readonly ICommandRepository<CashFiscalTransferDoc> _command;

    public CashFiscalTransferService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IAuditLogService auditLogService,
        IDocumentNumberService documentNumberService,
        ICashFiscalTransferLifecycleService lifecycleService,
        IQueryRepository<CashFiscalTransferDoc> query,
        ICommandRepository<CashFiscalTransferDoc> command,
        ILogger<CashFiscalTransferService> logger,
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

    public Task<Result<PagedResponse<CashFiscalTransferListDto>>> GetAllAsync(CashFiscalTransferListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var query = _queryBuilder.BuildPaged<CashFiscalTransferDoc, CashFiscalTransferListDto, CashFiscalTransferListFilter>(filter);
            var page = await _query.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(page, filter.Page, filter.PageSize));
        });

    public Task<Result<CashFiscalTransferDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var dto = await GetDtoAsync(id, ct);
            return dto is null
                ? Result.Failure<CashFiscalTransferDto>(CashFiscalTransferErrors.NotFound(id))
                : Result.Success(dto);
        });

    public Task<Result<long>> CreateAsync(CashFiscalTransferCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var number = await _documentNumberService.GetNextAsync(
                _userContext.OrganizationId.Value,
                DocumentTypeIdConst.CASHFISCALTRANSFER,
                dto.DocDate,
                ct);
            if (!number.IsSuccess)
                return Result.Failure<long>(number.Error);

            var entity = new CashFiscalTransferDoc
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
                await _auditLogService.CreateAsync(AuditLogTableConst.CashFiscalTransfer, entity.Id.ToString(), AuditLogOperationTypeConst.Create);
            }

            return Result.Success(entity.Id);
        }, ct);

    public Task<Result> UpdateAsync(long id, CashFiscalTransferUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            var entity = await GetEntityAsync(id, ct);
            if (entity is null)
                return Result.Failure(CashFiscalTransferErrors.NotFound(id));
            if (entity.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(CashFiscalTransferErrors.InvalidStatus(id, entity.StatusId));

            var old = await GetDtoAsync(id, ct);
            if (old is not null) _auditLogService.SetOldValues(old);

            Apply(dto, entity);
            await _command.UpdateAsync(entity, ct);

            var updated = await GetDtoAsync(id, ct);
            if (updated is not null)
            {
                _auditLogService.SetNewValues(updated);
                await _auditLogService.CreateAsync(AuditLogTableConst.CashFiscalTransfer, id.ToString(), AuditLogOperationTypeConst.Update, dto.Comment);
            }
            return Result.Success();
        }, ct);

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
        {
            var entity = await GetEntityAsync(id, ct);
            if (entity is null)
                return Result.Failure(CashFiscalTransferErrors.NotFound(id));
            if (entity.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(CashFiscalTransferErrors.InvalidStatus(id, entity.StatusId));

            entity.StateId = StateIdConst.PASSIVE;
            await _command.UpdateAsync(entity, ct);
            await _auditLogService.CreateAsync(AuditLogTableConst.CashFiscalTransfer, id.ToString(), AuditLogOperationTypeConst.Delete);
            return Result.Success();
        }, ct);

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) => _lifecycleService.ConfirmAsync(id, ct);
    public Task<Result> CancelAsync(long id, CancellationToken ct = default) => _lifecycleService.CancelAsync(id, ct);

    private async Task<CashFiscalTransferDoc?> GetEntityAsync(long id, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null) return null;
        var query = _queryBuilder.For<CashFiscalTransferDoc>()
            .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value && x.StateId == StateIdConst.ACTIVE)
            .Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task<CashFiscalTransferDto?> GetDtoAsync(long id, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null) return null;
        var query = _queryBuilder.For<CashFiscalTransferDoc>()
            .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value && x.StateId == StateIdConst.ACTIVE)
            .As<CashFiscalTransferDto>()
            .Build();
        return await _query.GetAsync(query, ct);
    }

    private static void Apply(CashFiscalTransferBaseDto dto, CashFiscalTransferDoc entity)
    {
        entity.FiscalCashRegisterId = dto.FiscalCashRegisterId;
        entity.CashBoxId = dto.CashBoxId;
        entity.DirectionId = dto.DirectionId;
        entity.DocDate = dto.DocDate;
        entity.CurrencyId = dto.CurrencyId;
        entity.Amount = dto.Amount;
        entity.ExchangeRate = dto.ExchangeRate;
        entity.FiscalCashAccountId = dto.FiscalCashAccountId;
        entity.CashBoxAccountId = dto.CashBoxAccountId;
        entity.Comment = dto.Comment;
    }
}
