using SharedKernel.Filters;

namespace Application.Features.WarehouseTransfers;

public class WarehouseTransferListFilter : ISearchFilter, IPaginationFilter
{
    public int? SourceWarehouseId { get; set; }
    public int? DestinationWarehouseId { get; set; }
    public short? StatusId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
