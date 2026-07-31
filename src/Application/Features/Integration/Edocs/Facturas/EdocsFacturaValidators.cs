using FluentValidation;

namespace Application.Features.Integration.Edocs.Facturas;

public sealed class EdocsFacturaPartyDtoValidator : AbstractValidator<EdocsFacturaPartyDto>
{
    public EdocsFacturaPartyDtoValidator()
    {
        RuleFor(x => x.BankId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Account).NotEmpty();
        RuleFor(x => x.Address).NotEmpty();
        RuleFor(x => x.DistrictId).NotEmpty();
    }
}

public sealed class EdocsFacturaProductRequestDtoValidator : AbstractValidator<EdocsFacturaProductRequestDto>
{
    public EdocsFacturaProductRequestDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Count).NotEmpty();
        RuleFor(x => x.Summa).NotEmpty();
        RuleForEach(x => x.MarkingCodeIds).GreaterThan(0);
    }
}

public sealed class EdocsFacturaSignRequestDtoValidator : AbstractValidator<EdocsFacturaSignRequestDto>
{
    public EdocsFacturaSignRequestDtoValidator()
    {
        RuleFor(x => x.MarkingEdocsDocumentId).GreaterThan(0);
        RuleFor(x => x.Pkcs7).NotEmpty();
    }
}

public sealed class EdocsFacturaSignBodyDtoValidator : AbstractValidator<EdocsFacturaSignBodyDto>
{
    public EdocsFacturaSignBodyDtoValidator()
    {
        RuleFor(x => x.Pkcs7).NotEmpty();
    }
}

public sealed class EdocsFacturaCreateRequestDtoValidator : AbstractValidator<EdocsFacturaCreateRequestDto>
{
    public EdocsFacturaCreateRequestDtoValidator()
    {
        RuleFor(x => x.InternalDocumentId).GreaterThan(0);
        RuleFor(x => x.InternalDocumentType).NotEmpty();
        RuleFor(x => x.SellerTin).NotEmpty();
        RuleFor(x => x.BuyerTin).NotEmpty();
        RuleFor(x => x.Seller).NotNull().SetValidator(new EdocsFacturaPartyDtoValidator());
        RuleFor(x => x.Buyer).NotNull().SetValidator(new EdocsFacturaPartyDtoValidator());
        RuleFor(x => x.FacturaNo).NotEmpty();
        RuleFor(x => x.Products).NotEmpty();
        RuleForEach(x => x.Products).SetValidator(new EdocsFacturaProductRequestDtoValidator());
        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(200);
    }
}
