using SharedKernel.Filters;

namespace Application.Features.SaleShipments;

public sealed class SaleShipmentFilter : ISearchFilter, IPaginationFilter
{
    public int? WarehouseId { get; set; }
    public int? CounterpartyId { get; set; }
    public short? StatusId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
