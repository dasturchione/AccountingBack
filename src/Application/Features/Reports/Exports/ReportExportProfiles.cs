using Application.Features.BankOperations;
using Application.Features.CashOperations;
using Application.Features.AccountingReports;
using Application.Features.CounterpartyRegisterBalances;
using Application.Features.PurchaseDocs;
using Application.Features.SaleDocs;
using Application.Features.WarehouseTransfers;
using Application.Features.InventoryCounts;
using Application.Features.Reports.FinancialReports;

namespace Application.Features.Reports.Exports;

/// <summary>
/// Standard export column factories per report type.
/// </summary>
public static class ReportExportProfiles
{
    public static IReadOnlyCollection<ReportExportColumn> SaleDocs => [
        new() { Header = "Id", ValueFactory = x => ((SaleDocListDto)x).Id },
        new() { Header = "DocNumber", ValueFactory = x => ((SaleDocListDto)x).DocNumber },
        new() { Header = "DocDate", ValueFactory = x => ((SaleDocListDto)x).DocDate },
        new() { Header = "Counterparty", ValueFactory = x => ((SaleDocListDto)x).CounterpartyName },
        new() { Header = "TotalAmount", ValueFactory = x => ((SaleDocListDto)x).TotalAmount },
        new() { Header = "FinalAmount", ValueFactory = x => ((SaleDocListDto)x).FinalAmount }
    ];

    public static IReadOnlyCollection<ReportExportColumn> PurchaseDocs => [
        new() { Header = "Id", ValueFactory = x => ((PurchaseDocListDto)x).Id },
        new() { Header = "DocNumber", ValueFactory = x => ((PurchaseDocListDto)x).DocNumber },
        new() { Header = "DocDate", ValueFactory = x => ((PurchaseDocListDto)x).DocDate },
        new() { Header = "Counterparty", ValueFactory = x => ((PurchaseDocListDto)x).CounterpartyName },
        new() { Header = "TotalAmount", ValueFactory = x => ((PurchaseDocListDto)x).TotalAmount },
        new() { Header = "FinalAmount", ValueFactory = x => ((PurchaseDocListDto)x).FinalAmount }
    ];

    public static IReadOnlyCollection<ReportExportColumn> WarehouseTransfers => [
        new() { Header = "Id", ValueFactory = x => ((WarehouseTransferListDto)x).Id },
        new() { Header = "DocNumber", ValueFactory = x => ((WarehouseTransferListDto)x).DocNumber },
        new() { Header = "DocDate", ValueFactory = x => ((WarehouseTransferListDto)x).DocDate },
        new() { Header = "SourceWarehouse", ValueFactory = x => ((WarehouseTransferListDto)x).SourceWarehouseName },
        new() { Header = "DestinationWarehouse", ValueFactory = x => ((WarehouseTransferListDto)x).DestinationWarehouseName }
    ];

    public static IReadOnlyCollection<ReportExportColumn> InventoryCounts => [
        new() { Header = "Id", ValueFactory = x => ((InventoryCountListDto)x).Id },
        new() { Header = "DocNumber", ValueFactory = x => ((InventoryCountListDto)x).DocNumber },
        new() { Header = "DocDate", ValueFactory = x => ((InventoryCountListDto)x).DocDate },
        new() { Header = "Warehouse", ValueFactory = x => ((InventoryCountListDto)x).WarehouseName }
    ];

    public static IReadOnlyCollection<ReportExportColumn> CashOperations => [
        new() { Header = "Id", ValueFactory = x => ((CashOperationListDto)x).Id },
        new() { Header = "DocNumber", ValueFactory = x => ((CashOperationListDto)x).DocNumber },
        new() { Header = "DocDate", ValueFactory = x => ((CashOperationListDto)x).DocDate },
        new() { Header = "CashBox", ValueFactory = x => ((CashOperationListDto)x).CashBoxName },
        new() { Header = "Amount", ValueFactory = x => ((CashOperationListDto)x).Amount }
    ];

    public static IReadOnlyCollection<ReportExportColumn> BankOperations => [
        new() { Header = "Id", ValueFactory = x => ((BankOperationListDto)x).Id },
        new() { Header = "DocNumber", ValueFactory = x => ((BankOperationListDto)x).DocNumber },
        new() { Header = "DocDate", ValueFactory = x => ((BankOperationListDto)x).DocDate },
        new() { Header = "Bank", ValueFactory = x => ((BankOperationListDto)x).BankName },
        new() { Header = "Amount", ValueFactory = x => ((BankOperationListDto)x).Amount }
    ];

    public static IReadOnlyCollection<ReportExportColumn> CounterpartyBalances => [
        new() { Header = "Id", ValueFactory = x => ((CounterpartyRegisterBalanceListDto)x).Id },
        new() { Header = "CounterpartyId", ValueFactory = x => ((CounterpartyRegisterBalanceListDto)x).CounterpartyId },
        new() { Header = "Amount", ValueFactory = x => ((CounterpartyRegisterBalanceListDto)x).Amount },
        new() { Header = "DocDate", ValueFactory = x => ((CounterpartyRegisterBalanceListDto)x).DocDate }
    ];

    public static IReadOnlyCollection<ReportExportColumn> FinancialTurnover => [
        new() { Header = "AccountId", ValueFactory = x => ((FinancialTurnoverItemDto)x).AccountId },
        new() { Header = "AccountCode", ValueFactory = x => ((FinancialTurnoverItemDto)x).AccountCode },
        new() { Header = "AccountName", ValueFactory = x => ((FinancialTurnoverItemDto)x).AccountName },
        new() { Header = "OpeningDebit", ValueFactory = x => ((FinancialTurnoverItemDto)x).OpeningDebit },
        new() { Header = "OpeningCredit", ValueFactory = x => ((FinancialTurnoverItemDto)x).OpeningCredit },
        new() { Header = "PeriodDebit", ValueFactory = x => ((FinancialTurnoverItemDto)x).PeriodDebit },
        new() { Header = "PeriodCredit", ValueFactory = x => ((FinancialTurnoverItemDto)x).PeriodCredit },
        new() { Header = "ClosingDebit", ValueFactory = x => ((FinancialTurnoverItemDto)x).ClosingDebit },
        new() { Header = "ClosingCredit", ValueFactory = x => ((FinancialTurnoverItemDto)x).ClosingCredit }
    ];

    public static IReadOnlyCollection<ReportExportColumn> FinancialBalanceSheet => [
        new() { Header = "PeriodId", ValueFactory = x => ((BalanceSheetDto)x).PeriodId },
        new() { Header = "DateFrom", ValueFactory = x => ((BalanceSheetDto)x).DateFrom },
        new() { Header = "DateTo", ValueFactory = x => ((BalanceSheetDto)x).DateTo },
        new() { Header = "CurrencyId", ValueFactory = x => ((BalanceSheetDto)x).CurrencyId },
        new() { Header = "TotalAssets", ValueFactory = x => ((BalanceSheetDto)x).TotalAssets },
        new() { Header = "TotalLiabilities", ValueFactory = x => ((BalanceSheetDto)x).TotalLiabilities },
        new() { Header = "TotalEquity", ValueFactory = x => ((BalanceSheetDto)x).TotalEquity },
        new() { Header = "TotalLiabilitiesAndEquity", ValueFactory = x => ((BalanceSheetDto)x).TotalLiabilitiesAndEquity }
    ];

    public static IReadOnlyCollection<ReportExportColumn> FinancialIncomeStatement => [
        new() { Header = "PeriodId", ValueFactory = x => ((IncomeStatementDto)x).PeriodId },
        new() { Header = "DateFrom", ValueFactory = x => ((IncomeStatementDto)x).DateFrom },
        new() { Header = "DateTo", ValueFactory = x => ((IncomeStatementDto)x).DateTo },
        new() { Header = "CurrencyId", ValueFactory = x => ((IncomeStatementDto)x).CurrencyId },
        new() { Header = "RevenueTotal", ValueFactory = x => ((IncomeStatementDto)x).RevenueTotal },
        new() { Header = "CostOfSalesTotal", ValueFactory = x => ((IncomeStatementDto)x).CostOfSalesTotal },
        new() { Header = "OperatingExpenseTotal", ValueFactory = x => ((IncomeStatementDto)x).OperatingExpenseTotal },
        new() { Header = "OtherIncomeTotal", ValueFactory = x => ((IncomeStatementDto)x).OtherIncomeTotal },
        new() { Header = "OtherExpenseTotal", ValueFactory = x => ((IncomeStatementDto)x).OtherExpenseTotal },
        new() { Header = "GrossProfit", ValueFactory = x => ((IncomeStatementDto)x).GrossProfit },
        new() { Header = "OperatingProfit", ValueFactory = x => ((IncomeStatementDto)x).OperatingProfit },
        new() { Header = "NetProfit", ValueFactory = x => ((IncomeStatementDto)x).NetProfit }
    ];

    public static IReadOnlyCollection<ReportExportColumn> FinancialCashFlow => [
        new() { Header = "PeriodId", ValueFactory = x => ((CashFlowDto)x).PeriodId },
        new() { Header = "DateFrom", ValueFactory = x => ((CashFlowDto)x).DateFrom },
        new() { Header = "DateTo", ValueFactory = x => ((CashFlowDto)x).DateTo },
        new() { Header = "CurrencyId", ValueFactory = x => ((CashFlowDto)x).CurrencyId },
        new() { Header = "OpeningCashBalance", ValueFactory = x => ((CashFlowDto)x).OpeningCashBalance },
        new() { Header = "ClosingCashBalance", ValueFactory = x => ((CashFlowDto)x).ClosingCashBalance }
    ];

    public static IReadOnlyCollection<ReportExportColumn> FinancialCardTransactions => [
        new() { Header = "Id", ValueFactory = x => ((AccountCardTransactionDto)x).Id },
        new() { Header = "PostingDate", ValueFactory = x => ((AccountCardTransactionDto)x).PostingDate },
        new() { Header = "JournalNumber", ValueFactory = x => ((AccountCardTransactionDto)x).JournalNumber },
        new() { Header = "DocumentNumber", ValueFactory = x => ((AccountCardTransactionDto)x).DocumentNumber },
        new() { Header = "DocumentType", ValueFactory = x => ((AccountCardTransactionDto)x).DocumentType },
        new() { Header = "Description", ValueFactory = x => ((AccountCardTransactionDto)x).Description },
        new() { Header = "Debit", ValueFactory = x => ((AccountCardTransactionDto)x).Debit },
        new() { Header = "Credit", ValueFactory = x => ((AccountCardTransactionDto)x).Credit },
        new() { Header = "RunningBalance", ValueFactory = x => ((AccountCardTransactionDto)x).RunningBalance },
        new() { Header = "Currency", ValueFactory = x => ((AccountCardTransactionDto)x).Currency },
        new() { Header = "Organization", ValueFactory = x => ((AccountCardTransactionDto)x).Organization },
        new() { Header = "Counterparty", ValueFactory = x => ((AccountCardTransactionDto)x).Counterparty },
        new() { Header = "Warehouse", ValueFactory = x => ((AccountCardTransactionDto)x).Warehouse }
    ];

    public static IReadOnlyCollection<ReportExportColumn> FinancialJournalEntries => [
        new() { Header = "Id", ValueFactory = x => ((JournalEntryDto)x).Id },
        new() { Header = "PostingDate", ValueFactory = x => ((JournalEntryDto)x).PostingDate },
        new() { Header = "JournalNumber", ValueFactory = x => ((JournalEntryDto)x).JournalNumber },
        new() { Header = "DocumentNumber", ValueFactory = x => ((JournalEntryDto)x).DocumentNumber },
        new() { Header = "DocumentType", ValueFactory = x => ((JournalEntryDto)x).DocumentType },
        new() { Header = "Description", ValueFactory = x => ((JournalEntryDto)x).Description },
        new() { Header = "DebitAccountCode", ValueFactory = x => ((JournalEntryDto)x).DebitAccountCode },
        new() { Header = "DebitAccountName", ValueFactory = x => ((JournalEntryDto)x).DebitAccountName },
        new() { Header = "CreditAccountCode", ValueFactory = x => ((JournalEntryDto)x).CreditAccountCode },
        new() { Header = "CreditAccountName", ValueFactory = x => ((JournalEntryDto)x).CreditAccountName },
        new() { Header = "Amount", ValueFactory = x => ((JournalEntryDto)x).Amount },
        new() { Header = "Currency", ValueFactory = x => ((JournalEntryDto)x).Currency },
        new() { Header = "Organization", ValueFactory = x => ((JournalEntryDto)x).Organization },
        new() { Header = "Counterparty", ValueFactory = x => ((JournalEntryDto)x).Counterparty },
        new() { Header = "Warehouse", ValueFactory = x => ((JournalEntryDto)x).Warehouse }
    ];
}
