using FluentValidation;
using Application.Features.Pur.PurchaseDocs;

namespace Application.Features.PurchaseDocs;

public sealed class PurchaseDocFromEdoRequestDtoValidator : AbstractValidator<PurchaseDocFromEdoRequestDto>
{
    public PurchaseDocFromEdoRequestDtoValidator()
    {
        RuleFor(x => x.DocumentIdentity)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.CounterpartyId).GreaterThan(0);
        RuleFor(x => x.ContractId).NotNull().GreaterThan(0);
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.CurrencyId).GreaterThan((short)0);
        RuleFor(x => x.Comment).MaximumLength(1000).When(x => x.Comment is not null);
        RuleFor(x => x.Lines).NotEmpty();

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(x => x.LineNumber).GreaterThan(0);
            line.RuleFor(x => x.ProductId).GreaterThan(0);
            line.RuleFor(x => x.UnitId).GreaterThan((short)0);
            line.RuleFor(x => x.VatRateId).GreaterThan((short)0).When(x => x.VatRateId.HasValue);
            line.RuleFor(x => x.Items).Must(items => items.Count == items.Select(item => item.MarkingNumber).Distinct(StringComparer.OrdinalIgnoreCase).Count())
                .WithMessage("Duplicate marking numbers are not allowed.");
            line.RuleForEach(x => x.Items).SetValidator(new PurchaseDocLineItemDtoValidator());
        });

        RuleFor(x => x.Lines)
            .Must(lines => lines.Select(line => line.LineNumber).Distinct().Count() == lines.Count)
            .WithMessage("LineNumber values must be unique.");

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
