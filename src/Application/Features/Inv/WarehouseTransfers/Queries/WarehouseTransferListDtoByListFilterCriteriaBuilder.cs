using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.WarehouseTransfers;

public class WarehouseTransferListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<WarehouseTransferListDto, WarehouseTransferListFilter>
{
    public Expression<Func<WarehouseTransferListDto, bool>> Build(WarehouseTransferListFilter options) =>
        x => (!options.SourceWarehouseId.HasValue || x.SourceWarehouseId == options.SourceWarehouseId.Value) &&
             (!options.DestinationWarehouseId.HasValue || x.DestinationWarehouseId == options.DestinationWarehouseId.Value) &&
             (!options.StatusId.HasValue || x.StatusId == options.StatusId.Value) &&
             (options.DateFrom == null || x.DocDate >= options.DateFrom) &&
             (options.DateTo == null || x.DocDate <= options.DateTo) &&
             (string.IsNullOrWhiteSpace(options.Search) || x.DocNumber.Contains(options.Search));
}
