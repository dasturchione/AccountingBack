using FluentValidation;

namespace Application.Features.ChartAccounts;

public class ChartAccountCreateDtoValidator : AbstractValidator<ChartAccountCreateDto>
{
    public ChartAccountCreateDtoValidator()
    {
        Include(new ChartAccountBaseDtoValidator());
    }
}
