using FluentValidation;

namespace Application.Features.Positions;

public class PositionUpdateDtoValidator : AbstractValidator<PositionUpdateDto>
{
    public PositionUpdateDtoValidator()
    {
        Include(new PositionBaseDtoValidator());
        RuleFor(x => x.StateId).GreaterThan((short)0);
    }
}
