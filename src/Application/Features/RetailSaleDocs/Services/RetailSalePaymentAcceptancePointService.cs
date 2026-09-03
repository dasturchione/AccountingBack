using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.DocumentNumbers;
using Application.Features.PaymentAcceptancePointOperations;
using Application.Features.Register;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.RetailSaleDocs;

public sealed class RetailSalePaymentAcceptancePointService : IRetailSalePaymentAcceptancePointService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IDocumentPostingLock _postingLock;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly IPaymentAcceptancePointMoneyRegisterService _moneyRegisterService;
    private readonly IQueryRepository<DocumentRegistry> _documentRegistryQuery;
    private readonly IQueryRepository<PaymentMethod> _paymentMethodQuery;
    private readonly IQueryRepository<PaymentAcceptancePointOperation> _operationQuery;
    private readonly ICommandRepository<PaymentAcceptancePointOperation> _operationCommand;
    private readonly IQueryRepository<PostingBatch> _batchQuery;
    private readonly ICommandRepository<PostingBatch> _batchCommand;

    public RetailSalePaymentAcceptancePointService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IDocumentPostingLock postingLock,
        IDocumentNumberService documentNumberService,
        IPaymentAcceptancePointMoneyRegisterService moneyRegisterService,
        IQueryRepository<DocumentRegistry> documentRegistryQuery,
        IQueryRepository<PaymentMethod> paymentMethodQuery,
        IQueryRepository<PaymentAcceptancePointOperation> operationQuery,
        ICommandRepository<PaymentAcceptancePointOperation> operationCommand,
        IQueryRepository<PostingBatch> batchQuery,
        ICommandRepository<PostingBatch> batchCommand)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _postingLock = postingLock;
        _documentNumberService = documentNumberService;
        _moneyRegisterService = moneyRegisterService;
        _documentRegistryQuery = documentRegistryQuery;
        _paymentMethodQuery = paymentMethodQuery;
        _operationQuery = operationQuery;
        _operationCommand = operationCommand;
        _batchQuery = batchQuery;
        _batchCommand = batchCommand;
    }

    public async Task<Result> PostAsync(RetailSaleDoc document, CancellationToken ct = default)
    {
        var registry = await GetRetailSaleRegistryAsync(document, ct);
        if (registry is null)
            return Result.Failure(RetailSaleDocErrors.DocumentRegistryNotFound(document.Id, _userContext.LanguageId));

        if (await _operationQuery.AnyAsync(x =>
                x.RelatedDocumentId == registry.Id &&
                x.StateId == StateIdConst.ACTIVE,
                ct))
            return Result.Failure(RetailSaleDocErrors.PaymentOperationsAlreadyExist(document.Id, _userContext.LanguageId));

        var methodIds = document.RetailSaleDocPayments
            .Select(x => x.PaymentMethodId)
            .Distinct()
            .ToList();
        var methods = await _paymentMethodQuery.GetAllAsync(
            _queryBuilder.For<PaymentMethod>()
                .Where(x => methodIds.Contains(x.Id))
                .Build(),
            ct);
        var methodCodes = methods.ToDictionary(x => x.Id, x => x.Code);
        if (methodCodes.Count != methodIds.Count)
            return Result.Failure(RetailSaleDocErrors.InvalidPayment(_userContext.LanguageId));

        var plannedPayments = new List<PlannedPayment>();
        foreach (var payment in document.RetailSaleDocPayments)
        {
            var plan = RetailSalePaymentAcceptancePointPolicy.Build(
                methodCodes[payment.PaymentMethodId],
                payment.PaymentAcceptancePointId,
                _userContext.LanguageId);
            if (!plan.IsSuccess)
                return Result.Failure(plan.Error);

            foreach (var movement in plan.Value)
            {
                plannedPayments.Add(new PlannedPayment(
                    payment.PaymentAcceptancePointId!.Value,
                    payment.Amount,
                    payment.TransactionNumber,
                    movement));
            }
        }

        foreach (var pointId in plannedPayments.Select(x => x.PaymentAcceptancePointId).Distinct().OrderBy(x => x))
        {
            await _postingLock.AcquireMoneyAsync(
                document.OrganizationId,
                RegisterDefaultsConst.PaymentAcceptancePoint,
                pointId,
                ct);
        }

        foreach (var planned in plannedPayments)
        {
            var number = await _documentNumberService.GetNextAsync(
                document.OrganizationId,
                DocumentTypeIdConst.PAYMENT_ACCEPTANCE_POINT_OPERATION,
                document.DocDate,
                ct);
            if (!number.IsSuccess)
                return Result.Failure(number.Error);

            var now = DateTime.Now;
            var operation = RetailSalePaymentAcceptancePointOperationFactory.Create(
                document.OrganizationId,
                planned.PaymentAcceptancePointId,
                registry.Id,
                number.Value.DocumentNumber,
                number.Value.DocumentDate,
                document.CurrencyId,
                planned.Amount,
                document.ExchangeRate,
                planned.TransactionNumber,
                planned.Movement,
                _userContext.Id,
                now);
            await _operationCommand.CreateAsync(operation, ct);

            if (operation.StatusId != DocumentStatusIdConst.POSTED)
                continue;

            var batch = await CreateBatchAsync(operation, PostingBatchStatusConst.POSTED, "Created from confirmed retail sale", ct);
            var money = await _moneyRegisterService.PostAsync(operation, batch.Id, ct);
            if (!money.IsSuccess)
                return Result.Failure(money.Error);
        }

        return Result.Success();
    }

    public async Task<Result> ReverseAsync(RetailSaleDoc document, CancellationToken ct = default)
    {
        var registry = await GetRetailSaleRegistryAsync(document, ct);
        if (registry is null)
            return Result.Failure(RetailSaleDocErrors.DocumentRegistryNotFound(document.Id, _userContext.LanguageId));

        var operations = await _operationQuery.GetAllAsync(
            _queryBuilder.For<PaymentAcceptancePointOperation>()
                .Where(x => x.RelatedDocumentId == registry.Id &&
                            x.StateId == StateIdConst.ACTIVE &&
                            x.StatusId != DocumentStatusIdConst.CANCELLED)
                .Build(),
            ct);
        if (operations.Count == 0)
            return Result.Success();

        foreach (var pointId in operations.Select(x => x.PaymentAcceptancePointId).Distinct().OrderBy(x => x))
        {
            await _postingLock.AcquireMoneyAsync(
                document.OrganizationId,
                RegisterDefaultsConst.PaymentAcceptancePoint,
                pointId,
                ct);
        }

        var postedGroups = operations
            .Where(x => x.StatusId == DocumentStatusIdConst.POSTED)
            .GroupBy(x => new { x.PaymentAcceptancePointId, x.CurrencyId });
        foreach (var group in postedGroups)
        {
            var balance = await _moneyRegisterService.GetBalanceAsync(
                group.Key.PaymentAcceptancePointId,
                group.Key.CurrencyId,
                DateTime.Now,
                ct);
            var balanceAfterReversal = balance - group.Sum(x => x.DirectionId * x.Amount);
            if (balanceAfterReversal < 0m)
                return Result.Failure(RetailSaleDocErrors.InsufficientPaymentPointBalance(balance, balanceAfterReversal, _userContext.LanguageId));
        }

        foreach (var operation in operations
                     .OrderBy(x => x.DirectionId == MovementDirectionIdConst.OUT ? 0 : 1)
                     .ThenBy(x => x.Id))
        {
            if (operation.StatusId == DocumentStatusIdConst.POSTED)
            {
                var activeBatch = await GetActiveBatchAsync(operation.Id, ct);
                if (activeBatch is null)
                    return Result.Failure(RetailSaleDocErrors.MissingPaymentOperationBatch(operation.Id, _userContext.LanguageId));

                var reversalBatch = await CreateBatchAsync(
                    operation,
                    PostingBatchStatusConst.REVERSAL,
                    $"Retail sale {document.Id} cancelled",
                    ct);
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
            await _operationCommand.UpdateAsync(operation, ct);
        }

        return Result.Success();
    }

    private Task<DocumentRegistry?> GetRetailSaleRegistryAsync(RetailSaleDoc document, CancellationToken ct)
    {
        var query = _queryBuilder.For<DocumentRegistry>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.RETAIL_SALE &&
                        x.DocumentId == document.Id &&
                        x.OrganizationId == document.OrganizationId &&
                        x.StateId == StateIdConst.ACTIVE)
            .Build();
        return _documentRegistryQuery.GetAsync(query, ct);
    }

    private async Task<PostingBatch?> GetActiveBatchAsync(long operationId, CancellationToken ct)
    {
        var query = _queryBuilder.For<PostingBatch>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.PAYMENT_ACCEPTANCE_POINT_OPERATION &&
                        x.DocumentId == operationId &&
                        x.Status == PostingBatchStatusConst.POSTED)
            .Build();
        return await _batchQuery.GetAsync(query, ct);
    }

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
            PostedAt = DateTime.Now,
            PostedByUserId = _userContext.Id,
            Comment = comment
        };
        await _batchCommand.CreateAsync(batch, ct);
        return batch;
    }

    private sealed record PlannedPayment(
        int PaymentAcceptancePointId,
        decimal Amount,
        string? TransactionNumber,
        RetailSalePaymentAcceptancePointMovement Movement);
}
