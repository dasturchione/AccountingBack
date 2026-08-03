using FluentValidation;

namespace Application.Features.FaAssets;

public class FaAssetUpdateDtoValidator : AbstractValidator<FaAssetUpdateDto>
{
    public FaAssetUpdateDtoValidator()
    {
        Include(new FaAssetBaseDtoValidator());
        RuleFor(x => x.StateId).GreaterThan((short)0);
    }
}