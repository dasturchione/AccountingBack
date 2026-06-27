using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.PurchaseDocTables;

public class PurchaseDocTableListDtoProjection : IProjectionBuilder<PurchaseDocTable, PurchaseDocTableListDto>
{
    public Expression<Func<PurchaseDocTable, PurchaseDocTableListDto>> Build() =>
        x => new PurchaseDocTableListDto
        {
            Id                 = x.Id,
            OwnerId            = x.Owner.OwnerId,
            ProductTableId     = x.ProductTableId,
            ProductName        = x.ProductTable.Product.Name,
            Quantity           = 1,
            Price              = x.Owner.UnitPrice,
            Amount             = x.Amount,
            VatRateId          = x.VatRateId,
            VatRateName        = x.VatRate != null ? x.VatRate.Name : null,
            VatAmount          = x.VatAmount,
            TotalAmount        = x.TotalAmount,
        };
}
