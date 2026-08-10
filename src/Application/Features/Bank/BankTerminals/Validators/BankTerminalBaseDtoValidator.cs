using FluentValidation;

namespace Application.Features.BankTerminals;

public class BankTerminalBaseDtoValidator : AbstractValidator<BankTerminalBaseDto>
{
    public BankTerminalBaseDtoValidator()
    {
        RuleFor(x => x.BankAccountId)
            .GreaterThan(0)
            .When(x => x.BankAccountId.HasValue);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MerchantId).MaximumLength(100);
        RuleFor(x => x.ExternalTerminalId).MaximumLength(100);
        RuleFor(x => x.SerialNumber).MaximumLength(100);
    }
}
