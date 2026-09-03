namespace Application.Features.AccountingRegisterEntries;

public sealed record AccountingRegisterEntryRebuildSource(
    object Document,
    long? PostingBatchId);
