using FluentValidation;

namespace Application.Features.FaMovements;

public class FaMovementUpdateDtoValidator : AbstractValidator<FaMovementUpdateDto>
{
    public FaMovementUpdateDtoValidator()
    {
        Include(new FaMovementBaseDtoValidator());
        RuleFor(x => x.StateId).GreaterThan((short)0);
    }
}
