using FluentValidation;

namespace Application.Features.CashOperations;

public class CashOperationUpdateDtoValidator : AbstractValidator<CashOperationUpdateDto>
{
    public CashOperationUpdateDtoValidator()
    {
        Include(new CashOperationBaseDtoValidator());
        RuleFor(x => x.StateId).GreaterThan((short)0);
    }
}
