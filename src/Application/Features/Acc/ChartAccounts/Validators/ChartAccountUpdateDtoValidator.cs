using FluentValidation;

namespace Application.Features.ChartAccounts;

public class ChartAccountUpdateDtoValidator : AbstractValidator<ChartAccountUpdateDto>
{
    public ChartAccountUpdateDtoValidator()
    {
        Include(new ChartAccountBaseDtoValidator());
        RuleFor(x => x.StateId).GreaterThan((short)0);
    }
}
