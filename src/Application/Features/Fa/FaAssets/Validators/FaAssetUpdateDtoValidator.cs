using FluentValidation;

namespace Application.Features.FaAssets;

public class FaAssetUpdateDtoValidator : AbstractValidator<FaAssetUpdateDto>
{
    public FaAssetUpdateDtoValidator()
    {
        RuleFor(x => x.InventoryNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(500);
        RuleFor(x => x.FaGroupId).GreaterThan(0);
    }
}
