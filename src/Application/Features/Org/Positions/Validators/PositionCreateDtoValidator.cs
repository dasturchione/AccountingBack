using FluentValidation;

namespace Application.Features.Positions;

public class PositionCreateDtoValidator : AbstractValidator<PositionCreateDto>
{
    public PositionCreateDtoValidator()
    {
        Include(new PositionBaseDtoValidator());
    }
}
