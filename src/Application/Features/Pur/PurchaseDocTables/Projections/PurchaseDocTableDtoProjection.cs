using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.PurchaseDocTables;

public class PurchaseDocTableDtoProjection : IProjectionBuilder<PurchaseDocTable, PurchaseDocTableDto>
{
    public Expression<Func<PurchaseDocTable, PurchaseDocTableDto>> Build() =>
        x => new PurchaseDocTableDto
        {
            Id                 = x.Id,
            OwnerId            = x.OwnerId,
            ItemTypeId         = x.ItemTypeId,
            ProductTableId     = x.ProductTableId,
            ProductName        = x.ProductTable != null ? x.ProductTable.Product.Name : null,
            Quantity           = x.Quantity,
            Price              = x.Price,
            Amount             = x.Amount,
            VatRateId          = x.VatRateId,
            VatRateName        = x.VatRate != null ? x.VatRate.Name : null,
            VatAmount          = x.VatAmount,
            TotalAmount        = x.TotalAmount,
            MarkingNumber      = x.ProductTable != null ? x.ProductTable.MarkingNumber : null,
            SerialNumber       = x.ProductTable != null ? x.ProductTable.SerialNumber : null,
            ServiceName        = x.ServiceName,
            ExpenseAccountId   = x.ExpenseAccountId,
            ExpenseAccountName = x.ExpenseAccount != null ? x.ExpenseAccount.Name : null,
        };
}
