using SharedKernel.Filters;

namespace Application.Features.AccountingRegisterEntries;

public class AccountingRegisterEntryListFilter : IPaginationFilter
{
    public int? OrganizationId { get; set; }
    public short? DocumentTypeId { get; set; }
    public long? DocumentId { get; set; }
    public int? DebitAccountId { get; set; }
    public int? CreditAccountId { get; set; }
    public short? CurrencyId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
