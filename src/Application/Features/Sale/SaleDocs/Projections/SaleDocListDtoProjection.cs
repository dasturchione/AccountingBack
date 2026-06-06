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
            CurrencyName = x.Currency.Name,
            TotalAmount = x.TotalAmount,
            FinalAmount = x.FinalAmount,
            StatusId = x.StatusId,
            StatusName = x.Status.Name,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
