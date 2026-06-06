using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.InventoryRegisterBalances;

public class InventoryRegisterBalanceDtoProjection : IProjectionBuilder<InventoryRegisterBalance, InventoryRegisterBalanceDto>
{
    public Expression<Func<InventoryRegisterBalance, InventoryRegisterBalanceDto>> Build() =>
        x => new InventoryRegisterBalanceDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            DocumentTypeId = x.DocumentTypeId,
            DocumentId = x.DocumentId,
            WarehouseId = x.WarehouseId,
            ProductId = x.ProductId,
            OperationTypeId = x.OperationTypeId,
            Quantity = x.Quantity,
            Amount = x.Amount,
            DocDate = x.DocDate,
            CreatedDate = x.CreatedDate
        };
}

public class InventoryRegisterBalanceListDtoProjection : IProjectionBuilder<InventoryRegisterBalance, InventoryRegisterBalanceListDto>
{
    public Expression<Func<InventoryRegisterBalance, InventoryRegisterBalanceListDto>> Build() =>
        x => new InventoryRegisterBalanceListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            DocumentTypeId = x.DocumentTypeId,
            DocumentId = x.DocumentId,
            WarehouseId = x.WarehouseId,
            ProductId = x.ProductId,
            OperationTypeId = x.OperationTypeId,
            Quantity = x.Quantity,
            Amount = x.Amount,
            DocDate = x.DocDate,
            CreatedDate = x.CreatedDate
        };
}
