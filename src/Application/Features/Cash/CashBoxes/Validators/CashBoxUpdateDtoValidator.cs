using FluentValidation;

namespace Application.Features.CashBoxes;

public class CashBoxUpdateDtoValidator : AbstractValidator<CashBoxUpdateDto>
{
    public CashBoxUpdateDtoValidator()
    {
        Include(new CashBoxBaseDtoValidator());
        RuleFor(x => x.StateId).GreaterThan((short)0);
    }
}
