using FluentValidation;

namespace Application.Features.Cmn.Taxes;

public sealed class TaxUpdateDtoValidator : AbstractValidator<TaxUpdateDto>
{
    public TaxUpdateDtoValidator()
    {
        Include(new TaxBaseDtoValidator());
        RuleFor(x => x.StateId).GreaterThan((short)0);
    }
}
