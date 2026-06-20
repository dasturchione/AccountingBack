using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.SaleDocTables;

public class SaleDocTableDtoProjection : IProjectionBuilder<SaleDocTable, SaleDocTableDto>
{
    public Expression<Func<SaleDocTable, SaleDocTableDto>> Build() =>
        x => new SaleDocTableDto
        {
            Id = x.Id,
            OwnerId = x.OwnerId,
            ProductTableId = x.ProductTableId,
            ProductId = x.ProductTable.ProductId,
            ProductName = x.ProductTable.Product.Name,
            Quantity = x.Quantity,
            CostPrice = x.CostPrice,
            Price = x.Price,
            Amount = x.Amount,
            VatRateId = x.VatRateId,
            VatRateName = x.VatRate != null ? x.VatRate.Name : null,
            VatAmount = x.VatAmount,
            TotalAmount = x.TotalAmount
        };
}
