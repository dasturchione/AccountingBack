using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.SaleShipments;

public sealed class SaleShipmentDtoProjection : IProjectionBuilder<SaleShipmentDoc, SaleShipmentDto>
{
    public Expression<Func<SaleShipmentDoc, SaleShipmentDto>> Build() =>
        shipment => new SaleShipmentDto
        {
            Id = shipment.Id,
            SaleDocId = shipment.SaleDocId,
            WarehouseId = shipment.WarehouseId,
            WarehouseName = shipment.Warehouse.Name,
            CounterpartyId = shipment.CounterpartyId,
            CounterpartyName = shipment.Counterparty == null ? null : shipment.Counterparty.ShortName,
            DocNumber = shipment.DocNumber,
            DocDate = shipment.DocDate,
            Comment = shipment.Comment,
            StatusId = shipment.StatusId,
            StatusName = shipment.Status.Name,
            Products = shipment.SaleShipmentProducts.Select(product => new SaleShipmentProductDto
            {
                Id = product.Id,
                SaleDocProductId = product.SaleDocProductId,
                ProductId = product.ProductId,
                ProductName = product.Product.Name,
                UnitId = product.UnitId,
                UnitName = product.Unit.Name,
                Quantity = product.Quantity,
                Batches = product.SaleShipmentProductBatches.Select(batch => new SaleShipmentProductBatchDto
                {
                    BatchId = batch.BatchId,
                    BatchNumber = batch.Batch.BatchNumber,
                    ReceivedDate = batch.Batch.ReceivedDate,
                    Quantity = batch.Quantity
                }).ToList(),
                ProductTables = product.SaleShipmentTables.Select(table => new SaleShipmentTableDto
                {
                    Id = table.Id,
                    ProductTableId = table.ProductTableId,
                    MarkingNumber = table.ProductTable.MarkingNumber,
                    SerialNumber = table.ProductTable.SerialNumber
                }).ToList()
            }).ToList()
        };
}
