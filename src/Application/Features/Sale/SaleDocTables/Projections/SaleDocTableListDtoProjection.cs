using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.SaleDocTables;

public class SaleDocTableListDtoProjection : IProjectionBuilder<SaleDocTable, SaleDocTableListDto>
{
    public Expression<Func<SaleDocTable, SaleDocTableListDto>> Build() =>
        x => new SaleDocTableListDto
        {
            Id = x.Id,
            OwnerId = x.OwnerId,
            ProductTableId = x.ProductTableId,
            ProductName = x.ProductTable.Product.Name,
            Quantity = x.Quantity,
            Price = x.Price,
            Amount = x.Amount,
            VatRateId = x.VatRateId,
            VatRateName = x.VatRate != null ? x.VatRate.Name : null,
            VatAmount = x.VatAmount,
            TotalAmount = x.TotalAmount
        };
}
