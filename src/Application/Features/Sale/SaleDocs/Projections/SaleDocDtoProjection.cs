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
            CustomerAccountId = x.CustomerAccountId,
            CustomerAccountNumber = x.CustomerAccount == null ? null : x.CustomerAccount.Number,
            CustomerAccountName = x.CustomerAccount == null ? null : x.CustomerAccount.Name,
            VatAccountId = x.VatAccountId,
            VatAccountNumber = x.VatAccount == null ? null : x.VatAccount.Number,
            VatAccountName = x.VatAccount == null ? null : x.VatAccount.Name,
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
                IsService   = p.Product.IsService,
                Quantity    = p.Quantity,
                UnitId      = p.UnitId,
                UnitName    = p.Unit.Name,
                UnitPrice   = p.UnitPrice,
                CostPrice   = p.CostPrice,
                Amount      = p.Amount,
                VatRateId   = p.VatRateId,
                VatRateName = p.VatRate != null ? p.VatRate.Name : null,
                InventoryAccountId = p.InventoryAccountId,
                InventoryAccountNumber = p.InventoryAccount == null ? null : p.InventoryAccount.Number,
                InventoryAccountName = p.InventoryAccount == null ? null : p.InventoryAccount.Name,
                IncomeAccountId = p.IncomeAccountId,
                IncomeAccountNumber = p.IncomeAccount == null ? null : p.IncomeAccount.Number,
                IncomeAccountName = p.IncomeAccount == null ? null : p.IncomeAccount.Name,
                CostAccountId = p.CostAccountId,
                CostAccountNumber = p.CostAccount == null ? null : p.CostAccount.Number,
                CostAccountName = p.CostAccount == null ? null : p.CostAccount.Name,
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
                }).ToList(),
                Batches = p.SaleDocProductBatches.Select(batch => new SaleDocProductBatchReadDto
                {
                    BatchId = batch.WarehouseProductBatchId,
                    Quantity = batch.Quantity
                }).ToList()
            }).ToList()
        };
}
