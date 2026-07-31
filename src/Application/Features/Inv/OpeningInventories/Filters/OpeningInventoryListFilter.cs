using SharedKernel.Filters;

namespace Application.Features.Inv.OpeningInventories;

public sealed class OpeningInventoryListFilter : ISearchFilter, IPaginationFilter, ISortFilter
{
    public int? CounterpartyId { get; set; }
    public int? WarehouseId { get; set; }
    public short? StatusId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public SortDirection SortDirection { get; set; } = SortDirection.Desc;
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
