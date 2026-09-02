using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Cmn.Currencies;

public sealed class CurrencyListDtoProjection(IUserContext userContext) : IProjectionBuilder<Currency, CurrencyListDto>
{
    public Expression<Func<Currency, CurrencyListDto>> Build()
    {
        var languageId = userContext.LanguageId;

        return x => new CurrencyListDto
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.CurrencyTranslations
                .Where(translation => translation.LanguageId == languageId)
                .Select(translation => translation.Name)
                .FirstOrDefault() ?? x.Name,
            Symbol = x.Symbol,
            StateId = x.StateId,
            StateName = x.State.FullName
        };
    }
}
