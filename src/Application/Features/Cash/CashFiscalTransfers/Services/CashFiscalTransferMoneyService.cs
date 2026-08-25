using Application.Abstractions;
using Application.Features.MoneyRegisterBalances;
using Application.Features.Register;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.CashFiscalTransfers;

public interface ICashFiscalTransferMoneyService
{
    Task<Result<List<MoneyRegisterBalance>>> PostAsync(CashFiscalTransferDoc document, long postingBatchId, CancellationToken ct);
    Task<Result<List<MoneyRegisterBalance>>> ReverseAsync(CashFiscalTransferDoc document, long postingBatchId, CancellationToken ct);
    Task<decimal> GetFiscalBalanceAsync(int fiscalCashRegisterId, short currencyId, DateTime asOfDate, CancellationToken ct);
    Task<decimal> GetCashBoxBalanceAsync(int cashBoxId, DateTime asOfDate, CancellationToken ct);
}

public sealed class CashFiscalTransferMoneyService : ICashFiscalTransferMoneyService
{
    private readonly IQueryBuilder _queryBuilder;
    private readonly ICashMoneyRegisterService _cashMoneyRegisterService;
    private readonly IQueryRepository<RetailSaleDocPayment> _paymentQuery;
    private readonly IQueryRepository<MoneyRegisterBalance> _moneyQuery;
    private readonly ICommandRepository<MoneyRegisterBalance> _moneyCommand;

    public CashFiscalTransferMoneyService(
        IQueryBuilder queryBuilder,
        ICashMoneyRegisterService cashMoneyRegisterService,
        IQueryRepository<RetailSaleDocPayment> paymentQuery,
        IQueryRepository<MoneyRegisterBalance> moneyQuery,
        ICommandRepository<MoneyRegisterBalance> moneyCommand)
    {
        _queryBuilder = queryBuilder;
        _cashMoneyRegisterService = cashMoneyRegisterService;
        _paymentQuery = paymentQuery;
        _moneyQuery = moneyQuery;
        _moneyCommand = moneyCommand;
    }

    public async Task<Result<List<MoneyRegisterBalance>>> PostAsync(CashFiscalTransferDoc document, long postingBatchId, CancellationToken ct)
    {
        var now = DateTime.Now;
        var rows = new List<MoneyRegisterBalance>
        {
            CreateRow(document, RegisterDefaultsConst.FiscalCashRegister, document.FiscalCashRegisterId, document.DirectionId, postingBatchId, now),
            CreateRow(document, RegisterDefaultsConst.CashFiscalTransferCashBox, document.CashBoxId, MovementDirectionIdConst.Reverse(document.DirectionId), postingBatchId, now)
        };
        await _moneyCommand.CreateAsync(rows, ct);
        return Result.Success(rows);
    }

    public async Task<Result<List<MoneyRegisterBalance>>> ReverseAsync(CashFiscalTransferDoc document, long postingBatchId, CancellationToken ct)
    {
        var query = _queryBuilder.For<MoneyRegisterBalance>()
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.CASHFISCALTRANSFER &&
                        x.DocumentId == document.Id && x.ReversalEntryId == null)
            .Build();
        var originals = await _moneyQuery.GetAllAsync(query, ct);
        if (originals.Count == 0)
            return Result.Failure<List<MoneyRegisterBalance>>(CashFiscalTransferErrors.MissingMoneyEntries(document.Id));

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
            ReversalEntryId = x.Id
        }).ToList();
        await _moneyCommand.CreateAsync(reversals, ct);
        return Result.Success(reversals);
    }

    public async Task<decimal> GetFiscalBalanceAsync(int fiscalCashRegisterId, short currencyId, DateTime asOfDate, CancellationToken ct)
    {
        var paymentQuery = _queryBuilder.For<RetailSaleDocPayment>()
            .Where(x => x.Owner.CashRegisterId == fiscalCashRegisterId &&
                        x.Owner.CurrencyId == currencyId &&
                        x.Owner.StatusId == DocumentStatusIdConst.POSTED &&
                        x.Owner.DocDate <= asOfDate &&
                        x.PaymentMethod.Code == PaymentMethodCodeConst.CASH)
            .As(x => x.Amount)
            .Build();
        var payments = await _paymentQuery.GetAllAsync(paymentQuery, ct);

        var movementQuery = _queryBuilder.For<MoneyRegisterBalance>()
            .Where(x => x.SourceType == RegisterDefaultsConst.FiscalCashRegister &&
                        x.SourceId == fiscalCashRegisterId &&
                        x.CurrencyId == currencyId &&
                        x.DocDate <= asOfDate)
            .Build();
        var movements = await _moneyQuery.GetAllAsync(movementQuery, ct);
        return payments.Sum() + movements.Sum(x => x.DirectionId * x.Amount);
    }

    public Task<decimal> GetCashBoxBalanceAsync(int cashBoxId, DateTime asOfDate, CancellationToken ct) =>
        _cashMoneyRegisterService.GetCashBoxBalanceAsync(cashBoxId, asOfDate, ct);

    private static MoneyRegisterBalance CreateRow(
        CashFiscalTransferDoc document,
        string sourceType,
        int sourceId,
        short directionId,
        long postingBatchId,
        DateTime now) => new()
    {
        OrganizationId = document.OrganizationId,
        DocumentTypeId = DocumentTypeIdConst.CASHFISCALTRANSFER,
        DocumentId = document.Id,
        SourceType = sourceType,
        SourceId = sourceId,
        DirectionId = directionId,
        CurrencyId = document.CurrencyId,
        Amount = document.Amount,
        DocDate = document.DocDate,
        CreatedDate = now,
        PostingBatchId = postingBatchId
    };
}
