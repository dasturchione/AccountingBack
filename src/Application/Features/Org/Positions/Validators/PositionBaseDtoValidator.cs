using FluentValidation;

namespace Application.Features.Positions;

public class PositionBaseDtoValidator : AbstractValidator<PositionBaseDto>
{
    public PositionBaseDtoValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(250);
    }
}
