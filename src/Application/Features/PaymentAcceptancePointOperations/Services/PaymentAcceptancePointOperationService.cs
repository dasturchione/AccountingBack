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

namespace Application.Features.PaymentAcceptancePointOperations;

public sealed class PaymentAcceptancePointOperationService : BaseService, IPaymentAcceptancePointOperationService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IAuditLogService _auditLogService;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IPaymentAcceptancePointOperationLifecycleService _lifecycleService;
    private readonly IPaymentAcceptancePointMoneyRegisterService _moneyRegisterService;
    private readonly IQueryRepository<PaymentAcceptancePointOperation> _query;
    private readonly ICommandRepository<PaymentAcceptancePointOperation> _command;
    private readonly IQueryRepository<PaymentAcceptancePoint> _pointQuery;
    private readonly IQueryRepository<Currency> _currencyQuery;

    public PaymentAcceptancePointOperationService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IAuditLogService auditLogService,
        IDocumentNumberService documentNumberService,
        IPaymentAcceptancePointOperationLifecycleService lifecycleService,
        IPaymentAcceptancePointMoneyRegisterService moneyRegisterService,
        IQueryRepository<PaymentAcceptancePointOperation> query,
        ICommandRepository<PaymentAcceptancePointOperation> command,
        IQueryRepository<PaymentAcceptancePoint> pointQuery,
        IQueryRepository<Currency> currencyQuery,
        ILogger<PaymentAcceptancePointOperationService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _auditLogService = auditLogService;
        _documentNumberService = documentNumberService;
        _lifecycleService = lifecycleService;
        _moneyRegisterService = moneyRegisterService;
        _query = query;
        _command = command;
        _pointQuery = pointQuery;
        _currencyQuery = currencyQuery;
    }

    public Task<Result<PagedResponse<PaymentAcceptancePointOperationListDto>>> GetAllAsync(
        PaymentAcceptancePointOperationListFilter filter,
        CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var query = _queryBuilder.BuildPaged<
                PaymentAcceptancePointOperation,
                PaymentAcceptancePointOperationListDto,
                PaymentAcceptancePointOperationListFilter>(filter);
            var page = await _query.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(page, filter.Page, filter.PageSize));
        });

    public Task<Result<PaymentAcceptancePointOperationDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            if (_userContext.OrganizationId is not int organizationId)
                return Result.Failure<PaymentAcceptancePointOperationDto>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<PaymentAcceptancePointOperation>()
                .Where(x => x.Id == id && x.OrganizationId == organizationId)
                .As<PaymentAcceptancePointOperationDto>()
                .Build();
            var dto = await _query.GetAsync(query, ct);
            return dto is null
                ? Result.Failure<PaymentAcceptancePointOperationDto>(PaymentAcceptancePointOperationErrors.NotFound(id, _userContext.LanguageId))
                : Result.Success(dto);
        });

    public Task<Result<long>> CreateAsync(PaymentAcceptancePointOperationCreateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is not int organizationId)
                return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var validation = await ValidateReferencesAsync(dto, organizationId, ct);
            if (!validation.IsSuccess)
                return Result.Failure<long>(validation.Error);

            var number = await _documentNumberService.GetNextAsync(
                organizationId,
                DocumentTypeIdConst.PAYMENT_ACCEPTANCE_POINT_OPERATION,
                dto.DocDate,
                ct);
            if (!number.IsSuccess)
                return Result.Failure<long>(number.Error);

            var entity = new PaymentAcceptancePointOperation
            {
                OrganizationId = organizationId,
                PaymentAcceptancePointId = dto.PaymentAcceptancePointId,
                DirectionId = dto.DirectionId,
                DocNumber = number.Value.DocumentNumber,
                DocDate = number.Value.DocumentDate,
                CurrencyId = dto.CurrencyId,
                Amount = dto.Amount,
                ExchangeRate = dto.ExchangeRate,
                ExternalTransactionNumber = Normalize(dto.ExternalTransactionNumber),
                Comment = Normalize(dto.Comment),
                StatusId = DocumentStatusIdConst.DRAFT,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.Now
            };

            await _command.CreateAsync(entity, ct);
            await _auditLogService.CreateAsync(
                AuditLogTableConst.PaymentAcceptancePointOperation,
                entity.Id.ToString(),
                AuditLogOperationTypeConst.Create);
            return Result.Success(entity.Id);
        }, ct);

    public Task<Result> UpdateAsync(long id, PaymentAcceptancePointOperationUpdateDto dto, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is not int organizationId)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var entity = await GetEntityAsync(id, ct);
            if (entity is null)
                return Result.Failure(PaymentAcceptancePointOperationErrors.NotFound(id, _userContext.LanguageId));
            if (entity.OrganizationId != organizationId)
                return Result.Failure(PaymentAcceptancePointOperationErrors.OrganizationMismatch(id, _userContext.LanguageId));
            if (entity.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(PaymentAcceptancePointOperationErrors.InvalidStatus(id, entity.StatusId, "updated", _userContext.LanguageId));

            var validation = await ValidateReferencesAsync(dto, organizationId, ct);
            if (!validation.IsSuccess)
                return validation;

            entity.PaymentAcceptancePointId = dto.PaymentAcceptancePointId;
            entity.DirectionId = dto.DirectionId;
            entity.DocDate = dto.DocDate;
            entity.CurrencyId = dto.CurrencyId;
            entity.Amount = dto.Amount;
            entity.ExchangeRate = dto.ExchangeRate;
            entity.ExternalTransactionNumber = Normalize(dto.ExternalTransactionNumber);
            entity.Comment = Normalize(dto.Comment);
            await _command.UpdateAsync(entity, ct);

            await _auditLogService.CreateAsync(
                AuditLogTableConst.PaymentAcceptancePointOperation,
                id.ToString(),
                AuditLogOperationTypeConst.Update,
                dto.Comment);
            return Result.Success();
        }, ct);

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
        {
            if (_userContext.OrganizationId is not int organizationId)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var entity = await GetEntityAsync(id, ct);
            if (entity is null)
                return Result.Failure(PaymentAcceptancePointOperationErrors.NotFound(id, _userContext.LanguageId));
            if (entity.OrganizationId != organizationId)
                return Result.Failure(PaymentAcceptancePointOperationErrors.OrganizationMismatch(id, _userContext.LanguageId));
            if (entity.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(PaymentAcceptancePointOperationErrors.InvalidStatus(id, entity.StatusId, "deleted", _userContext.LanguageId));

            entity.StateId = StateIdConst.PASSIVE;
            await _command.UpdateAsync(entity, ct);
            await _auditLogService.CreateAsync(
                AuditLogTableConst.PaymentAcceptancePointOperation,
                id.ToString(),
                AuditLogOperationTypeConst.Delete);
            return Result.Success();
        }, ct);

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        _lifecycleService.ConfirmAsync(id, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        _lifecycleService.CancelAsync(id, ct);

    public Task<Result<PaymentAcceptancePointBalanceDto>> GetBalanceAsync(
        int paymentAcceptancePointId,
        short currencyId,
        DateTime? asOfDate,
        CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetBalanceAsync), async () =>
        {
            if (_userContext.OrganizationId is not int organizationId)
                return Result.Failure<PaymentAcceptancePointBalanceDto>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            if (!await _pointQuery.AnyAsync(x =>
                    x.Id == paymentAcceptancePointId &&
                    x.OrganizationId == organizationId &&
                    x.StateId == StateIdConst.ACTIVE, ct))
                return Result.Failure<PaymentAcceptancePointBalanceDto>(
                    PaymentAcceptancePointOperationErrors.PointNotFound(paymentAcceptancePointId, _userContext.LanguageId));

            if (!await _currencyQuery.AnyAsync(x => x.Id == currencyId && x.StateId == StateIdConst.ACTIVE, ct))
                return Result.Failure<PaymentAcceptancePointBalanceDto>(
                    PaymentAcceptancePointOperationErrors.CurrencyNotFound(currencyId, _userContext.LanguageId));

            var effectiveDate = asOfDate ?? DateTime.Now;
            var balance = await _moneyRegisterService.GetBalanceAsync(
                paymentAcceptancePointId,
                currencyId,
                effectiveDate,
                ct);
            return Result.Success(new PaymentAcceptancePointBalanceDto
            {
                PaymentAcceptancePointId = paymentAcceptancePointId,
                CurrencyId = currencyId,
                AsOfDate = effectiveDate,
                Balance = balance
            });
        });

    private async Task<PaymentAcceptancePointOperation?> GetEntityAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PaymentAcceptancePointOperation>()
            .Where(x => x.Id == id)
            .Build();
        return await _query.GetAsync(query, ct);
    }

    private async Task<Result> ValidateReferencesAsync(
        PaymentAcceptancePointOperationBaseDto dto,
        int organizationId,
        CancellationToken ct)
    {
        if (!MovementDirectionIdConst.IsValid(dto.DirectionId) || dto.Amount <= 0m || dto.ExchangeRate <= 0m)
            return Result.Failure(PaymentAcceptancePointOperationErrors.InvalidValues(
                _userContext.LanguageId));

        if (!await _pointQuery.AnyAsync(x =>
                x.Id == dto.PaymentAcceptancePointId &&
                x.OrganizationId == organizationId &&
                x.StateId == StateIdConst.ACTIVE, ct))
            return Result.Failure(PaymentAcceptancePointOperationErrors.PointNotFound(
                dto.PaymentAcceptancePointId,
                _userContext.LanguageId));

        if (!await _currencyQuery.AnyAsync(x => x.Id == dto.CurrencyId && x.StateId == StateIdConst.ACTIVE, ct))
            return Result.Failure(PaymentAcceptancePointOperationErrors.CurrencyNotFound(
                dto.CurrencyId,
                _userContext.LanguageId));

        return Result.Success();
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
