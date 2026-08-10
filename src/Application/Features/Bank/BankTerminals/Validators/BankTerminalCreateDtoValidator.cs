using FluentValidation;

namespace Application.Features.BankTerminals;

public class BankTerminalCreateDtoValidator : AbstractValidator<BankTerminalCreateDto>
{
    public BankTerminalCreateDtoValidator()
    {
        Include(new BankTerminalBaseDtoValidator());
    }
}
