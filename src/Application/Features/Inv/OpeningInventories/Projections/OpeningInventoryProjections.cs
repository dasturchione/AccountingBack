using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Inv.OpeningInventories;

public sealed class OpeningInventoryListDtoProjection : IProjectionBuilder<OpeningInventory, OpeningInventoryListDto>
{
    public Expression<Func<OpeningInventory, OpeningInventoryListDto>> Build() =>
        x => new OpeningInventoryListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            DocNumber = x.DocNumber,
            DocDate = x.DocDate,
            CounterpartyId = x.CounterpartyId,
            CounterpartyName = x.Counterparty.ShortName,
            ContractId = x.ContractId,
            ContractNumber = x.Contract == null ? null : x.Contract.ContractNumber,
            WarehouseId = x.WarehouseId,
            WarehouseName = x.Warehouse.Name,
            TotalAmount = x.TotalAmount,
            StatusId = x.StatusId,
            StatusName = x.Status.Name,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate,
            PostedAt = x.PostedAt
        };
}

public sealed class OpeningInventoryDtoProjection : IProjectionBuilder<OpeningInventory, OpeningInventoryDto>
{
    public Expression<Func<OpeningInventory, OpeningInventoryDto>> Build() =>
        x => new OpeningInventoryDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            DocNumber = x.DocNumber,
            DocDate = x.DocDate,
            CounterpartyId = x.CounterpartyId,
            CounterpartyName = x.Counterparty.ShortName,
            ContractId = x.ContractId,
            ContractNumber = x.Contract == null ? null : x.Contract.ContractNumber,
            WarehouseId = x.WarehouseId,
            WarehouseName = x.Warehouse.Name,
            TotalAmount = x.TotalAmount,
            StatusId = x.StatusId,
            StatusName = x.Status.Name,
            Comment = x.Comment,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate,
            PostedAt = x.PostedAt,
            PostedByUserId = x.PostedByUserId,
            Lines = x.OpeningInventoryProducts.Select(line => new OpeningInventoryProductDto
            {
                Id = line.Id,
                ProductId = line.ProductId,
                ProductName = line.Product.Name,
                ProductMxik = line.Product.Mxik,
                Quantity = line.Quantity,
                UnitId = line.UnitId,
                UnitName = line.Unit.Name,
                UnitPrice = line.UnitPrice,
                Amount = line.Amount,
                DebitAccountId = line.DebitAccountId,
                DebitAccountNumber = line.DebitAccount.Number,
                DebitAccountName = line.DebitAccount.Name,
                WarehouseBatchId = line.Product.WarehouseProductBatches
                    .Where(batch => batch.ReceiptMovement.DocumentTypeId == SharedKernel.Constants.DocumentTypeIdConst.OPENINGINVENTORY &&
                                    batch.ReceiptMovement.DocumentId == x.Id)
                    .Select(batch => (long?)batch.Id)
                    .FirstOrDefault(),
                Items = line.OpeningInventoryTables.Select(item => new OpeningInventoryItemDto
                {
                    Id = item.Id,
                    ProductTableId = item.ProductTableId,
                    MarkingNumber = item.ProductTable.MarkingNumber,
                    SerialNumber = item.ProductTable.SerialNumber,
                    Amount = item.Amount
                }).ToList()
            }).ToList()
        };
}
