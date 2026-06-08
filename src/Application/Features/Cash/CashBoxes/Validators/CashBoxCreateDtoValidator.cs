using FluentValidation;

namespace Application.Features.CashBoxes;

public class CashBoxCreateDtoValidator : AbstractValidator<CashBoxCreateDto>
{
    public CashBoxCreateDtoValidator()
    {
        Include(new CashBoxBaseDtoValidator());
    }
}
