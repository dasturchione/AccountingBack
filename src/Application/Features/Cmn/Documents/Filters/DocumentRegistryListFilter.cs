using SharedKernel.Constants;
using SharedKernel.Filters;

namespace Application.Features.Cmn.Documents;

public sealed class DocumentRegistryListFilter : ISearchFilter
{
    public string? DocumentTypeCode { get; set; }
    public short? CurrencyId { get; set; }
    public short? StatusId { get; set; }
    public short? StateId { get; set; } = StateIdConst.ACTIVE;
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Search { get; set; }
}
