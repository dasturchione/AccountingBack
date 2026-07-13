using Domain.Entities;
using SharedKernel.Constants;
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
            ExchangeRate = x.ExchangeRate,
            SupplierAccountId = x.SupplierAccountId,
            SupplierAccountNumber = x.SupplierAccount == null ? null : x.SupplierAccount.Number,
            SupplierAccountName = x.SupplierAccount == null ? null : x.SupplierAccount.Name,
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
            Lines = x.PurchaseDocProducts
                .Select(l => new PurchaseDocProductDto
                {
                    Id                 = l.Id,
                    OwnerId            = l.OwnerId,
                    ProductId          = l.ProductId,
                    ProductName        = l.Product.Name,
                    ProductMxik        = l.Product.Mxik,
                    Quantity           = l.Quantity,
                    UnitId             = l.UnitId,
                    UnitName           = l.Unit.Name,
                    UnitPrice          = l.UnitPrice,
                    Amount             = l.Amount,
                    VatRateId          = l.VatRateId,
                    VatRateName        = l.VatRate != null ? l.VatRate.Name : null,
                    DebitAccountId     = l.DebitAccountId,
                    DebitAccountNumber = l.DebitAccount == null ? null : l.DebitAccount.Number,
                    DebitAccountName   = l.DebitAccount == null ? null : l.DebitAccount.Name,
                    VatAccountId       = l.VatAccountId,
                    VatAccountNumber   = l.VatAccount == null ? null : l.VatAccount.Number,
                    VatAccountName     = l.VatAccount == null ? null : l.VatAccount.Name,
                    VatAmount          = l.VatAmount,
                    TotalAmount        = l.TotalAmount,
                    Items = l.PurchaseDocTables.Select(t => new PurchaseDocProductItemDto
                    {
                        Id             = t.Id,
                        ProductTableId = t.ProductTableId,
                        MarkingNumber  = t.ProductTable.MarkingNumber,
                        SerialNumber   = t.ProductTable.SerialNumber,
                        Amount         = t.Amount,
                        VatRateId      = t.VatRateId,
                        VatRateName    = t.VatRate != null ? t.VatRate.Name : null,
                        VatAmount      = t.VatAmount,
                        TotalAmount    = t.TotalAmount,
                    }).ToList()
                }).ToList()
        };
}
