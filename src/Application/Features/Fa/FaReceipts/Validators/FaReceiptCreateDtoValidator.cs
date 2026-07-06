using FluentValidation;

namespace Application.Features.FaReceipts;

public class FaReceiptCreateDtoValidator : AbstractValidator<FaReceiptCreateDto>
{
    public FaReceiptCreateDtoValidator()
    {
        Include(new FaReceiptBaseDtoValidator());
    }
}
