using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.SaleDocs;

public class SaleDocListDtoProjection : IProjectionBuilder<SaleDoc, SaleDocListDto>
{
    public Expression<Func<SaleDoc, SaleDocListDto>> Build() =>
        x => new SaleDocListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            DocNumber = x.DocNumber,
            DocDate = x.DocDate,
            CounterpartyId = x.CounterpartyId,
            CounterpartyName = x.Counterparty.ShortName,
            WarehouseId = x.WarehouseId,
            WarehouseName = x.Warehouse.Name,
            CurrencyId = x.CurrencyId,
            CurrencyCode = x.Currency.Code,
            CurrencyName = x.Currency.Name,
            TotalAmount = x.TotalAmount,
            FinalAmount = x.FinalAmount,
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
            StatusId = x.StatusId,
            StatusName = x.Status.Name,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate,
            ContractId = x.ContractId,
            ContractNumber = x.Contract == null ? null : x.Contract.ContractNumber
        };
}
