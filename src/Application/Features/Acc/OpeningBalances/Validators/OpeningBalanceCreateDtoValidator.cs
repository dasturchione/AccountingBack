using FluentValidation;

namespace Application.Features.Acc.OpeningBalances;

public class OpeningBalanceCreateDtoValidator : AbstractValidator<OpeningBalanceCreateDto>
{
    public OpeningBalanceCreateDtoValidator()
    {
        RuleFor(x => x.BalanceDate).NotEmpty();
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
