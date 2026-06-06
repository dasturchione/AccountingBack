using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.SaleDocs;

public class SaleDocDtoProjection : IProjectionBuilder<SaleDoc, SaleDocDto>
{
    public Expression<Func<SaleDoc, SaleDocDto>> Build() =>
        x => new SaleDocDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            DocNumber = x.DocNumber,
            DocDate = x.DocDate,
            CounterpartyId = x.CounterpartyId,
            CounterpartyName = x.Counterparty.ShortName,
            WarehouseId = x.WarehouseId,
            WarehouseName = x.Warehouse.Name,
            CurrencyId = x.CurrencyId,
            CurrencyName = x.Currency.Name,
            TotalAmount = x.TotalAmount,
            VatAmount = x.VatAmount,
            FinalAmount = x.FinalAmount,
            StatusId = x.StatusId,
            StatusName = x.Status.Name,
            Comment = x.Comment,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
