using FluentValidation;

namespace Application.Features.Cmn.Taxes;

public sealed class TaxCreateDtoValidator : AbstractValidator<TaxCreateDto>
{
    public TaxCreateDtoValidator()
    {
        Include(new TaxBaseDtoValidator());
    }
}
