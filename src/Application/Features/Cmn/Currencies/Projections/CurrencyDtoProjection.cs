using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Cmn.Currencies;

public sealed class CurrencyDtoProjection : IProjectionBuilder<Currency, CurrencyDto>
{
    public Expression<Func<Currency, CurrencyDto>> Build() =>
        x => new CurrencyDto
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            Symbol = x.Symbol,
            StateId = x.StateId,
            StateName = x.State.FullName
        };
}
