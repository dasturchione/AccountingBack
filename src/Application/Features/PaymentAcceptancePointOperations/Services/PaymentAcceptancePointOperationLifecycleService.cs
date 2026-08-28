using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Acc.AccountingPeriods;
using Application.Features.AuditLogs;
using Application.Features.Register;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.PaymentAcceptancePointOperations;

public sealed class PaymentAcceptancePointOperationLifecycleService
    : BaseService, IPaymentAcceptancePointOperationLifecycleService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IDocumentPostingLock _postingLock;
    private readonly IAccountingPeriodValidator _periodValidator;
    private readonly IAuditLogService _auditLogService;
    private readonly IPaymentAcceptancePointMoneyRegisterService _moneyRegisterService;
    private readonly IQueryRepository<PaymentAcceptancePointOperation> _query;
    private readonly ICommandRepository<PaymentAcceptancePointOperation> _command;
    private readonly IQueryRepository<PostingBatch> _batchQuery;
    private readonly ICommandRepository<PostingBatch> _batchCommand;
    private readonly IQueryRepository<MoneyRegisterBalance> _moneyQuery;

    public PaymentAcceptancePointOperationLifecycleService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IDocumentPostingLock postingLock,
        IAccountingPeriodValidator periodValidator,
        IAuditLogService auditLogService,
        IPaymentAcceptancePointMoneyRegisterService moneyRegisterService,
        IQueryRepository<PaymentAcceptancePointOperation> query,
        ICommandRepository<PaymentAcceptancePointOperation> command,
        IQueryRepository<PostingBatch> batchQuery,
        ICommandRepository<PostingBatch> batchCommand,
        IQueryRepository<MoneyRegisterBalance> moneyQuery,
        ILogger<PaymentAcceptancePointOperationLifecycleService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _postingLock = postingLock;
        _periodValidator = periodValidator;
        _auditLogService = auditLogService;
        _moneyRegisterService = moneyRegisterService;
        _query = query;
        _command = command;
        _batchQuery = batchQuery;
        _batchCommand = batchCommand;
        _moneyQuery = moneyQuery;
    }

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(ConfirmAsync), async () =>
        {
            if (_userContext.OrganizationId is not int organizationId)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.PAYMENT_ACCEPTANCE_POINT_OPERATION, id, ct);
            var operation = await GetOperationAsync(id, organizationId, ct);
            if (operation is null)
                return Result.Failure(PaymentAcceptancePointOperationErrors.NotFound(id, _userContext.LanguageId));
            if (operation.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Failure(PaymentAcceptancePointOperationErrors.InvalidStatus(id, operation.StatusId, "confirmed"));
            if (operation.StatusId == DocumentStatusIdConst.POSTED)
                return await GetActiveBatchAsync(id, ct) is not null
                    ? Result.Success()
                    : Result.Failure(PaymentAcceptancePointOperationErrors.MissingPostingBatch(id));
            if (operation.StatusId != DocumentStatusIdConst.DRAFT)
                return Result.Failure(PaymentAcceptancePointOperationErrors.InvalidStatus(id, operation.StatusId, "confirmed"));

            var period = await _periodValidator.EnsureOpenAsync(operation.OrganizationId, operation.DocDate, ct);
            if (!period.IsSuccess)
                return period;

            var validation = ValidateConfiguration(operation);
            if (!validation.IsSuccess)
                return validation;

            await _postingLock.AcquireMoneyAsync(
                operation.OrganizationId,
                RegisterDefaultsConst.PaymentAcceptancePoint,
                operation.PaymentAcceptancePointId,
                ct);

            if (operation.DirectionId == MovementDirectionIdConst.OUT)
            {
                var balance = await _moneyRegisterService.GetBalanceAsync(
                    operation.PaymentAcceptancePointId,
                    operation.CurrencyId,
                    operation.DocDate,
                    ct);
                if (balance < operation.Amount)
                    return Result.Failure(PaymentAcceptancePointOperationErrors.InsufficientBalance(balance, operation.Amount));
            }

            if (await GetActiveBatchAsync(id, ct) is not null || await HasEffectsAsync(id, ct))
                return Result.Failure(PaymentAcceptancePointOperationErrors.BusinessEffectsAlreadyExist(id));

            var batch = await CreateBatchAsync(operation, PostingBatchStatusConst.POSTED, "Payment acceptance point operation confirmed", ct);
            var money = await _moneyRegisterService.PostAsync(operation, batch.Id, ct);
            if (!money.IsSuccess)
                return Result.Failure(money.Error);

            operation.StatusId = DocumentStatusIdConst.POSTED;
            operation.PostedAt = DateTime.Now;
            operation.PostedByUserId = _userContext.Id;
            await _command.UpdateAsync(operation, ct);
            await _auditLogService.CreateAsync(
                AuditLogTableConst.PaymentAcceptancePointOperation,
                id.ToString(),
                AuditLogOperationTypeConst.Update,
                "Confirmed");
            return Result.Success();
        }, ct);

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CancelAsync), async () =>
        {
            if (_userContext.OrganizationId is not int organizationId)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            await _postingLock.AcquireAsync(DocumentTypeIdConst.PAYMENT_ACCEPTANCE_POINT_OPERATION, id, ct);
            var operation = await GetOperationAsync(id, organizationId, ct);
            if (operation is null)
                return Result.Failure(PaymentAcceptancePointOperationErrors.NotFound(id, _userContext.LanguageId));
            if (operation.StatusId == DocumentStatusIdConst.CANCELLED)
                return Result.Success();
            if (operation.StatusId is not (DocumentStatusIdConst.DRAFT or DocumentStatusIdConst.POSTED))
                return Result.Failure(PaymentAcceptancePointOperationErrors.InvalidStatus(id, operation.StatusId, "cancelled"));

            if (operation.StatusId == DocumentStatusIdConst.POSTED)
            {
                var originalPeriod = await _periodValidator.EnsureOpenAsync(operation.OrganizationId, operation.DocDate, ct);
                if (!originalPeriod.IsSuccess)
                    return originalPeriod;
                var reversalPeriod = await _periodValidator.EnsureOpenAsync(operation.OrganizationId, DateTime.Now, ct);
                if (!reversalPeriod.IsSuccess)
                    return reversalPeriod;

                await _postingLock.AcquireMoneyAsync(
                    operation.OrganizationId,
                    RegisterDefaultsConst.PaymentAcceptancePoint,
                    operation.PaymentAcceptancePointId,
                    ct);

                if (operation.DirectionId == MovementDirectionIdConst.IN)
                {
                    var balance = await _moneyRegisterService.GetBalanceAsync(
                        operation.PaymentAcceptancePointId,
                        operation.CurrencyId,
                        DateTime.Now,
                        ct);
                    if (balance < operation.Amount)
                        return Result.Failure(PaymentAcceptancePointOperationErrors.InsufficientBalance(balance, operation.Amount));
                }

                var activeBatch = await GetActiveBatchAsync(id, ct);
                if (activeBatch is null)
                    return Result.Failure(PaymentAcceptancePointOperationErrors.MissingPostingBatch(id));

                var reversalBatch = await CreateBatchAsync(operation, PostingBatchStatusConst.REVERSAL, "Payment acceptance point operation cancelled", ct);
                var reversal = await _moneyRegisterService.ReverseAsync(operation, reversalBatch.Id, ct);
                if (!reversal.IsSuccess)
                    return Result.Failure(reversal.Error);

                activeBatch.Status = PostingBatchStatusConst.REVERSED;
                activeBatch.ReversedAt = DateTime.Now;
                activeBatch.ReversedByUserId = _userContext.Id;
                await _batchCommand.UpdateAsync(activeBatch, ct);
            }

            operation.StatusId = DocumentStatusIdConst.CANCELLED;
            operation.CancelledAt = DateTime.Now;
            operation.CancelledByUserId = _userContext.Id;
            await _command.UpdateAsync(operation, ct);
            await _auditLogService.CreateAsync(
                AuditLogTableConst.PaymentAcceptancePointOperation,
                id.ToString(),
                AuditLogOperationTypeConst.Update,
                "Cancelled");
            return Result.Success();
        }, ct);

    private async Task<PaymentAcceptancePointOperation?> GetOperationAsync(
        long id,
        int organizationId,
        CancellationToken ct)
    {
        var query = _queryBuilder.For<PaymentAcceptancePointOperation>()
            .Where(x => x.Id == id &&
                        x.OrganizationId == organizationId &&
                        x.StateId == StateIdConst.ACTIVE)
            .Build();
        query.AddIncludes(x => x.Include(operation => operation.PaymentAcceptancePoint));
        return await _query.GetAsync(query, ct);
    }

    private static Result ValidateConfiguration(PaymentAcceptancePointOperation operation)
    {
        if (!MovementDirectionIdConst.IsValid(operation.DirectionId) ||
            operation.Amount <= 0m ||
            operation.ExchangeRate <= 0m)
            return Result.Failure(PaymentAcceptancePointOperationErrors.InvalidConfiguration(
                "Direction must be IN or OUT; amount and exchange rate must be greater than zero."));

        if (operation.PaymentAcceptancePoint.OrganizationId != operation.OrganizationId ||
            operation.PaymentAcceptancePoint.StateId != StateIdConst.ACTIVE)
            return Result.Failure(PaymentAcceptancePointOperationErrors.InvalidConfiguration(
                "Payment acceptance point is inactive or belongs to another organization."));

        return Result.Success();
    }

    private async Task<PostingBatch?> GetActiveBatchAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PostingBatch>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.PAYMENT_ACCEPTANCE_POINT_OPERATION &&
                        x.DocumentId == id &&
                        x.Status == PostingBatchStatusConst.POSTED)
            .Build();
        return await _batchQuery.GetAsync(query, ct);
    }

    private Task<bool> HasEffectsAsync(long id, CancellationToken ct) =>
        _moneyQuery.AnyAsync(x =>
            x.DocumentTypeId == DocumentTypeIdConst.PAYMENT_ACCEPTANCE_POINT_OPERATION &&
            x.DocumentId == id &&
            x.ReversalEntryId == null,
            ct);

    private async Task<PostingBatch> CreateBatchAsync(
        PaymentAcceptancePointOperation operation,
        string status,
        string comment,
        CancellationToken ct)
    {
        var batch = new PostingBatch
        {
            OrganizationId = operation.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.PAYMENT_ACCEPTANCE_POINT_OPERATION,
            DocumentId = operation.Id,
            Status = status,
            PostedByUserId = _userContext.Id,
            PostedAt = DateTime.Now,
            Comment = comment
        };
        await _batchCommand.CreateAsync(batch, ct);
        return batch;
    }
}
