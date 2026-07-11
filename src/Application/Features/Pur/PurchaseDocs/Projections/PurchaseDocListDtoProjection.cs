using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.PurchaseDocs;

public class PurchaseDocListDtoProjection : IProjectionBuilder<PurchaseDoc, PurchaseDocListDto>
{
    public Expression<Func<PurchaseDoc, PurchaseDocListDto>> Build() =>
        x => new PurchaseDocListDto
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
            CurrencyName = x.Currency.Name,
            TotalAmount = x.TotalAmount,
            FinalAmount = x.FinalAmount,
            ExchangeRate = x.ExchangeRate,
            SupplierAccountId = x.SupplierAccountId,
            SupplierAccountNumber = x.SupplierAccount == null ? null : x.SupplierAccount.Number,
            SupplierAccountName = x.SupplierAccount == null ? null : x.SupplierAccount.Name,
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
