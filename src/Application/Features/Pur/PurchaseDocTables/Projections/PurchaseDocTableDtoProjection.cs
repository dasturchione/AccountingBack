using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.PurchaseDocTables;

public class PurchaseDocTableDtoProjection : IProjectionBuilder<PurchaseDocTable, PurchaseDocTableDto>
{
    public Expression<Func<PurchaseDocTable, PurchaseDocTableDto>> Build() =>
        x => new PurchaseDocTableDto
        {
            Id = x.Id,
            OwnerId = x.OwnerId,
            ProductId = x.ProductId,
            ProductName = x.Product.Name,
            Quantity = x.Quantity,
            Price = x.Price,
            Amount = x.Amount,
            VatRateId = x.VatRateId,
            VatRateName = x.VatRate != null ? x.VatRate.Name : null,
            VatAmount = x.VatAmount,
            TotalAmount = x.TotalAmount
        };
}
