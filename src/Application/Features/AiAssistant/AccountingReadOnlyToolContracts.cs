namespace Application.Features.AiAssistant;

public static class AccountingToolNames
{
    public const string ContractExpiryLookup = "accounting.contract-expiry.lookup";
    public const string SupplierOverdueLookup = "accounting.supplier-overdue.lookup";
    public const string CustomerOverdueLookup = "accounting.customer-overdue.lookup";
    public const string TrialBalanceWarningLookup = "accounting.trial-balance-warning.lookup";
    public const string EdoMappingIssueLookup = "accounting.edo-mapping-issue.lookup";
}

public static class AccountingReadOnlyToolCatalog
{
    public static IReadOnlyList<AiToolDescriptor> All { get; } =
    [
        new(AccountingToolNames.ContractExpiryLookup, true, AiToolAuthorizationRequirement.AuthenticatedOrganizationMember),
        new(AccountingToolNames.SupplierOverdueLookup, true, AiToolAuthorizationRequirement.AuthenticatedOrganizationMember),
        new(AccountingToolNames.CustomerOverdueLookup, true, AiToolAuthorizationRequirement.AuthenticatedOrganizationMember),
        new(AccountingToolNames.TrialBalanceWarningLookup, true, AiToolAuthorizationRequirement.AuthenticatedOrganizationMember),
        new(AccountingToolNames.EdoMappingIssueLookup, true, AiToolAuthorizationRequirement.AuthenticatedOrganizationMember)
    ];

    public static bool IsAllowed(string? toolName) =>
        !string.IsNullOrWhiteSpace(toolName)
        && All.Any(tool => tool.Name.Equals(toolName, StringComparison.Ordinal));
}

public sealed record ContractExpiryLookupInput(int DaysAhead = 30) : IAiToolInput
{
    public bool IsValid => DaysAhead is >= 0 and <= 366;
}

public sealed record ContractExpiryLookupItem(
    string ContractNumber,
    string? CounterpartyName,
    DateOnly ExpiryDate,
    int DaysFromToday,
    string StatusLabel);

public sealed record ContractExpiryLookupResult(
    IReadOnlyList<ContractExpiryLookupItem> Items);

public interface IContractExpiryLookupTool
    : IReadOnlyAccountingTool<ContractExpiryLookupInput, ContractExpiryLookupResult>
{
}

public sealed record SupplierOverdueLookupInput(
    DateOnly? AsOfDate = null,
    int? MinimumDaysOverdue = null) : IAiToolInput
{
    public bool IsValid => !MinimumDaysOverdue.HasValue || MinimumDaysOverdue.Value >= 0;
}

public sealed record SupplierOverdueLookupItem(
    string SupplierName,
    string DocumentNumber,
    DateOnly DueDate,
    int DaysOverdue,
    decimal OutstandingAmount,
    string CurrencyName);

public sealed record SupplierOverdueLookupResult(
    IReadOnlyList<SupplierOverdueLookupItem> Items);

public interface ISupplierOverdueLookupTool
    : IReadOnlyAccountingTool<SupplierOverdueLookupInput, SupplierOverdueLookupResult>
{
}

public sealed record CustomerOverdueLookupInput(
    DateOnly? AsOfDate = null,
    int? MinimumDaysOverdue = null) : IAiToolInput
{
    public bool IsValid => !MinimumDaysOverdue.HasValue || MinimumDaysOverdue.Value >= 0;
}

public sealed record CustomerOverdueLookupItem(
    string CustomerName,
    string DocumentNumber,
    DateOnly DueDate,
    int DaysOverdue,
    decimal OutstandingAmount,
    string CurrencyName);

public sealed record CustomerOverdueLookupResult(
    IReadOnlyList<CustomerOverdueLookupItem> Items);

public interface ICustomerOverdueLookupTool
    : IReadOnlyAccountingTool<CustomerOverdueLookupInput, CustomerOverdueLookupResult>
{
}

public sealed record TrialBalanceWarningLookupInput(
    DateOnly FromDate,
    DateOnly ToDate,
    short? CurrencyId = null) : IAiToolInput
{
    public bool IsValid => FromDate <= ToDate && (!CurrencyId.HasValue || CurrencyId.Value > 0);
}

public sealed record TrialBalanceWarningLookupItem(
    string AccountNumber,
    string AccountName,
    decimal Debit,
    decimal Credit,
    string WarningLabel);

public sealed record TrialBalanceWarningLookupResult(
    IReadOnlyList<TrialBalanceWarningLookupItem> Items);

public interface ITrialBalanceWarningLookupTool
    : IReadOnlyAccountingTool<TrialBalanceWarningLookupInput, TrialBalanceWarningLookupResult>
{
}

public sealed record EdoMappingIssueLookupInput(
    DateOnly? FromDate = null,
    DateOnly? ToDate = null) : IAiToolInput
{
    public bool IsValid => !FromDate.HasValue || !ToDate.HasValue || FromDate.Value <= ToDate.Value;
}

public sealed record EdoMappingIssueLookupItem(
    string DocumentNumber,
    string IssueLabel,
    string Description,
    DateTimeOffset OccurredAt);

public sealed record EdoMappingIssueLookupResult(
    IReadOnlyList<EdoMappingIssueLookupItem> Items);

public interface IEdoMappingIssueLookupTool
    : IReadOnlyAccountingTool<EdoMappingIssueLookupInput, EdoMappingIssueLookupResult>
{
}
