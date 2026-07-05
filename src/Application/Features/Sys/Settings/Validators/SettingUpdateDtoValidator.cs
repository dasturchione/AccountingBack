using FluentValidation;

namespace Application.Features.Settings;

public sealed class SettingUpdateDtoValidator : AbstractValidator<SettingUpdateDto>
{
    public SettingUpdateDtoValidator()
    {
        RuleFor(x => x.Value).NotNull();
    }
}
