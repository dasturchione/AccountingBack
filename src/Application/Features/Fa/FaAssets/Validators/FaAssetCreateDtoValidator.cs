using FluentValidation;

namespace Application.Features.FaAssets;

public class FaAssetCreateDtoValidator : AbstractValidator<FaAssetCreateDto>
{
    public FaAssetCreateDtoValidator()
    {
        Include(new FaAssetBaseDtoValidator());
    }
}
