using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.InventoryRegisterBalances;

public class InventoryRegisterBalanceDtoProjection : IProjectionBuilder<RegisterBalance, InventoryRegisterBalanceDto>
{
    public Expression<Func<RegisterBalance, InventoryRegisterBalanceDto>> Build() =>
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
            PostingBatchId = x.PostingBatchId,
            SourceLineId = x.SourceLineId,
            ReversalEntryId = x.ReversalEntryId,
            CreatedDate = x.CreatedDate
        };
}

public class InventoryRegisterBalanceListDtoProjection : IProjectionBuilder<RegisterBalance, InventoryRegisterBalanceListDto>
{
    public Expression<Func<RegisterBalance, InventoryRegisterBalanceListDto>> Build() =>
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
