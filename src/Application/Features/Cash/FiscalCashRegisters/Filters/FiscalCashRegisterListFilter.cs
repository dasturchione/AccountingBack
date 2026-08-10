using SharedKernel.Filters;

namespace Application.Features.FiscalCashRegisters;

public class FiscalCashRegisterListFilter : ISearchFilter, IPaginationFilter
{
    public int? WarehouseId { get; set; }
    public short? RegisterTypeId { get; set; }
    public short? StateId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}
