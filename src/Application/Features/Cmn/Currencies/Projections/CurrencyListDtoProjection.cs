using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Cmn.Currencies;

public sealed class CurrencyListDtoProjection : IProjectionBuilder<Currency, CurrencyListDto>
{
    public Expression<Func<Currency, CurrencyListDto>> Build() =>
        x => new CurrencyListDto
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            Symbol = x.Symbol,
            StateId = x.StateId,
            StateName = x.State.FullName
        };
}
