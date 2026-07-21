using FluentValidation;
using SharedKernel.Constants;

namespace Application.Features.Acc.OpeningBalances;

public class OpeningBalanceUpdateDtoValidator : AbstractValidator<OpeningBalanceUpdateDto>
{
    public OpeningBalanceUpdateDtoValidator()
    {
        Include(new OpeningBalanceCreateDtoValidator());
        RuleFor(x => x.StateId)
            .Must(x => x is StateIdConst.ACTIVE or StateIdConst.PASSIVE)
            .WithMessage("StateId must be ACTIVE or PASSIVE.");
    }
}
