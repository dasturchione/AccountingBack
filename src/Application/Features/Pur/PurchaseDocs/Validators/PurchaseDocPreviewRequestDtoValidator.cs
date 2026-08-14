using FluentValidation;
using Application.Features.Pur.PurchaseDocs;

namespace Application.Features.PurchaseDocs;

public sealed class PurchaseDocPreviewRequestDtoValidator : AbstractValidator<PurchaseDocPreviewRequestDto>
{
    public PurchaseDocPreviewRequestDtoValidator()
    {
        RuleFor(x => x.DocumentIdentity)
            .NotEmpty()
            .MaximumLength(100);

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(x => x.LineNumber).GreaterThan(0);
            line.RuleFor(x => x.ProductId).GreaterThan(0).When(x => x.ProductId.HasValue);
            line.RuleFor(x => x.UnitId).GreaterThan((short)0).When(x => x.UnitId.HasValue);
            line.RuleFor(x => x.VatRateId).GreaterThan((short)0).When(x => x.VatRateId.HasValue);
            line.RuleFor(x => x.Items)
                .Must(items => items.Count == items
                    .Select(item => item.MarkingNumber)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count())
                .WithMessage("Duplicate marking numbers are not allowed.");
            line.RuleForEach(x => x.Items).SetValidator(new PurchaseDocLineItemDtoValidator());
        });

        RuleFor(x => x.Lines)
            .Must(lines =>
            {
                var markings = lines
                    .SelectMany(line => line.Items)
                    .Select(item => item.MarkingNumber)
                    .ToList();
                return markings.Count == markings.Distinct(StringComparer.OrdinalIgnoreCase).Count();
            })
            .WithMessage("Duplicate marking numbers are not allowed across lines.");
    }
}
