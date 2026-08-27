namespace Application.Features.AccountingRegisterEntries;

public sealed class AccountingRegisterEntryRebuildDto
{
    public short DocumentTypeId { get; set; }
    public long DocumentId { get; set; }
    public int DeletedCount { get; set; }
    public int CreatedCount { get; set; }
}
