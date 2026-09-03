using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Dashboard.DTOs;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;

namespace Application.Features.Dashboard.Services;

public sealed class BusinessDashboardService : IBusinessDashboardService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<BankAccount> _bankAccountQuery;
    private readonly IQueryRepository<BankOperation> _bankOperationQuery;
    private readonly IQueryRepository<CashBox> _cashBoxQuery;
    private readonly IQueryRepository<CashOperation> _cashOperationQuery;
    private readonly IQueryRepository<CounterpartyRegisterBalance> _counterpartyBalanceQuery;
    private readonly IQueryRepository<EdoDocument> _edoDocumentQuery;
    private readonly IQueryRepository<OrganizationTaxSetting> _taxSettingQuery;
    private readonly IQueryRepository<SaleDoc> _saleDocQuery;
    private readonly IQueryRepository<PurchaseDoc> _purchaseDocQuery;

    public BusinessDashboardService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<BankAccount> bankAccountQuery,
        IQueryRepository<BankOperation> bankOperationQuery,
        IQueryRepository<CashBox> cashBoxQuery,
        IQueryRepository<CashOperation> cashOperationQuery,
        IQueryRepository<CounterpartyRegisterBalance> counterpartyBalanceQuery,
        IQueryRepository<EdoDocument> edoDocumentQuery,
        IQueryRepository<OrganizationTaxSetting> taxSettingQuery,
        IQueryRepository<SaleDoc> saleDocQuery,
        IQueryRepository<PurchaseDoc> purchaseDocQuery)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _bankAccountQuery = bankAccountQuery;
        _bankOperationQuery = bankOperationQuery;
        _cashBoxQuery = cashBoxQuery;
        _cashOperationQuery = cashOperationQuery;
        _counterpartyBalanceQuery = counterpartyBalanceQuery;
        _edoDocumentQuery = edoDocumentQuery;
        _taxSettingQuery = taxSettingQuery;
        _saleDocQuery = saleDocQuery;
        _purchaseDocQuery = purchaseDocQuery;
    }

    public async Task<DashboardOverviewDto> GetOverviewAsync(DashboardFilterDto filter, CancellationToken ct = default) =>
        new()
        {
            Filters = filter,
            Cash = await GetCashAsync(filter, ct),
            Relationships = await GetRelationshipsAsync(filter, ct),
            Tasks = new TaskCalendarDto(),
            Receivables = await GetDebtAsync(filter, false, ct),
            Payables = await GetDebtAsync(filter, true, ct),
            Tax = await GetTaxSummaryAsync(filter, ct),
            ElectronicDocuments = await GetElectronicDocumentsAsync(filter, ct)
        };

    public async Task<DashboardCashDto> GetCashAsync(DashboardFilterDto filter, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is not int organizationId)
            return new DashboardCashDto { SourceStatus = "NOT_AVAILABLE" };

        try
        {
            var accounts = await _bankAccountQuery.GetAllAsync(_queryBuilder.For<BankAccount>()
                .Where(x => x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE &&
                    (!filter.HasCurrencyFilter || filter.CurrencyIds.Contains(x.CurrencyId)))
                .As(x => new CashAccountRow
                {
                    AccountId = x.Id,
                    AccountName = x.Name ?? x.AccountNumber,
                    CurrencyId = x.CurrencyId,
                    CurrencyCode = x.Currency.Code,
                    OpeningBalance = x.OpeningBalance,
                    OpeningBalanceDate = x.OpeningBalanceDate
                })
                .Build(), ct);

            var cashBoxes = await _cashBoxQuery.GetAllAsync(_queryBuilder.For<CashBox>()
                .Where(x => x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE &&
                    (!filter.HasCurrencyFilter || filter.CurrencyIds.Contains(x.CurrencyId)))
                .As(x => new CashAccountRow
                {
                    AccountId = x.Id,
                    AccountName = x.Name,
                    CurrencyId = x.CurrencyId,
                    CurrencyCode = x.Currency.Code,
                    OpeningBalance = x.OpeningBalance,
                    OpeningBalanceDate = x.OpeningBalanceDate
                })
                .Build(), ct);

            var bankOperations = await _bankOperationQuery.GetAllAsync(_queryBuilder.For<BankOperation>()
                .Where(x => x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE &&
                    x.PostedAt.HasValue && !x.CancelledAt.HasValue &&
                    (!filter.HasCurrencyFilter || filter.CurrencyIds.Contains(x.CurrencyId)) &&
                    (!filter.HasStatusFilter || filter.StatusIds.Contains(x.StatusId)) &&
                    (!filter.DateTo.HasValue || x.DocDate < filter.DateTo.Value.Date.AddDays(1)))
                .As(x => new CashMovementRow
                {
                    AccountId = x.BankAccountId,
                    CurrencyId = x.CurrencyId,
                    Date = x.DocDate,
                    Inflow = x.DirectionId == MovementDirectionIdConst.IN ? x.Amount : 0m,
                    Outflow = x.DirectionId == MovementDirectionIdConst.OUT ? x.Amount : 0m
                })
                .Build(), ct);

            var cashOperations = await _cashOperationQuery.GetAllAsync(_queryBuilder.For<CashOperation>()
                .Where(x => x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE &&
                    x.PostedAt.HasValue && !x.CancelledAt.HasValue &&
                    (!filter.HasCurrencyFilter || filter.CurrencyIds.Contains(x.CurrencyId)) &&
                    (!filter.HasStatusFilter || filter.StatusIds.Contains(x.StatusId)) &&
                    (!filter.DateTo.HasValue || x.DocDate < filter.DateTo.Value.Date.AddDays(1)))
                .As(x => new CashMovementRow
                {
                    AccountId = x.CashBoxId,
                    CurrencyId = x.CurrencyId,
                    Date = x.DocDate,
                    Inflow = x.OperationTypeId == OperationTypeIdConst.IN ? x.Amount : 0m,
                    Outflow = x.OperationTypeId == OperationTypeIdConst.OUT ? x.Amount : 0m
                })
                .Build(), ct);

            var allMovements = bankOperations.Concat(cashOperations).ToList();
            var items = accounts.Concat(cashBoxes)
                .Select(account =>
                {
                    var movements = allMovements.Where(x => x.AccountId == account.AccountId && x.CurrencyId == account.CurrencyId).ToList();
                    var openingMovements = filter.DateFrom.HasValue
                        ? movements.Where(x => x.Date < filter.DateFrom.Value.Date).ToList()
                        : [];
                    var periodMovements = filter.DateFrom.HasValue || filter.DateTo.HasValue
                        ? movements.Where(x => (!filter.DateFrom.HasValue || x.Date >= filter.DateFrom.Value.Date) &&
                                               (!filter.DateTo.HasValue || x.Date < filter.DateTo.Value.Date.AddDays(1))).ToList()
                        : movements;
                    var opening = account.OpeningBalance + openingMovements.Sum(x => x.Inflow - x.Outflow);
                    var inflow = periodMovements.Sum(x => x.Inflow);
                    var outflow = periodMovements.Sum(x => x.Outflow);
                    return new DashboardCashItemDto
                    {
                        AccountId = account.AccountId,
                        AccountName = account.AccountName,
                        CurrencyId = account.CurrencyId,
                        CurrencyCode = account.CurrencyCode,
                        OpeningBalance = opening,
                        Inflow = inflow,
                        Outflow = outflow,
                        ClosingBalance = opening + inflow - outflow
                    };
                })
                .ToList();

            return new DashboardCashDto
            {
                SourceStatus = filter.HasWarehouseFilter || filter.HasDocumentTypeFilter ? "PARTIAL" : "AVAILABLE",
                Items = items,
                Totals = new DashboardCashTotalsDto
                {
                    OpeningBalance = items.Sum(x => x.OpeningBalance),
                    Inflow = items.Sum(x => x.Inflow),
                    Outflow = items.Sum(x => x.Outflow),
                    ClosingBalance = items.Sum(x => x.ClosingBalance)
                }
            };
        }
        catch
        {
            return new DashboardCashDto { SourceStatus = "NOT_AVAILABLE" };
        }
    }

    public async Task<DashboardReceivablesPayablesDto> GetReceivablesPayablesAsync(DashboardFilterDto filter, CancellationToken ct = default) =>
        new()
        {
            Receivables = await GetDebtAsync(filter, false, ct),
            Payables = await GetDebtAsync(filter, true, ct)
        };

    private async Task<DashboardRelationshipsDto> GetRelationshipsAsync(DashboardFilterDto filter, CancellationToken ct) =>
        new()
        {
            SourceStatus = filter.HasCurrencyFilter || filter.HasStatusFilter || filter.HasWarehouseFilter
                ? "PARTIAL"
                : "AVAILABLE",
            Incoming = await GetRelationshipsSideAsync(filter, "INBOX", ct),
            Outgoing = await GetRelationshipsSideAsync(filter, "OUTBOX", ct)
        };

    public async Task<DashboardElectronicDocumentsDto> GetElectronicDocumentsAsync(DashboardFilterDto filter, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is not int organizationId)
            return new DashboardElectronicDocumentsDto { SourceStatus = "NOT_AVAILABLE" };

        try
        {
            var query = _queryBuilder.For<EdoDocument>()
                .Where(x => x.OrganizationId == organizationId &&
                    (!filter.HasDocumentTypeFilter || filter.DocumentTypes.Contains(x.DocumentType)) &&
                    x.DocumentDate.HasValue &&
                    (!filter.DateFrom.HasValue || x.DocumentDate.Value >= DateOnly.FromDateTime(filter.DateFrom.Value.Date)) &&
                    (!filter.DateTo.HasValue || x.DocumentDate.Value < DateOnly.FromDateTime(filter.DateTo.Value.Date.AddDays(1))))
                .As(x => new EdoDocumentRow
                {
                    Status = x.Status,
                    DocumentType = x.DocumentType,
                    Direction = x.Direction,
                    DocumentDate = x.DocumentDate
                })
                .Build();

            var rows = await _edoDocumentQuery.GetAllAsync(query, ct);
            var result = new DashboardElectronicDocumentsDto
            {
                SourceStatus = filter.HasStatusFilter || filter.HasCurrencyFilter || filter.HasWarehouseFilter
                    ? "PARTIAL"
                    : "AVAILABLE",
                StatusCounts = GroupCounts(rows.Select(x => (x.Status, (short?)null))),
                TypeCounts = GroupCounts(rows.Select(x => (x.DocumentType, (short?)null))),
                DirectionCounts = GroupCounts(rows.Select(x => (x.Direction, (short?)null))),
                CurrencyTotals = [],
                AmountSeries = []
            };
            return result;
        }
        catch
        {
            return new DashboardElectronicDocumentsDto { SourceStatus = "NOT_AVAILABLE" };
        }
    }

    public async Task<DashboardTaxSummaryDto> GetTaxSummaryAsync(DashboardFilterDto filter, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is not int organizationId)
            return new DashboardTaxSummaryDto { SourceStatus = "NOT_AVAILABLE" };

        try
        {
            var saleRows = filter.IncludesDocumentType("SALE")
                ? await GetSaleTaxRowsAsync(organizationId, filter, ct)
                : [];
            var purchaseRows = filter.IncludesDocumentType("PURCHASE")
                ? await GetPurchaseTaxRowsAsync(organizationId, filter, ct)
                : [];

            var settings = await _taxSettingQuery.GetAllAsync(_queryBuilder.For<OrganizationTaxSetting>()
                .Where(x => x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE)
                .As(x => new TaxSettingRow { IsVatPayer = x.IsVatPayer, EffectiveFrom = x.EffectiveFrom, EffectiveTo = x.EffectiveTo })
                .Build(), ct);
            var asOf = DateOnly.FromDateTime(filter.DateTo ?? DateTime.Today);
            var setting = settings
                .Where(x => x.EffectiveFrom <= asOf && (!x.EffectiveTo.HasValue || x.EffectiveTo.Value >= asOf))
                .OrderByDescending(x => x.EffectiveFrom)
                .FirstOrDefault();

            var items = saleRows.Concat(purchaseRows)
                .GroupBy(x => new { x.DocumentType, x.CurrencyId })
                .Select(x => new DashboardTaxSummaryItemDto
                {
                    DocumentType = x.Key.DocumentType,
                    CurrencyId = x.Key.CurrencyId,
                    Count = x.Count(),
                    VatAmount = x.Sum(y => y.VatAmount)
                })
                .ToList();

            return new DashboardTaxSummaryDto
            {
                SourceStatus = setting is null ? "PARTIAL" : "AVAILABLE",
                IsVatPayer = setting?.IsVatPayer,
                Total = items.Sum(x => x.VatAmount),
                Items = items
            };
        }
        catch
        {
            return new DashboardTaxSummaryDto { SourceStatus = "NOT_AVAILABLE" };
        }
    }

    private async Task<DashboardRelationshipSideDto> GetRelationshipsSideAsync(DashboardFilterDto filter, string direction, CancellationToken ct)
    {
        if (_userContext.OrganizationId is not int organizationId)
            return new DashboardRelationshipSideDto { SourceStatus = "NOT_AVAILABLE" };

        try
        {
            var rows = await _edoDocumentQuery.GetAllAsync(_queryBuilder.For<EdoDocument>()
                .Where(x => x.OrganizationId == organizationId && x.Direction == direction &&
                    (!filter.HasDocumentTypeFilter || filter.DocumentTypes.Contains(x.DocumentType)) &&
                    x.DocumentDate.HasValue &&
                    (!filter.DateFrom.HasValue || x.DocumentDate.Value >= DateOnly.FromDateTime(filter.DateFrom.Value.Date)) &&
                    (!filter.DateTo.HasValue || x.DocumentDate.Value < DateOnly.FromDateTime(filter.DateTo.Value.Date.AddDays(1))))
                .As(x => new EdoDocumentRow { Status = x.Status })
                .Build(), ct);
            return new DashboardRelationshipSideDto
            {
                SourceStatus = filter.HasCurrencyFilter || filter.HasStatusFilter || filter.HasWarehouseFilter
                    ? "PARTIAL"
                    : "AVAILABLE",
                Total = rows.Count,
                StatusCounts = GroupCounts(rows.Select(x => (x.Status, (short?)null)))
            };
        }
        catch
        {
            return new DashboardRelationshipSideDto { SourceStatus = "NOT_AVAILABLE" };
        }
    }

    private async Task<DashboardDebtDto> GetDebtAsync(DashboardFilterDto filter, bool payable, CancellationToken ct)
    {
        if (_userContext.OrganizationId is not int organizationId)
            return new DashboardDebtDto { SourceStatus = "NOT_AVAILABLE" };

        var requiredDocumentType = payable ? "PURCHASE" : "SALE";
        if (!filter.IncludesDocumentType(requiredDocumentType))
            return new DashboardDebtDto
            {
                SourceStatus = filter.HasWarehouseFilter || filter.HasStatusFilter ? "PARTIAL" : "AVAILABLE"
            };

        try
        {
            var rows = await _counterpartyBalanceQuery.GetAllAsync(_queryBuilder.For<CounterpartyRegisterBalance>()
                .Where(x => x.OrganizationId == organizationId &&
                    (payable ? x.DocumentTypeId == DocumentTypeIdConst.PURCHASE : x.DocumentTypeId == DocumentTypeIdConst.SALE) &&
                    (!filter.HasCurrencyFilter || filter.CurrencyIds.Contains(x.CurrencyId)) &&
                    (!filter.DateFrom.HasValue || x.DocDate >= filter.DateFrom.Value.Date) &&
                    (!filter.DateTo.HasValue || x.DocDate < filter.DateTo.Value.Date.AddDays(1)))
                .As(x => new DebtRow
                {
                    CounterpartyId = x.CounterpartyId,
                    CounterpartyName = x.Counterparty.ShortName,
                    CurrencyId = x.CurrencyId,
                    OperationTypeId = x.OperationTypeId,
                    Amount = x.Amount
                })
                .Build(), ct);

            var counterparties = rows.GroupBy(x => new { x.CounterpartyId, x.CounterpartyName, x.CurrencyId })
                .Select(x => new DashboardCounterpartyDebtDto
                {
                    CounterpartyId = x.Key.CounterpartyId,
                    CounterpartyName = x.Key.CounterpartyName,
                    CurrencyId = x.Key.CurrencyId,
                    CurrentAmount = x.Sum(SignedAmount),
                    OverdueAmount = null
                })
                .ToList();

            return new DashboardDebtDto
            {
                SourceStatus = filter.HasWarehouseFilter || filter.HasStatusFilter
                    ? "PARTIAL"
                    : rows.Count == 0 ? "AVAILABLE" : "PARTIAL",
                Current = counterparties.Sum(x => x.CurrentAmount),
                Overdue = null,
                Buckets = [],
                Counterparties = counterparties
            };

            decimal SignedAmount(DebtRow row) => row.OperationTypeId == OperationTypeIdConst.DEBT_DECREASE ? -row.Amount : row.Amount;
        }
        catch
        {
            return new DashboardDebtDto { SourceStatus = "NOT_AVAILABLE" };
        }
    }

    private async Task<List<TaxRow>> GetSaleTaxRowsAsync(int organizationId, DashboardFilterDto filter, CancellationToken ct)
    {
        if (!filter.IncludesDocumentType("SALE"))
            return [];
        return await _saleDocQuery.GetAllAsync(_queryBuilder.For<SaleDoc>()
            .Where(x => x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE &&
                (!filter.HasWarehouseFilter || filter.WarehouseIds.Contains(x.WarehouseId)) &&
                (!filter.HasCurrencyFilter || filter.CurrencyIds.Contains(x.CurrencyId)) &&
                (!filter.HasStatusFilter || filter.StatusIds.Contains(x.StatusId)) &&
                (!filter.DateFrom.HasValue || x.DocDate >= filter.DateFrom.Value.Date) &&
                (!filter.DateTo.HasValue || x.DocDate < filter.DateTo.Value.Date.AddDays(1)))
            .As(x => new TaxRow { DocumentType = "SALE", CurrencyId = x.CurrencyId, VatAmount = x.VatAmount })
            .Build(), ct);
    }

    private async Task<List<TaxRow>> GetPurchaseTaxRowsAsync(int organizationId, DashboardFilterDto filter, CancellationToken ct)
    {
        return await _purchaseDocQuery.GetAllAsync(_queryBuilder.For<PurchaseDoc>()
            .Where(x => x.OrganizationId == organizationId && x.StateId == StateIdConst.ACTIVE &&
                (!filter.HasWarehouseFilter || filter.WarehouseIds.Contains(x.WarehouseId)) &&
                (!filter.HasCurrencyFilter || filter.CurrencyIds.Contains(x.CurrencyId)) &&
                (!filter.HasStatusFilter || filter.StatusIds.Contains(x.StatusId)) &&
                (!filter.DateFrom.HasValue || x.DocDate >= filter.DateFrom.Value.Date) &&
                (!filter.DateTo.HasValue || x.DocDate < filter.DateTo.Value.Date.AddDays(1)))
            .As(x => new TaxRow { DocumentType = "PURCHASE", CurrencyId = x.CurrencyId, VatAmount = x.VatAmount })
            .Build(), ct);
    }

    private static List<DashboardStatusCountDto> GroupCounts(IEnumerable<(string Status, short? CurrencyId)> values) =>
        values.GroupBy(x => new { x.Status, x.CurrencyId })
            .Select(x => new DashboardStatusCountDto
            {
                Status = x.Key.Status,
                Count = x.Count(),
                CurrencyId = x.Key.CurrencyId,
                Amount = null
            })
            .ToList();

    private sealed class CashAccountRow
    {
        public int AccountId { get; init; }
        public string AccountName { get; init; } = string.Empty;
        public short CurrencyId { get; init; }
        public string CurrencyCode { get; init; } = string.Empty;
        public decimal OpeningBalance { get; init; }
        public DateOnly? OpeningBalanceDate { get; init; }
    }

    private sealed class CashMovementRow
    {
        public int AccountId { get; init; }
        public short CurrencyId { get; init; }
        public DateTime Date { get; init; }
        public decimal Inflow { get; init; }
        public decimal Outflow { get; init; }
    }

    private sealed class EdoDocumentRow
    {
        public string Status { get; init; } = string.Empty;
        public string DocumentType { get; init; } = string.Empty;
        public string Direction { get; init; } = string.Empty;
        public DateOnly? DocumentDate { get; init; }
    }

    private sealed class DebtRow
    {
        public int CounterpartyId { get; init; }
        public string CounterpartyName { get; init; } = string.Empty;
        public short CurrencyId { get; init; }
        public short OperationTypeId { get; init; }
        public decimal Amount { get; init; }
    }

    private sealed class TaxRow
    {
        public string DocumentType { get; init; } = string.Empty;
        public short CurrencyId { get; init; }
        public decimal VatAmount { get; init; }
    }

    private sealed class TaxSettingRow
    {
        public bool IsVatPayer { get; init; }
        public DateOnly EffectiveFrom { get; init; }
        public DateOnly? EffectiveTo { get; init; }
    }
}
