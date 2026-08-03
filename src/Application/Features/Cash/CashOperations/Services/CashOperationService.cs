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

namespace Application.Features.CashOperations;

public class CashOperationService : BaseService, ICashOperationService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly ICashLifecycleService _cashLifecycleService;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IQueryRepository<CashOperation> _query;
    private readonly ICommandRepository<CashOperation> _command;

    public CashOperationService(IUserContext userContext,
                               IQueryBuilder queryBuilder,
                               IAuditLogService auditLogService,
                               ICashLifecycleService cashLifecycleService,
                               IDocumentNumberService documentNumberService,
                               IQueryRepository<CashOperation> query,
                               ICommandRepository<CashOperation> command,
                               ILogger<CashOperationService> logger,
                               IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _queryBuilder = queryBuilder;
        _userContext = userContext;
        _auditLogService = auditLogService;
        _cashLifecycleService = cashLifecycleService;
        _documentNumberService = documentNumberService;
        _query = query;
        _command = command;
    }

    public Task<Result<long>> CreateAsync(CashOperationCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var documentNumberResult = await _documentNumberService.GetNextAsync(
                _userContext.OrganizationId.Value,
                DocumentTypeIdConst.CASHOPERATION,
                dto.DocDate,
                ct);
            if (!documentNumberResult.IsSuccess)
                return Result.Failure<long>(documentNumberResult.Error);

            var entity = new CashOperation
            {
                OrganizationId = _userContext.OrganizationId.Value,
                CashBoxId = dto.CashBoxId,
                DestinationCashBoxId = dto.DestinationCashBoxId,
                OperationTypeId = dto.OperationTypeId,
                PaymentTypeId = dto.PaymentTypeId,
                CashChartAccountId = dto.CashChartAccountId,
                OffsetAccountId = dto.OffsetAccountId,
                CounterpartyId = dto.CounterpartyId,
                DocNumber = documentNumberResult.Value.DocumentNumber,
                DocDate = dto.DocDate,
                CurrencyId = dto.CurrencyId,
                Amount = dto.Amount,
                ExchangeRate = dto.ExchangeRate == 0 ? 1m : dto.ExchangeRate,
                Comment = dto.Comment,
                StatusId = DocumentStatusIdConst.DRAFT,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.Now
            };

            await _command.CreateAsync(entity, ct);

            var docDto = await GetByIdInternalAsync(entity.Id, ct);
            if (docDto != null)
            {
                _auditLogService.SetNewValues(docDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.CashOperation, entity.Id.ToString(), AuditLogOperationTypeConst.Create);
            }

            return Result.Success(entity.Id);
        }, ct);

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<CashOperation>().Where(x => x.Id == id).Build();
            var entity = await _query.GetAsync(query, ct);
            if (entity == null)
                return Result.Failure(CashOperationErrors.NotFound(id, _userContext.LanguageId));

            if (entity.OrganizationId != _userContext.OrganizationId.Value)
                return Result.Failure(CashOperationErrors.OrganizationMismatch(id, _userContext.LanguageId));

            if (entity.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(CashOperationErrors.CannotDeleteInCurrentStatus(id, entity.StatusId, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            entity.StateId = StateIdConst.PASSIVE;
            await _command.UpdateAsync(entity, ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto != null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.CashOperation, id.ToString(), AuditLogOperationTypeConst.Delete);
            }

            return Result.Success();
        }, ct);

    public Task<Result<PagedResponse<CashOperationListDto>>> GetAllAsync(CashOperationListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var query = _queryBuilder.BuildPaged<CashOperation, CashOperationListDto, CashOperationListFilter>(filter);
            var pagedList = await _query.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
        });

    public Task<Result<CashOperationDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure<CashOperationDto>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<CashOperation>()
                .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
                .As<CashOperationDto>()
                .Build();
            var entity = await _query.GetAsync(query, ct);
            if (entity == null)
                return Result.Failure<CashOperationDto>(CashOperationErrors.NotFound(id, _userContext.LanguageId));
            return entity;
        });

    public Task<Result> UpdateAsync(long id, CashOperationUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is null)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<CashOperation>().Where(x => x.Id == id).Build();
            var entity = await _query.GetAsync(query, ct);
            if (entity == null)
                return Result.Failure(CashOperationErrors.NotFound(id, _userContext.LanguageId));

            if (entity.OrganizationId != _userContext.OrganizationId.Value)
                return Result.Failure(CashOperationErrors.OrganizationMismatch(id, _userContext.LanguageId));

            if (entity.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(CashOperationErrors.CannotUpdateInCurrentStatus(id, entity.StatusId, _userContext.LanguageId));

            var oldDocDto = await GetByIdInternalAsync(id, ct);
            if (oldDocDto != null)
                _auditLogService.SetOldValues(oldDocDto);

            entity.CashBoxId = dto.CashBoxId;
            entity.DestinationCashBoxId = dto.DestinationCashBoxId;
            entity.OperationTypeId = dto.OperationTypeId;
            entity.PaymentTypeId = dto.PaymentTypeId;
            entity.CashChartAccountId = dto.CashChartAccountId;
            entity.OffsetAccountId = dto.OffsetAccountId;
            entity.CounterpartyId = dto.CounterpartyId;
            entity.DocDate = dto.DocDate;
            entity.CurrencyId = dto.CurrencyId;
            entity.Amount = dto.Amount;
            entity.ExchangeRate = dto.ExchangeRate == 0 ? 1m : dto.ExchangeRate;
            entity.Comment = dto.Comment;

            await _command.UpdateAsync(entity, ct);

            var newDocDto = await GetByIdInternalAsync(id, ct);
            if (newDocDto != null)
            {
                _auditLogService.SetNewValues(newDocDto);
                await _auditLogService.CreateAsync(AuditLogTableConst.CashOperation, id.ToString(), AuditLogOperationTypeConst.Update, dto.Comment);
            }

            return Result.Success();
        }, ct);

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        _cashLifecycleService.ConfirmAsync(id, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        _cashLifecycleService.CancelAsync(id, ct);

    private async Task<CashOperationDto?> GetByIdInternalAsync(long id, CancellationToken ct)
    {
        if (_userContext.OrganizationId is null)
            return null;

        var query = _queryBuilder.For<CashOperation>()
            .Where(x => x.Id == id && x.OrganizationId == _userContext.OrganizationId.Value)
            .As<CashOperationDto>()
            .Build();
        return await _query.GetAsync(query, ct);
    }
}
