using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.InventoryRegisterBalances;

public class InventoryRegisterBalanceByListFilterCriteriaBuilder : ICriteriaBuilder<RegisterBalance, InventoryRegisterBalanceListFilter>
{
    public Expression<Func<RegisterBalance, bool>> Build(InventoryRegisterBalanceListFilter options) =>
        x => (!options.DocumentTypeId.HasValue || x.DocumentTypeId == options.DocumentTypeId.Value) &&
             (!options.DocumentId.HasValue || x.DocumentId == options.DocumentId.Value) &&
             (!options.WarehouseId.HasValue || x.WarehouseId == options.WarehouseId.Value) &&
             (!options.ProductId.HasValue || x.ProductId == options.ProductId.Value) &&
             (!options.OperationTypeId.HasValue || x.OperationTypeId == options.OperationTypeId.Value) &&
             (options.DateFrom == null || x.DocDate >= options.DateFrom) &&
             (options.DateTo == null || x.DocDate <= options.DateTo);
}
