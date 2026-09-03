namespace Application.Features.AccountingRegisterEntries;

public interface IAccountingRegisterEntryRebuildRepository
{
    bool Supports(short documentTypeId);

    Task<AccountingRegisterEntryRebuildSource?> GetSourceAsync(
        short documentTypeId,
        long documentId,
        int organizationId,
        CancellationToken ct = default);

    Task<int> DeleteEntriesAsync(
        short documentTypeId,
        long documentId,
        int organizationId,
        CancellationToken ct = default);
}
