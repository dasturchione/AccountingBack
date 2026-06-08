using FluentValidation;

namespace Application.Features.CashOperations;

public class CashOperationCreateDtoValidator : AbstractValidator<CashOperationCreateDto>
{
    public CashOperationCreateDtoValidator()
    {
        Include(new CashOperationBaseDtoValidator());
    }
}
