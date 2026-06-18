using SharedKernel.Filters;

namespace Application.Features.InventoryRegisterBalances;

public class InventoryRegisterBalanceListFilter : IPaginationFilter
{
    public short? DocumentTypeId { get; set; }
    public long? DocumentId { get; set; }
    public int? WarehouseId { get; set; }
    public int? ProductId { get; set; }
    public short? OperationTypeId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
