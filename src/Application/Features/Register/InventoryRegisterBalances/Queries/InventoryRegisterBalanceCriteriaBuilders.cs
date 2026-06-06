using Domain.Entities;
using SharedKernel.Query;
using SharedKernel.Query.Options;
using System.Linq.Expressions;

namespace Application.Features.InventoryRegisterBalances;

public class InventoryRegisterBalanceByIdCriteriaBuilder : ICriteriaBuilder<InventoryRegisterBalance, GetByIdOptions<long>>
{
    public Expression<Func<InventoryRegisterBalance, bool>> Build(GetByIdOptions<long> options) => x => x.Id == options.Id;
}

public class InventoryRegisterBalanceByListFilterCriteriaBuilder : ICriteriaBuilder<InventoryRegisterBalance, InventoryRegisterBalanceListFilter>
{
    public Expression<Func<InventoryRegisterBalance, bool>> Build(InventoryRegisterBalanceListFilter options) =>
        x => (!options.OrganizationId.HasValue || x.OrganizationId == options.OrganizationId.Value) &&
             (!options.DocumentTypeId.HasValue || x.DocumentTypeId == options.DocumentTypeId.Value) &&
             (!options.DocumentId.HasValue || x.DocumentId == options.DocumentId.Value) &&
             (!options.WarehouseId.HasValue || x.WarehouseId == options.WarehouseId.Value) &&
             (!options.ProductId.HasValue || x.ProductId == options.ProductId.Value) &&
             (!options.OperationTypeId.HasValue || x.OperationTypeId == options.OperationTypeId.Value) &&
             (options.DateFrom == null || x.DocDate >= options.DateFrom) &&
             (options.DateTo == null || x.DocDate <= options.DateTo);
}
