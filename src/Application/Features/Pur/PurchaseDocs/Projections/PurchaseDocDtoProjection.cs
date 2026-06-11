using Application.Features.PurchaseDocTables;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.PurchaseDocs;

public class PurchaseDocDtoProjection : IProjectionBuilder<PurchaseDoc, PurchaseDocDto>
{
    public Expression<Func<PurchaseDoc, PurchaseDocDto>> Build() =>
        x => new PurchaseDocDto
        {
            Id               = x.Id,
            OrganizationId   = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            DocNumber        = x.DocNumber,
            DocDate          = x.DocDate,
            CounterpartyId   = x.CounterpartyId,
            CounterpartyName = x.Counterparty.ShortName,
            WarehouseId      = x.WarehouseId,
            WarehouseName    = x.Warehouse.Name,
            CurrencyId       = x.CurrencyId,
            CurrencyName     = x.Currency.Name,
            TotalAmount      = x.TotalAmount,
            VatAmount        = x.VatAmount,
            FinalAmount      = x.FinalAmount,
            StatusId         = x.StatusId,
            StatusName       = x.Status.Name,
            Comment          = x.Comment,
            StateId          = x.StateId,
            StateName        = x.State.FullName,
            CreatedDate      = x.CreatedDate,
            Lines = x.Lines.Select(l => new PurchaseDocTableDto
            {
                Id          = l.Id,
                OwnerId     = l.OwnerId,
                ProductTableId = l.ProductTableId,
                ProductName = l.ProductTable.Product.Name,
                Quantity    = l.Quantity,
                Price       = l.Price,
                Amount      = l.Amount,
                VatRateId   = l.VatRateId,
                VatRateName = l.VatRate != null ? l.VatRate.Name : null,
                VatAmount   = l.VatAmount,
                TotalAmount = l.TotalAmount
            }).ToList()
        };
}
