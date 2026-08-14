using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.RetailSaleDocs;

public class RetailSaleDocDtoProjection : IProjectionBuilder<RetailSaleDoc, RetailSaleDocDto>
{
    public Expression<Func<RetailSaleDoc, RetailSaleDocDto>> Build() =>
        x => new RetailSaleDocDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            DocNumber = x.DocNumber,
            DocDate = x.DocDate,
            CounterpartyId = x.CounterpartyId,
            CounterpartyName = x.Counterparty == null ? null : x.Counterparty.ShortName,
            WarehouseId = x.WarehouseId,
            WarehouseName = x.Warehouse.Name,
            CashRegisterId = x.CashRegisterId,
            CashRegisterName = x.CashRegister.Name,
            CurrencyId = x.CurrencyId,
            CurrencyName = x.Currency.Name,
            CurrencyCode = x.Currency.Code,
            ExchangeRate = x.ExchangeRate,
            TotalAmount = x.TotalAmount,
            VatAmount = x.VatAmount,
            FinalAmount = x.FinalAmount,
            ReceivableAccountId = x.ReceivableAccountId,
            ReceivableAccountNumber = x.ReceivableAccount == null ? null : x.ReceivableAccount.Number,
            ReceivableAccountName = x.ReceivableAccount == null ? null : x.ReceivableAccount.Name,
            VatAccountId = x.VatAccountId,
            VatAccountNumber = x.VatAccount == null ? null : x.VatAccount.Number,
            VatAccountName = x.VatAccount == null ? null : x.VatAccount.Name,
            StatusId = x.StatusId,
            StatusName = x.Status.Name,
            StateId = x.StateId,
            StateName = x.State.FullName,
            Comment = x.Comment,
            CreatedDate = x.CreatedDate,
            PostedAt = x.PostedAt,
            PostedByUserId = x.PostedByUserId,
            CancelledAt = x.CancelledAt,
            CancelledByUserId = x.CancelledByUserId,
            Lines = x.RetailSaleDocProducts.Select(line => new RetailSaleDocProductDto
            {
                Id = line.Id,
                ProductId = line.ProductId,
                ProductName = line.Product.Name,
                ProductMxik = line.Product.Mxik,
                IsService = line.Product.IsService,
                IsPieceTracked = line.Product.IsPieceTracked,
                Quantity = line.Quantity,
                UnitId = line.UnitId,
                UnitName = line.Unit.Name,
                UnitPrice = line.UnitPrice,
                CostPrice = line.CostPrice,
                Amount = line.Amount,
                VatRateId = line.VatRateId,
                VatRateName = line.VatRate == null ? null : line.VatRate.Name,
                VatAmount = line.VatAmount,
                TotalAmount = line.TotalAmount,
                InventoryAccountId = line.InventoryAccountId,
                IncomeAccountId = line.IncomeAccountId,
                CostAccountId = line.CostAccountId,
                Items = line.RetailSaleDocTables.Select(item => new RetailSaleDocProductTableDto
                {
                    Id = item.Id,
                    ProductTableId = item.ProductTableId,
                    MarkingNumber = item.ProductTable.MarkingNumber,
                    SerialNumber = item.ProductTable.SerialNumber,
                    CostPrice = item.CostPrice,
                    Amount = item.Amount,
                    VatRateId = item.VatRateId,
                    VatAmount = item.VatAmount,
                    TotalAmount = item.TotalAmount
                }).ToList()
            }).ToList(),
            Payments = x.RetailSaleDocPayments.Select(payment => new RetailSaleDocPaymentReadDto
            {
                Id = payment.Id,
                PaymentMethodId = payment.PaymentMethodId,
                PaymentMethodName = payment.PaymentMethod.Name,
                BankTerminalId = payment.BankTerminalId,
                BankTerminalName = payment.BankTerminal == null ? null : payment.BankTerminal.Name,
                DebitAccountId = payment.DebitAccountId,
                DebitAccountNumber = payment.DebitAccount.Number,
                DebitAccountName = payment.DebitAccount.Name,
                Amount = payment.Amount,
                TransactionNumber = payment.TransactionNumber
            }).ToList()
        };
}

public class RetailSaleDocListDtoProjection : IProjectionBuilder<RetailSaleDoc, RetailSaleDocListDto>
{
    public Expression<Func<RetailSaleDoc, RetailSaleDocListDto>> Build() =>
        x => new RetailSaleDocListDto
        {
            Id = x.Id,
            DocNumber = x.DocNumber,
            DocDate = x.DocDate,
            CounterpartyId = x.CounterpartyId,
            CounterpartyName = x.Counterparty == null ? null : x.Counterparty.ShortName,
            WarehouseId = x.WarehouseId,
            WarehouseName = x.Warehouse.Name,
            CashRegisterId = x.CashRegisterId,
            CashRegisterName = x.CashRegister.Name,
            CurrencyId = x.CurrencyId,
            CurrencyCode = x.Currency.Code,
            FinalAmount = x.FinalAmount,
            StatusId = x.StatusId,
            StatusName = x.Status.Name,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
