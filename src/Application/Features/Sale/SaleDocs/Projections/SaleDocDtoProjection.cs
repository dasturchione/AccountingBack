using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.SaleDocs;

public class SaleDocDtoProjection : IProjectionBuilder<SaleDoc, SaleDocDto>
{
    public Expression<Func<SaleDoc, SaleDocDto>> Build() =>
        x => new SaleDocDto
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
            CurrencyCode     = x.Currency.Code,
            TotalAmount      = x.TotalAmount,
            VatAmount        = x.VatAmount,
            FinalAmount      = x.FinalAmount,
            ExchangeRate = x.ExchangeRate,
            PostedAt = x.PostedAt,
            PostedByUserId = x.PostedByUserId,
            CancelledAt = x.CancelledAt,
            CancelledByUserId = x.CancelledByUserId,
            StatusId         = x.StatusId,
            StatusName       = x.Status.Name,
            Comment          = x.Comment,
            StateId          = x.StateId,
            StateName        = x.State.FullName,
            CreatedDate      = x.CreatedDate,
            ContractId       = x.ContractId,
            ContractNumber   = x.Contract == null ? null : x.Contract.ContractNumber,
            Lines            = x.SaleDocProducts.Select(p => new SaleDocProductDto
            {
                Id          = p.Id,
                ProductId   = p.ProductId,
                ProductName = p.Product.Name,
                ProductMxik = p.Product.Mxik,
                Quantity    = p.Quantity,
                UnitId      = p.UnitId,
                UnitName    = p.Unit.Name,
                UnitPrice   = p.UnitPrice,
                CostPrice   = p.CostPrice,
                Amount      = p.Amount,
                VatRateId   = p.VatRateId,
                VatRateName = p.VatRate != null ? p.VatRate.Name : null,
                VatAmount   = p.VatAmount,
                TotalAmount = p.TotalAmount,
                Items       = p.SaleDocTables.Select(t => new SaleDocProductTableDto
                {
                    Id             = t.Id,
                    ProductTableId = t.ProductTableId,
                    MarkingNumber  = t.ProductTable.MarkingNumber,
                    SerialNumber   = t.ProductTable.SerialNumber,
                    CostPrice      = t.CostPrice,
                    Amount         = t.Amount,
                    VatRateId      = t.VatRateId,
                    VatAmount      = t.VatAmount,
                    TotalAmount    = t.TotalAmount,
                }).ToList()
            }).ToList()
        };
}
