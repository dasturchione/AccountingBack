using FluentValidation;

namespace Application.Features.Banks;

public class BankCreateDtoValidator : AbstractValidator<BankCreateDto>
{
    public BankCreateDtoValidator()
    {
        Include(new BankBaseDtoValidator());
    }
}
