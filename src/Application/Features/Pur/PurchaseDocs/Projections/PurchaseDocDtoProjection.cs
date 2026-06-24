using Application.Features.PurchaseDocTables;
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
            StatusId         = x.StatusId,
            StatusName       = x.Status.Name,
            Comment          = x.Comment,
            StateId          = x.StateId,
            StateName        = x.State.FullName,
            CreatedDate      = x.CreatedDate,
            ContractId       = x.ContractId,
            ContractNumber   = x.Contract == null ? null : x.Contract.ContractNumber,
            Lines = x.PurchaseDocTables
                .Where(l => l.ItemTypeId == PurchaseItemTypeIdConst.PRODUCT)
                .Select(l => new PurchaseDocTableDto
                {
                    Id                 = l.Id,
                    OwnerId            = l.OwnerId,
                    ItemTypeId         = l.ItemTypeId,
                    ProductTableId     = l.ProductTableId,
                    ProductName        = l.ProductTable != null ? l.ProductTable.Product.Name : null,
                    Quantity           = l.Quantity,
                    Price              = l.Price,
                    Amount             = l.Amount,
                    VatRateId          = l.VatRateId,
                    VatRateName        = l.VatRate != null ? l.VatRate.Name : null,
                    VatAmount          = l.VatAmount,
                    TotalAmount        = l.TotalAmount,
                    SerialNumber       = l.ProductTable != null ? l.ProductTable.SerialNumber : null,
                    MarkingNumber      = l.ProductTable != null ? l.ProductTable.MarkingNumber : null,
                }).ToList(),
            ServiceLines = x.PurchaseDocTables
                .Where(l => l.ItemTypeId == PurchaseItemTypeIdConst.SERVICE)
                .Select(l => new PurchaseDocTableDto
                {
                    Id                 = l.Id,
                    OwnerId            = l.OwnerId,
                    ItemTypeId         = l.ItemTypeId,
                    ServiceId          = l.ServiceId,
                    ServiceName        = l.Service != null ? l.Service.Name : null,
                    ExpenseAccountId   = l.Service != null ? l.Service.ServiceType.AccountId : null,
                    ExpenseAccountName = l.Service != null ? l.Service.ServiceType.Account.Name : null,
                    Quantity           = l.Quantity,
                    Price              = l.Price,
                    Amount             = l.Amount,
                    VatRateId          = l.VatRateId,
                    VatRateName        = l.VatRate != null ? l.VatRate.Name : null,
                    VatAmount          = l.VatAmount,
                    TotalAmount        = l.TotalAmount,
                }).ToList()
        };
}
