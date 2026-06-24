using FluentValidation;

namespace Application.Features.BankOperations;

public class BankOperationsCreateDtoValidator : AbstractValidator<BankOperationsCreateDto>
{
    public BankOperationsCreateDtoValidator()
    {
        RuleFor(x => x.Operations).NotEmpty();
        RuleForEach(x => x.Operations).SetValidator(new BankOperationCreateDtoValidator());
    }
}
