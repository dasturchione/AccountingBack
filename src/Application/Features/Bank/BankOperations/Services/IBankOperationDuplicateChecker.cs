namespace Application.Features.BankOperations;

public readonly record struct BankOperationIdentity(
    int BankAccountId,
    string BankDocumentNumber,
    DateOnly DocumentDate)
{
    public static BankOperationIdentity? Create(
        int bankAccountId,
        string? bankDocumentNumber,
        DateTime documentDate)
    {
        var normalizedNumber = bankDocumentNumber?.Trim();
        return string.IsNullOrWhiteSpace(normalizedNumber)
            ? null
            : new BankOperationIdentity(
                bankAccountId,
                normalizedNumber,
                DateOnly.FromDateTime(documentDate));
    }
}

public interface IBankOperationDuplicateChecker
{
    Task<IReadOnlySet<BankOperationIdentity>> FindExistingAsync(
        int organizationId,
        IReadOnlyCollection<BankOperationIdentity> identities,
        long? excludedOperationId = null,
        CancellationToken ct = default);
}
