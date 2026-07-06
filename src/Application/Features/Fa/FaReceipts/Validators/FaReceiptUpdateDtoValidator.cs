using FluentValidation;

namespace Application.Features.FaReceipts;

public class FaReceiptUpdateDtoValidator : AbstractValidator<FaReceiptUpdateDto>
{
    public FaReceiptUpdateDtoValidator()
    {
        Include(new FaReceiptBaseDtoValidator());
    }
}
