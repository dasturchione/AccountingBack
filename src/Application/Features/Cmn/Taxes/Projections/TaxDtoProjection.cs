using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Cmn.Taxes;

public sealed class TaxDtoProjection : IProjectionBuilder<VatRate, TaxDto>
{
    public Expression<Func<VatRate, TaxDto>> Build() =>
        x => new TaxDto
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
