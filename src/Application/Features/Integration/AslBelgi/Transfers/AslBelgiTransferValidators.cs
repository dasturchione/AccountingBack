using Application.Features.Integration.AslBelgi.DTOs;
using FluentValidation;

namespace Application.Features.Integration.AslBelgi.Transfers;

public sealed class CrptTransferRequestSubmitDtoValidator : AbstractValidator<CrptTransferRequestSubmitDto>
{
    public CrptTransferRequestSubmitDtoValidator(IValidator<CrptSignedDocumentPayloadDto> payloadValidator)
    {
        RuleFor(x => x.SellerCounterpartyId).GreaterThan(0);
        RuleFor(x => x.BuyerCounterpartyId).GreaterThan(0);
        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SignedPayload).SetValidator(payloadValidator);
    }
}

public sealed class CrptTransferConfirmationSubmitDtoValidator : AbstractValidator<CrptTransferConfirmationSubmitDto>
{
    public CrptTransferConfirmationSubmitDtoValidator(IValidator<CrptSignedDocumentPayloadDto> payloadValidator)
    {
        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SignedPayload).SetValidator(payloadValidator);
    }
}
