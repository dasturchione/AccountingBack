using FluentValidation;

namespace Application.Features.Cmn.Currencies;

public sealed class CurrencyCreateDtoValidator : AbstractValidator<CurrencyCreateDto>
{
    public CurrencyCreateDtoValidator()
    {
        Include(new CurrencyBaseDtoValidator());
    }
}
