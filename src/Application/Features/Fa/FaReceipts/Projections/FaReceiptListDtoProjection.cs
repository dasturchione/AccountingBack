using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaReceipts;

public class FaReceiptListDtoProjection : IProjectionBuilder<FaReceiptDoc, FaReceiptListDto>
{
    public Expression<Func<FaReceiptDoc, FaReceiptListDto>> Build() =>
        x => new FaReceiptListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            DocNumber = x.DocNumber,
            DocDate = x.DocDate,
            CounterpartyId = x.CounterpartyId,
            CounterpartyName = x.Counterparty != null ? x.Counterparty.ShortName : null,
            WarehouseId = x.WarehouseId,
            WarehouseName = x.Warehouse != null ? x.Warehouse.Name : null,
            CurrencyId = x.CurrencyId,
            CurrencyName = x.Currency.Name,
            TotalAmount = x.TotalAmount,
            VatAmount = x.VatAmount,
            FinalAmount = x.FinalAmount,
            StatusId = x.StatusId,
            StatusName = x.Status.Name,
            ReceiptTypeId = x.ReceiptTypeId,
            SupplierAccountId = x.SupplierAccountId,
            StateId = x.StateId,
            StateName = x.State.FullName,
            UpdatedDate = x.UpdatedDate
        };
}
