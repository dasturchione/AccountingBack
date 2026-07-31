using FluentValidation;

namespace Application.Features.Integration.Didox.Facturas;

public sealed class DidoxFacturaPartyDtoValidator : AbstractValidator<DidoxFacturaPartyDto>
{
    public DidoxFacturaPartyDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.VatRegCode).NotEmpty();
        RuleFor(x => x.VatRegStatus).NotEmpty();
        RuleFor(x => x.Account).NotEmpty();
        RuleFor(x => x.BankId).NotEmpty();
        RuleFor(x => x.Address).NotEmpty();
    }
}

public sealed class DidoxFacturaEmpowermentDtoValidator : AbstractValidator<DidoxFacturaEmpowermentDto>
{
    public DidoxFacturaEmpowermentDtoValidator()
    {
        RuleFor(x => x.EmpowermentNo).NotEmpty();
        RuleFor(x => x.AgentFio).NotEmpty();
        RuleFor(x => x.AgentPinfl).NotEmpty();
    }
}

public sealed class DidoxFacturaProductRequestDtoValidator : AbstractValidator<DidoxFacturaProductRequestDto>
{
    public DidoxFacturaProductRequestDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.CatalogCode).NotEmpty();
        RuleFor(x => x.CatalogName).NotEmpty();
        RuleFor(x => x.PackageCode).NotEmpty();
        RuleFor(x => x.PackageName).NotEmpty();
        RuleFor(x => x.Count).NotEmpty();
        RuleFor(x => x.Summa).NotEmpty();
        RuleFor(x => x.VatRate).NotEmpty();
        RuleFor(x => x.VatSum).NotEmpty();
        RuleForEach(x => x.MarkingCodeIds).GreaterThan(0);
    }
}

public sealed class DidoxFacturaCreateRequestDtoValidator : AbstractValidator<DidoxFacturaCreateRequestDto>
{
    public DidoxFacturaCreateRequestDtoValidator()
    {
        RuleFor(x => x.InternalDocumentId).GreaterThan(0);
        RuleFor(x => x.InternalDocumentType).NotEmpty();
        RuleFor(x => x.SellerTin).NotEmpty();
        RuleFor(x => x.BuyerTin).NotEmpty();
        RuleFor(x => x.Seller).NotNull().SetValidator(new DidoxFacturaPartyDtoValidator());
        RuleFor(x => x.Buyer).NotNull().SetValidator(new DidoxFacturaPartyDtoValidator());
        RuleFor(x => x.FacturaNo).NotEmpty();
        RuleFor(x => x.ContractNo).NotEmpty();
        RuleFor(x => x.Empowerment).NotNull().SetValidator(new DidoxFacturaEmpowermentDtoValidator());
        RuleFor(x => x.Products).NotEmpty();
        RuleForEach(x => x.Products).SetValidator(new DidoxFacturaProductRequestDtoValidator());
        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(200);
    }
}

public sealed class DidoxFacturaSignRequestDtoValidator : AbstractValidator<DidoxFacturaSignRequestDto>
{
    public DidoxFacturaSignRequestDtoValidator()
    {
        RuleFor(x => x.MarkingDidoxDocumentId).GreaterThan(0);
        RuleFor(x => x.Pkcs7).NotEmpty();
        RuleFor(x => x.SignatureHex).NotEmpty();
    }
}
