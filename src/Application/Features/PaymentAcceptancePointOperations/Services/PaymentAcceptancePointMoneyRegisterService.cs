using Application.Abstractions;
using Application.Features.Register;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.PaymentAcceptancePointOperations;

public sealed class PaymentAcceptancePointMoneyRegisterService : IPaymentAcceptancePointMoneyRegisterService
{
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<MoneyRegisterBalance> _query;
    private readonly ICommandRepository<MoneyRegisterBalance> _command;

    public PaymentAcceptancePointMoneyRegisterService(
        IQueryBuilder queryBuilder,
        IQueryRepository<MoneyRegisterBalance> query,
        ICommandRepository<MoneyRegisterBalance> command)
    {
        _queryBuilder = queryBuilder;
        _query = query;
        _command = command;
    }

    public async Task<Result<List<MoneyRegisterBalance>>> PostAsync(
        PaymentAcceptancePointOperation operation,
        long postingBatchId,
        CancellationToken ct = default)
    {
        var row = new MoneyRegisterBalance
        {
            OrganizationId = operation.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.PAYMENT_ACCEPTANCE_POINT_OPERATION,
            DocumentId = operation.Id,
            SourceType = RegisterDefaultsConst.PaymentAcceptancePoint,
            SourceId = operation.PaymentAcceptancePointId,
            DirectionId = operation.DirectionId,
            CurrencyId = operation.CurrencyId,
            Amount = operation.Amount,
            DocDate = operation.DocDate,
            CreatedDate = DateTime.Now,
            PostingBatchId = postingBatchId
        };

        await _command.CreateAsync(row, ct);
        return Result.Success(new List<MoneyRegisterBalance> { row });
    }

    public async Task<Result<List<MoneyRegisterBalance>>> ReverseAsync(
        PaymentAcceptancePointOperation operation,
        long postingBatchId,
        CancellationToken ct = default)
    {
        var query = _queryBuilder.For<MoneyRegisterBalance>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.PAYMENT_ACCEPTANCE_POINT_OPERATION &&
                        x.DocumentId == operation.Id &&
                        x.ReversalEntryId == null)
            .Build();
        var originals = await _query.GetAllAsync(query, ct);
        if (originals.Count == 0)
            return Result.Failure<List<MoneyRegisterBalance>>(
                PaymentAcceptancePointOperationErrors.MissingMoneyEntries(operation.Id));

        var now = DateTime.Now;
        var reversals = originals.Select(x => new MoneyRegisterBalance
        {
            OrganizationId = x.OrganizationId,
            DocumentTypeId = x.DocumentTypeId,
            DocumentId = x.DocumentId,
            SourceType = x.SourceType,
            SourceId = x.SourceId,
            DirectionId = MovementDirectionIdConst.Reverse(x.DirectionId),
            CurrencyId = x.CurrencyId,
            Amount = x.Amount,
            DocDate = now,
            CreatedDate = now,
            PostingBatchId = postingBatchId,
            SourceLineId = x.SourceLineId,
            ReversalEntryId = x.Id
        }).ToList();

        await _command.CreateAsync(reversals, ct);
        return Result.Success(reversals);
    }

    public async Task<decimal> GetBalanceAsync(
        int paymentAcceptancePointId,
        short currencyId,
        DateTime asOfDate,
        CancellationToken ct = default)
    {
        var query = _queryBuilder.For<MoneyRegisterBalance>()
            .Where(x => x.SourceType == RegisterDefaultsConst.PaymentAcceptancePoint &&
                        x.SourceId == paymentAcceptancePointId &&
                        x.CurrencyId == currencyId &&
                        x.DocDate <= asOfDate)
            .Build();
        var entries = await _query.GetAllAsync(query, ct);
        return entries.Sum(x => x.DirectionId * x.Amount);
    }
}
