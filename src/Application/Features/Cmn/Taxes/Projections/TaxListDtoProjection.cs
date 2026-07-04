using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Cmn.Taxes;

public sealed class TaxListDtoProjection : IProjectionBuilder<VatRate, TaxListDto>
{
    public Expression<Func<VatRate, TaxListDto>> Build() =>
        x => new TaxListDto
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            Rate = x.Rate,
            EffectiveFrom = x.EffectiveFrom,
            EffectiveTo = x.EffectiveTo,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
