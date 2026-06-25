using FluentValidation;

namespace Application.Features.BankOperations;

public class BankOperationUpdateDtoValidator : AbstractValidator<BankOperationUpdateDto>
{
    public BankOperationUpdateDtoValidator()
    {
        Include(new BankOperationBaseDtoValidator());
    }
}
