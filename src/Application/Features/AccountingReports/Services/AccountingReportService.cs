using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Ledger;
using Application.Features.TrialBalance;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Filters;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.AccountingReports;

public class AccountingReportService : IAccountingReportService
{
    private const int DefaultPageSize = 50;

    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<AccountingPeriod> _periodQuery;
    private readonly IQueryRepository<Currency> _currencyQuery;
    private readonly IQueryRepository<ChartAccount> _chartAccountQuery;
    private readonly ITrialBalanceReadRepository _trialBalanceReadRepository;
    private readonly ILedgerService _ledgerService;
    private readonly IAccountingReportReadRepository _readRepository;

    public AccountingReportService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<AccountingPeriod> periodQuery,
        IQueryRepository<Currency> currencyQuery,
        IQueryRepository<ChartAccount> chartAccountQuery,
        ITrialBalanceReadRepository trialBalanceReadRepository,
        ILedgerService ledgerService,
        IAccountingReportReadRepository readRepository)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _periodQuery = periodQuery;
        _currencyQuery = currencyQuery;
        _chartAccountQuery = chartAccountQuery;
        _trialBalanceReadRepository = trialBalanceReadRepository;
        _ledgerService = ledgerService;
        _readRepository = readRepository;
    }

    public async Task<Result<BalanceSheetDto>> GetBalanceSheetAsync(BalanceSheetFilter filter, CancellationToken ct = default)
    {
        var validation = await ValidateBaseFilterAsync(filter, requirePagination: false, ct);
        if (!validation.IsSuccess)
            return Result.Failure<BalanceSheetDto>(validation.Error);

        var rows = await LoadTrialBalanceRowsAsync(validation.Value, ct);
        var incomeStatement = BuildIncomeStatement(filter, validation.Value, rows);
        var currentPeriodResult = incomeStatement.NetProfit;

        var assetRows = new List<BalanceSheetRowDto>();
        var liabilityRows = new List<BalanceSheetRowDto>();
        var equityRows = new List<BalanceSheetRowDto>();

        foreach (var row in rows)
        {
            var net = GetClosingNet(row);
            if (net == 0m)
                continue;

            var target = ResolveBalanceSheetTarget(row, net, assetRows, liabilityRows, equityRows);
            if (target is null)
                continue;

            target.Add(new BalanceSheetRowDto
            {
                AccountId = row.AccountId,
                AccountCode = row.AccountCode,
                AccountNumber = row.AccountNumber,
                AccountName = row.AccountName,
                Balance = decimal.Abs(net)
            });
        }

        if (currentPeriodResult != 0m)
        {
            equityRows.Add(new BalanceSheetRowDto
            {
                AccountCode = CurrentPeriodResultCode,
                AccountName = CurrentPeriodResultName,
                Balance = currentPeriodResult
            });
        }

        var sections = new List<BalanceSheetSectionDto>
        {
            BuildBalanceSheetSection(AssetsCode, AssetsName, assetRows),
            BuildBalanceSheetSection(LiabilitiesCode, LiabilitiesName, liabilityRows),
            BuildBalanceSheetSection(EquityCode, EquityName, equityRows)
        };

        return new BalanceSheetDto
        {
            PeriodId = filter.PeriodId,
            DateFrom = validation.Value.DateFrom,
            DateTo = validation.Value.DateTo,
            CurrencyId = filter.CurrencyId,
            TotalAssets = sections[0].Total,
            TotalLiabilities = sections[1].Total,
            TotalEquity = sections[2].Rows.Sum(x => x.Balance),
            Sections = sections
        };
    }

    public async Task<Result<IncomeStatementDto>> GetIncomeStatementAsync(IncomeStatementFilter filter, CancellationToken ct = default)
    {
        var validation = await ValidateBaseFilterAsync(filter, requirePagination: false, ct);
        if (!validation.IsSuccess)
            return Result.Failure<IncomeStatementDto>(validation.Error);

        var rows = await LoadTrialBalanceRowsAsync(validation.Value, ct);
        return BuildIncomeStatement(filter, validation.Value, rows);
    }

    public async Task<Result<CashFlowDto>> GetCashFlowAsync(CashFlowFilter filter, CancellationToken ct = default)
    {
        var validation = await ValidateBaseFilterAsync(filter, requirePagination: false, ct);
        if (!validation.IsSuccess)
            return Result.Failure<CashFlowDto>(validation.Error);

        var readResult = await _readRepository.GetCashFlowAsync(new CashFlowReadRequest
        {
            DateFrom = validation.Value.DateFrom,
            DateTo = validation.Value.DateTo,
            CurrencyId = filter.CurrencyId
        }, ct);

        var sectionMap = new Dictionary<string, CashFlowSectionDto>(StringComparer.Ordinal)
        {
            [OperatingCode] = new() { Code = OperatingCode, Name = OperatingName },
            [InvestingCode] = new() { Code = InvestingCode, Name = InvestingName },
            [FinancingCode] = new() { Code = FinancingCode, Name = FinancingName },
            [TransfersCode] = new() { Code = TransfersCode, Name = TransfersName }
        };

        foreach (var row in readResult.Rows
                     .OrderBy(x => x.CounterpartAccountCode, StringComparer.Ordinal)
                     .ThenBy(x => x.CounterpartAccountName, StringComparer.Ordinal))
        {
            var section = sectionMap[ClassifyCashFlowSection(row.CounterpartAccountCode)];
            section.Rows.Add(new CashFlowRowDto
            {
                CounterpartAccountCode = row.CounterpartAccountCode,
                CounterpartAccountName = row.CounterpartAccountName,
                Inflow = row.Inflow,
                Outflow = row.Outflow
            });
        }

        foreach (var section in sectionMap.Values)
        {
            section.Inflow = section.Rows.Sum(x => x.Inflow);
            section.Outflow = section.Rows.Sum(x => x.Outflow);
        }

        return new CashFlowDto
        {
            PeriodId = filter.PeriodId,
            DateFrom = validation.Value.DateFrom,
            DateTo = validation.Value.DateTo,
            CurrencyId = filter.CurrencyId,
            OpeningCashBalance = readResult.OpeningCashBalance,
            ClosingCashBalance = readResult.ClosingCashBalance,
            Sections = [sectionMap[OperatingCode], sectionMap[InvestingCode], sectionMap[FinancingCode], sectionMap[TransfersCode]]
        };
    }

    public async Task<Result<AccountTurnoverDto>> GetAccountTurnoverAsync(AccountTurnoverFilter filter, CancellationToken ct = default)
    {
        var validation = await ValidateBaseFilterAsync(filter, requirePagination: false, ct);
        if (!validation.IsSuccess)
            return Result.Failure<AccountTurnoverDto>(validation.Error);

        if (filter.AccountId.HasValue)
        {
            var accountExists = await _chartAccountQuery.AnyAsync(
                x => x.Id == filter.AccountId.Value && x.StateId == StateIdConst.ACTIVE,
                ct);

            if (!accountExists)
                return Result.Failure<AccountTurnoverDto>(AccountingReportErrors.AccountNotFound(filter.AccountId.Value, _userContext.LanguageId));
        }

        var rows = await LoadTrialBalanceRowsAsync(validation.Value, ct);
        var items = rows
            .Where(x => !filter.AccountId.HasValue || x.AccountId == filter.AccountId.Value)
            .Select(MapTurnoverRow)
            .Where(x => filter.IncludeZeroBalance || HasAnyTurnover(x))
            .ToList();

        return new AccountTurnoverDto
        {
            PeriodId = filter.PeriodId,
            DateFrom = validation.Value.DateFrom,
            DateTo = validation.Value.DateTo,
            CurrencyId = filter.CurrencyId,
            IncludeZeroBalance = filter.IncludeZeroBalance,
            OpeningDebitTotal = items.Sum(x => x.OpeningDebit),
            OpeningCreditTotal = items.Sum(x => x.OpeningCredit),
            PeriodDebitTotal = items.Sum(x => x.PeriodDebit),
            PeriodCreditTotal = items.Sum(x => x.PeriodCredit),
            ClosingDebitTotal = items.Sum(x => x.ClosingDebit),
            ClosingCreditTotal = items.Sum(x => x.ClosingCredit),
            Items = items
        };
    }

    public async Task<Result<AccountCardDto>> GetAccountCardAsync(AccountCardFilter filter, CancellationToken ct = default)
    {
        var ledgerResult = await _ledgerService.GetAsync(new LedgerFilter
        {
            AccountId = filter.AccountId,
            PeriodId = filter.PeriodId,
            DateFrom = filter.DateFrom,
            DateTo = filter.DateTo,
            CurrencyId = filter.CurrencyId,
            CounterpartyId = filter.CounterpartyId,
            WarehouseId = filter.WarehouseId,
            Page = filter.Page,
            PageSize = filter.PageSize
        }, ct);

        if (!ledgerResult.IsSuccess)
            return Result.Failure<AccountCardDto>(ledgerResult.Error);

        return new AccountCardDto
        {
            AccountId = ledgerResult.Value.AccountId,
            AccountCode = ledgerResult.Value.AccountCode,
            AccountName = ledgerResult.Value.AccountName,
            PeriodId = ledgerResult.Value.PeriodId,
            DateFrom = ledgerResult.Value.DateFrom,
            DateTo = ledgerResult.Value.DateTo,
            CurrencyId = ledgerResult.Value.CurrencyId,
            OpeningBalance = ledgerResult.Value.OpeningBalance,
            ClosingBalance = ledgerResult.Value.ClosingBalance,
            TotalDebit = ledgerResult.Value.TotalDebit,
            TotalCredit = ledgerResult.Value.TotalCredit,
            Page = ledgerResult.Value.Page,
            PageSize = ledgerResult.Value.PageSize,
            TotalCount = ledgerResult.Value.TotalCount,
            TotalPages = ledgerResult.Value.TotalPages,
            HasPreviousPage = ledgerResult.Value.HasPreviousPage,
            HasNextPage = ledgerResult.Value.HasNextPage,
            Transactions = ledgerResult.Value.Transactions.Select(MapAccountCardTransaction).ToList()
        };
    }

    public async Task<Result<JournalDto>> GetJournalAsync(JournalFilter filter, CancellationToken ct = default)
    {
        var validation = await ValidateBaseFilterAsync(filter, requirePagination: true, ct);
        if (!validation.IsSuccess)
            return Result.Failure<JournalDto>(validation.Error);

        var pageSize = filter.PageSize ?? DefaultPageSize;
        var readResult = await _readRepository.GetJournalAsync(new JournalReadRequest
        {
            DateFrom = validation.Value.DateFrom,
            DateTo = validation.Value.DateTo,
            CurrencyId = filter.CurrencyId,
            DocumentTypeId = filter.DocumentTypeId,
            Page = filter.Page,
            PageSize = pageSize
        }, ct);

        var totalPages = readResult.TotalCount > 0
            ? (int)Math.Ceiling(readResult.TotalCount / (double)pageSize)
            : 0;

        return new JournalDto
        {
            PeriodId = filter.PeriodId,
            DateFrom = validation.Value.DateFrom,
            DateTo = validation.Value.DateTo,
            CurrencyId = filter.CurrencyId,
            DocumentTypeId = filter.DocumentTypeId,
            Page = filter.Page,
            PageSize = pageSize,
            TotalCount = readResult.TotalCount,
            TotalPages = totalPages,
            HasPreviousPage = filter.Page > 1,
            HasNextPage = filter.Page < totalPages,
            Entries = readResult.Rows.Select(MapJournalEntry).ToList()
        };
    }

    private async Task<List<TrialBalanceReadRow>> LoadTrialBalanceRowsAsync(
        ReportValidationState validation,
        CancellationToken ct)
    {
        var readResult = await _trialBalanceReadRepository.GetAsync(new TrialBalanceReadRequest
        {
            DateFrom = validation.DateFrom,
            DateTo = validation.DateTo,
            CurrencyId = validation.CurrencyId
        }, ct);

        return readResult.Rows
            .OrderBy(x => x.AccountCode, StringComparer.Ordinal)
            .ThenBy(x => x.AccountId)
            .ToList();
    }

    private async Task<Result<ReportValidationState>> ValidateBaseFilterAsync(
        AccountingReportFilterBase filter,
        bool requirePagination,
        CancellationToken ct)
    {
        if (requirePagination && filter is IPaginationFilter paginationFilter &&
            (paginationFilter.Page <= 0 || paginationFilter.PageSize <= 0))
        {
            return Result.Failure<ReportValidationState>(AccountingReportErrors.InvalidPagination(_userContext.LanguageId));
        }

        if (filter.DateFrom.HasValue && filter.DateTo.HasValue && filter.DateFrom.Value.Date > filter.DateTo.Value.Date)
            return Result.Failure<ReportValidationState>(AccountingReportErrors.InvalidDateRange(_userContext.LanguageId));

        if (filter.CurrencyId.HasValue)
        {
            var currencyExists = await _currencyQuery.AnyAsync(
                x => x.Id == filter.CurrencyId.Value && x.StateId == StateIdConst.ACTIVE,
                ct);

            if (!currencyExists)
                return Result.Failure<ReportValidationState>(AccountingReportErrors.CurrencyNotFound(filter.CurrencyId.Value, _userContext.LanguageId));
        }

        DateTime? dateFrom = filter.DateFrom?.Date;
        DateTime? dateTo = filter.DateTo?.Date.AddDays(1).AddTicks(-1);

        if (filter.PeriodId.HasValue)
        {
            var periodQuery = _queryBuilder.For<AccountingPeriod>()
                .Where(x => x.Id == filter.PeriodId.Value)
                .Build();
            var period = await _periodQuery.GetAsync(periodQuery, ct);

            if (period is null)
                return Result.Failure<ReportValidationState>(AccountingReportErrors.PeriodNotFound(filter.PeriodId.Value, _userContext.LanguageId));

            var periodStart = period.StartDate.ToDateTime(TimeOnly.MinValue);
            var periodEnd = period.EndDate.ToDateTime(TimeOnly.MaxValue);

            dateFrom ??= periodStart;
            dateTo ??= periodEnd;

            if (dateFrom.Value < periodStart || dateTo.Value > periodEnd)
                return Result.Failure<ReportValidationState>(AccountingReportErrors.DateRangeOutsidePeriod(period.Id, _userContext.LanguageId));
        }

        return new ReportValidationState
        {
            DateFrom = dateFrom,
            DateTo = dateTo,
            CurrencyId = filter.CurrencyId
        };
    }

    private static IncomeStatementDto BuildIncomeStatement(
        AccountingReportFilterBase filter,
        ReportValidationState validation,
        IReadOnlyCollection<TrialBalanceReadRow> rows)
    {
        var revenueRows = rows
            .Where(x => IsRevenueCode(x.AccountCode))
            .Select(x => MapIncomeRow(x, GetCreditDominantPeriodAmount(x)))
            .Where(x => x.Amount != 0m)
            .ToList();

        var costRows = rows
            .Where(x => IsCostOfSalesCode(x.AccountCode))
            .Select(x => MapIncomeRow(x, GetDebitDominantPeriodAmount(x)))
            .Where(x => x.Amount != 0m)
            .ToList();

        var operatingExpenseRows = rows
            .Where(x => IsOperatingExpenseCode(x.AccountCode))
            .Select(x => MapIncomeRow(x, GetDebitDominantPeriodAmount(x)))
            .Where(x => x.Amount != 0m)
            .ToList();

        var otherIncomeRows = rows
            .Where(x => IsOtherIncomeCode(x.AccountCode))
            .Select(x => MapIncomeRow(x, GetCreditDominantPeriodAmount(x)))
            .Where(x => x.Amount != 0m)
            .ToList();

        var otherExpenseRows = rows
            .Where(x => IsOtherExpenseCode(x.AccountCode))
            .Select(x => MapIncomeRow(x, GetDebitDominantPeriodAmount(x)))
            .Where(x => x.Amount != 0m)
            .ToList();

        var sections = new List<IncomeStatementSectionDto>
        {
            BuildIncomeSection(RevenueCode, RevenueName, revenueRows),
            BuildIncomeSection(CostOfSalesCode, CostOfSalesName, costRows),
            BuildIncomeSection(OperatingExpenseCode, OperatingExpenseName, operatingExpenseRows),
            BuildIncomeSection(OtherIncomeCode, OtherIncomeName, otherIncomeRows),
            BuildIncomeSection(OtherExpenseCode, OtherExpenseName, otherExpenseRows)
        };

        var revenueTotal = sections[0].Total;
        var costOfSalesTotal = sections[1].Total;
        var operatingExpenseTotal = sections[2].Total;
        var otherIncomeTotal = sections[3].Total;
        var otherExpenseTotal = sections[4].Total;
        var grossProfit = revenueTotal - costOfSalesTotal;
        var operatingProfit = grossProfit - operatingExpenseTotal + otherIncomeTotal - otherExpenseTotal;

        return new IncomeStatementDto
        {
            PeriodId = filter.PeriodId,
            DateFrom = validation.DateFrom,
            DateTo = validation.DateTo,
            CurrencyId = filter.CurrencyId,
            RevenueTotal = revenueTotal,
            CostOfSalesTotal = costOfSalesTotal,
            OperatingExpenseTotal = operatingExpenseTotal,
            OtherIncomeTotal = otherIncomeTotal,
            OtherExpenseTotal = otherExpenseTotal,
            GrossProfit = grossProfit,
            OperatingProfit = operatingProfit,
            NetProfit = operatingProfit,
            Sections = sections
        };
    }

    private static AccountTurnoverRowDto MapTurnoverRow(TrialBalanceReadRow row)
    {
        var openingNet = row.OpeningDebitTurnover - row.OpeningCreditTurnover;
        var closingNet = GetClosingNet(row);

        return new AccountTurnoverRowDto
        {
            AccountId = row.AccountId,
            AccountCode = row.AccountCode,
            AccountNumber = row.AccountNumber,
            AccountName = row.AccountName,
            OpeningDebit = openingNet > 0m ? openingNet : 0m,
            OpeningCredit = openingNet < 0m ? decimal.Abs(openingNet) : 0m,
            PeriodDebit = row.PeriodDebitTurnover,
            PeriodCredit = row.PeriodCreditTurnover,
            ClosingDebit = closingNet > 0m ? closingNet : 0m,
            ClosingCredit = closingNet < 0m ? decimal.Abs(closingNet) : 0m
        };
    }

    private static BalanceSheetSectionDto BuildBalanceSheetSection(string code, string name, List<BalanceSheetRowDto> rows) => new()
    {
        Code = code,
        Name = name,
        Total = rows.Sum(x => x.Balance),
        Rows = rows.OrderBy(x => x.AccountCode, StringComparer.Ordinal).ToList()
    };

    private static IncomeStatementSectionDto BuildIncomeSection(string code, string name, List<IncomeStatementRowDto> rows) => new()
    {
        Code = code,
        Name = name,
        Total = rows.Sum(x => x.Amount),
        Rows = rows.OrderBy(x => x.AccountCode, StringComparer.Ordinal).ToList()
    };

    private static IncomeStatementRowDto MapIncomeRow(TrialBalanceReadRow row, decimal amount) => new()
    {
        AccountId = row.AccountId,
        AccountCode = row.AccountCode,
        AccountName = row.AccountName,
        Amount = amount
    };

    private static AccountCardTransactionDto MapAccountCardTransaction(LedgerTransactionDto transaction) => new()
    {
        Id = transaction.Id,
        PostingDate = transaction.PostingDate,
        JournalNumber = transaction.JournalNumber,
        DocumentNumber = transaction.DocumentNumber,
        DocumentTypeId = transaction.DocumentTypeId,
        DocumentType = transaction.DocumentType,
        Reference = transaction.Reference,
        Description = transaction.Description,
        Debit = transaction.Debit,
        Credit = transaction.Credit,
        RunningBalance = transaction.RunningBalance,
        CurrencyId = transaction.CurrencyId,
        Currency = transaction.Currency,
        OrganizationId = transaction.OrganizationId,
        Organization = transaction.Organization,
        CounterpartyId = transaction.CounterpartyId,
        Counterparty = transaction.Counterparty,
        WarehouseId = transaction.WarehouseId,
        Warehouse = transaction.Warehouse
    };

    private static JournalEntryDto MapJournalEntry(JournalReadRow row) => new()
    {
        Id = row.Id,
        PostingDate = row.PostingDate,
        JournalNumber = row.JournalNumber,
        DocumentNumber = row.DocumentNumber,
        DocumentTypeId = row.DocumentTypeId,
        DocumentType = row.DocumentType,
        Description = row.Description,
        DebitAccountCode = row.DebitAccountCode,
        DebitAccountName = row.DebitAccountName,
        CreditAccountCode = row.CreditAccountCode,
        CreditAccountName = row.CreditAccountName,
        Amount = row.Amount,
        CurrencyId = row.CurrencyId,
        Currency = row.Currency,
        OrganizationId = row.OrganizationId,
        Organization = row.Organization,
        Counterparty = row.Counterparty,
        Warehouse = row.Warehouse
    };

    private static List<BalanceSheetRowDto>? ResolveBalanceSheetTarget(
        TrialBalanceReadRow row,
        decimal net,
        List<BalanceSheetRowDto> assetRows,
        List<BalanceSheetRowDto> liabilityRows,
        List<BalanceSheetRowDto> equityRows)
    {
        if (IsIncomeStatementCode(row.AccountCode))
            return null;

        if (IsEquityCode(row.AccountCode))
            return net <= 0m ? equityRows : assetRows;

        return row.AccountTypeId switch
        {
            AccountTypeIdConst.Active => net >= 0m ? assetRows : liabilityRows,
            AccountTypeIdConst.Passive => net <= 0m ? liabilityRows : assetRows,
            AccountTypeIdConst.ActivePassive => net >= 0m ? assetRows : liabilityRows,
            _ => IsAssetCode(row.AccountCode) ? assetRows :
                IsLiabilityCode(row.AccountCode) ? liabilityRows :
                IsEquityCode(row.AccountCode) ? equityRows :
                null
        };
    }

    private static decimal GetClosingNet(TrialBalanceReadRow row) =>
        row.OpeningDebitTurnover - row.OpeningCreditTurnover + row.PeriodDebitTurnover - row.PeriodCreditTurnover;

    private static decimal GetDebitDominantPeriodAmount(TrialBalanceReadRow row) =>
        row.PeriodDebitTurnover - row.PeriodCreditTurnover;

    private static decimal GetCreditDominantPeriodAmount(TrialBalanceReadRow row) =>
        row.PeriodCreditTurnover - row.PeriodDebitTurnover;

    private static bool HasAnyTurnover(AccountTurnoverRowDto item) =>
        item.OpeningDebit != 0m ||
        item.OpeningCredit != 0m ||
        item.PeriodDebit != 0m ||
        item.PeriodCredit != 0m ||
        item.ClosingDebit != 0m ||
        item.ClosingCredit != 0m;

    private static string NormalizeAccountCode(string? accountCode) =>
        string.IsNullOrWhiteSpace(accountCode)
            ? string.Empty
            : accountCode.Trim().Split('.', 2)[0];

    private static bool IsAssetCode(string accountCode) =>
        NormalizeAccountCode(accountCode) is var code &&
        code.Length > 0 &&
        code[0] is '1' or '2' or '3' or '4' or '5';

    private static bool IsLiabilityCode(string accountCode) =>
        NormalizeAccountCode(accountCode) is var code &&
        code.Length > 0 &&
        code[0] is '6' or '7';

    private static bool IsEquityCode(string accountCode) =>
        NormalizeAccountCode(accountCode).StartsWith("8", StringComparison.Ordinal);

    private static bool IsRevenueCode(string accountCode) =>
        NormalizeAccountCode(accountCode).StartsWith("90", StringComparison.Ordinal);

    private static bool IsCostOfSalesCode(string accountCode) =>
        NormalizeAccountCode(accountCode).StartsWith("91", StringComparison.Ordinal);

    private static bool IsOperatingExpenseCode(string accountCode) =>
        NormalizeAccountCode(accountCode).StartsWith("94", StringComparison.Ordinal);

    private static bool IsOtherIncomeCode(string accountCode) =>
        NormalizeAccountCode(accountCode).StartsWith("93", StringComparison.Ordinal);

    private static bool IsOtherExpenseCode(string accountCode)
    {
        var code = NormalizeAccountCode(accountCode);
        return code.StartsWith("95", StringComparison.Ordinal) ||
               code.StartsWith("96", StringComparison.Ordinal) ||
               code.StartsWith("97", StringComparison.Ordinal) ||
               code.StartsWith("98", StringComparison.Ordinal) ||
               code.StartsWith("99", StringComparison.Ordinal);
    }

    private static bool IsIncomeStatementCode(string accountCode) =>
        NormalizeAccountCode(accountCode).StartsWith("9", StringComparison.Ordinal);

    private static string ClassifyCashFlowSection(string counterpartAccountCode)
    {
        var code = NormalizeAccountCode(counterpartAccountCode);

        if (code.StartsWith("5", StringComparison.Ordinal))
            return TransfersCode;

        if (code.StartsWith("66", StringComparison.Ordinal) ||
            code.StartsWith("67", StringComparison.Ordinal) ||
            code.StartsWith("68", StringComparison.Ordinal) ||
            code.StartsWith("7", StringComparison.Ordinal) ||
            code.StartsWith("8", StringComparison.Ordinal))
        {
            return FinancingCode;
        }

        if (code.StartsWith("0", StringComparison.Ordinal) ||
            code.StartsWith("1", StringComparison.Ordinal) ||
            code.StartsWith("2", StringComparison.Ordinal) ||
            code.StartsWith("3", StringComparison.Ordinal) ||
            code.StartsWith("47", StringComparison.Ordinal))
        {
            return InvestingCode;
        }

        return OperatingCode;
    }

    private sealed class ReportValidationState
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public short? CurrencyId { get; set; }
    }

    private const string AssetsCode = "ASSETS";
    private const string LiabilitiesCode = "LIABILITIES";
    private const string EquityCode = "EQUITY";
    private const string AssetsName = "Assets";
    private const string LiabilitiesName = "Liabilities";
    private const string EquityName = "Equity";
    private const string CurrentPeriodResultCode = "CURRENT_RESULT";
    private const string CurrentPeriodResultName = "Current period result";

    private const string RevenueCode = "REVENUE";
    private const string CostOfSalesCode = "COST_OF_SALES";
    private const string OperatingExpenseCode = "OPERATING_EXPENSE";
    private const string OtherIncomeCode = "OTHER_INCOME";
    private const string OtherExpenseCode = "OTHER_EXPENSE";
    private const string RevenueName = "Revenue";
    private const string CostOfSalesName = "Cost of sales";
    private const string OperatingExpenseName = "Operating expenses";
    private const string OtherIncomeName = "Other income";
    private const string OtherExpenseName = "Other expenses";

    private const string OperatingCode = "OPERATING";
    private const string InvestingCode = "INVESTING";
    private const string FinancingCode = "FINANCING";
    private const string TransfersCode = "TRANSFERS";
    private const string OperatingName = "Operating activities";
    private const string InvestingName = "Investing activities";
    private const string FinancingName = "Financing activities";
    private const string TransfersName = "Cash transfers";
}
