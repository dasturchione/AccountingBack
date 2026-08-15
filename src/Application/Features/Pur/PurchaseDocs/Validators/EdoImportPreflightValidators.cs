using FluentValidation;

namespace Application.Features.PurchaseDocs;

public sealed class EdoImportPreflightRequestDtoValidator : AbstractValidator<EdoImportPreflightRequestDto>
{
    public EdoImportPreflightRequestDtoValidator()
    {
        RuleFor(x => x.DateTo).NotEmpty();
        RuleFor(x => x).Must(x => !x.DateFrom.HasValue || x.DateFrom.Value <= x.DateTo)
            .WithMessage("DateFrom must not be later than DateTo.");
    }
}

public sealed class EdoImportCandidateListFilterValidator : AbstractValidator<EdoImportCandidateListFilter>
{
    public EdoImportCandidateListFilterValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class EdoImportCandidateMappingRequestDtoValidator
    : AbstractValidator<EdoImportCandidateMappingRequestDto>
{
    public EdoImportCandidateMappingRequestDtoValidator()
    {
        RuleFor(x => x.CounterpartyId).GreaterThan(0).When(x => x.CounterpartyId.HasValue);
        RuleFor(x => x.ContractId).GreaterThan(0).When(x => x.ContractId.HasValue);
        RuleFor(x => x.CurrencyId).GreaterThan((short)0).When(x => x.CurrencyId.HasValue);
        RuleFor(x => x.WarehouseId).GreaterThan(0).When(x => x.WarehouseId.HasValue);
        RuleFor(x => x.Lines).NotNull();
        RuleForEach(x => x.Lines).SetValidator(new EdoImportCandidateLineMappingRequestDtoValidator());
        RuleFor(x => x.Lines)
            .Must(lines => lines.Select(line => line.LineNumber).Distinct().Count() == lines.Count)
            .WithMessage("Line numbers must be unique.");
    }
}

public sealed class EdoImportCandidateLineMappingRequestDtoValidator
    : AbstractValidator<EdoImportCandidateLineMappingRequestDto>
{
    public EdoImportCandidateLineMappingRequestDtoValidator()
    {
        RuleFor(x => x.LineNumber).GreaterThan(0);
        RuleFor(x => x.ProductId).GreaterThan(0).When(x => x.ProductId.HasValue);
        RuleFor(x => x.UnitId).GreaterThan((short)0).When(x => x.UnitId.HasValue);
        RuleFor(x => x.VatRateId).GreaterThan((short)0).When(x => x.VatRateId.HasValue);
        RuleFor(x => x.DebitAccountId).GreaterThan(0).When(x => x.DebitAccountId.HasValue);
        RuleFor(x => x.VatAccountId).GreaterThan(0).When(x => x.VatAccountId.HasValue);
    }
}

public sealed class EdoImportMasterDataApplyRequestDtoValidator
    : AbstractValidator<EdoImportMasterDataApplyRequestDto>
{
    public EdoImportMasterDataApplyRequestDtoValidator()
    {
        RuleFor(x => x.Confirm).Equal(true);
        RuleFor(x => x.ExpectedPlanHash).NotEmpty().Length(64);
        RuleForEach(x => x.Counterparties).ChildRules(item =>
        {
            item.RuleFor(x => x.SellerTin).NotEmpty().MaximumLength(20);
            item.RuleFor(x => x.Action).Must(IsApplyAction);
        });
        RuleForEach(x => x.Contracts).ChildRules(item =>
        {
            item.RuleFor(x => x.SellerTin).NotEmpty().MaximumLength(20);
            item.RuleFor(x => x.ProviderContractNumber).NotEmpty().MaximumLength(100);
            item.RuleFor(x => x.ProviderContractDate).NotEmpty();
            item.RuleFor(x => x.Action).Must(IsApplyAction);
        });
        RuleForEach(x => x.Products).ChildRules(item =>
        {
            item.RuleFor(x => x.CatalogCode).NotEmpty().Length(17);
            item.RuleFor(x => x.Action).Must(IsApplyAction);
            item.RuleFor(x => x.UnitId).GreaterThan((short)0).When(x => x.UnitId.HasValue);
            item.RuleFor(x => x.VatRateId).GreaterThan((short)0).When(x => x.VatRateId.HasValue);
        });
    }

    private static bool IsApplyAction(string action) => action is "CREATE" or "USE_EXISTING";
}

public sealed class EdoImportProductDefaultsApplyRequestDtoValidator
    : AbstractValidator<EdoImportProductDefaultsApplyRequestDto>
{
    public EdoImportProductDefaultsApplyRequestDtoValidator()
    {
        RuleFor(x => x.Confirm).Equal(true);
        RuleFor(x => x.ExpectedPlanHash).NotEmpty().Length(64);
        RuleFor(x => x.MarkingPolicy).Equal("PIECE_TRACKED_WHEN_REQUIRED");
        RuleFor(x => x.PackageUnitMappings).NotEmpty();
        RuleForEach(x => x.PackageUnitMappings).ChildRules(item =>
        {
            item.RuleFor(x => x.PackageName).NotEmpty().MaximumLength(250);
            item.RuleFor(x => x.UnitId).GreaterThan((short)0);
        });
        RuleFor(x => x.PackageUnitMappings)
            .Must(items => items.Select(item => item.PackageName)
                .Distinct(StringComparer.Ordinal).Count() == items.Count)
            .WithMessage("Package names must be exact and unique.");
    }
}

public sealed class EdoImportProductConflictApplyRequestDtoValidator
    : AbstractValidator<EdoImportProductConflictApplyRequestDto>
{
    public EdoImportProductConflictApplyRequestDtoValidator()
    {
        RuleFor(x => x.Confirm).Equal(true);
        RuleFor(x => x.ExpectedPlanHash).NotEmpty().Length(64);
        RuleFor(x => x.Items).NotEmpty();
        RuleFor(x => x.Items).Must(items =>
                items.SelectMany(item => item.IdentityKeys).Distinct(StringComparer.Ordinal).Count()
                == items.Sum(item => item.IdentityKeys.Count))
            .WithMessage("Provider product identities must be unique.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.IdentityKeys).NotEmpty();
            item.RuleForEach(x => x.IdentityKeys).NotEmpty().Length(64);
            item.RuleFor(x => x.Action).Must(action => action is "CREATE" or "USE_EXISTING");
            item.RuleFor(x => x.ExistingProductId).GreaterThan(0)
                .When(x => x.Action == "USE_EXISTING");
            item.RuleFor(x => x.IsService).NotNull();
            item.RuleFor(x => x.UnitId).NotNull().GreaterThan((short)0);
            item.RuleFor(x => x.VatRateId).NotNull().GreaterThan((short)0);
            item.RuleFor(x => x.IsPieceTracked).NotNull();
        });
    }
}

public sealed class EdoImportMarkingConflictApplyRequestDtoValidator
    : AbstractValidator<EdoImportMarkingConflictApplyRequestDto>
{
    public EdoImportMarkingConflictApplyRequestDtoValidator()
    {
        RuleFor(x => x.Confirm).Equal(true);
        RuleFor(x => x.ExpectedConflictHash).NotEmpty().Length(64);
        RuleFor(x => x.Items).NotEmpty();
        RuleFor(x => x.Items).Must(items => items.Select(item => item.CandidateId)
            .Distinct().Count() == items.Count)
            .WithMessage("Candidate IDs must be unique.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.CandidateId).GreaterThan(0);
            item.RuleFor(x => x.Action).Equal("SKIP");
        });
    }
}

public sealed class EdoImportDraftBatchRequestDtoValidator
    : AbstractValidator<EdoImportDraftBatchRequestDto>
{
    public EdoImportDraftBatchRequestDtoValidator()
    {
        RuleFor(x => x.Confirm).Equal(true);
        RuleFor(x => x.ExpectedImportPlanHash).NotEmpty().Length(64);
        RuleFor(x => x.BatchSize).InclusiveBetween(1, 50);
    }
}

public sealed class EdoImportDraftRequeueRequestDtoValidator
    : AbstractValidator<EdoImportDraftRequeueRequestDto>
{
    public EdoImportDraftRequeueRequestDtoValidator()
    {
        RuleFor(x => x.Confirm).Equal(true);
    }
}

public sealed class EdoImportDraftFailureApplyRequestDtoValidator
    : AbstractValidator<EdoImportDraftFailureApplyRequestDto>
{
    public EdoImportDraftFailureApplyRequestDtoValidator()
    {
        RuleFor(x => x.Confirm).Equal(true);
        RuleFor(x => x.ExpectedFailureHash).NotEmpty().Length(64);
        RuleFor(x => x.Items).NotEmpty()
            .Must(items => items.Select(item => item.CandidateId).Distinct().Count() == items.Count)
            .WithMessage("Candidate IDs must be unique.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.CandidateId).GreaterThan(0);
            item.RuleFor(x => x.Action)
                .Must(action => action is "SKIP" or "MARK_DUPLICATE");
        });
    }
}

public sealed class EdoImportBulkDraftStartRequestDtoValidator
    : AbstractValidator<EdoImportBulkDraftStartRequestDto>
{
    public EdoImportBulkDraftStartRequestDtoValidator()
    {
        RuleFor(x => x.Confirm).Equal(true);
        RuleFor(x => x.ExpectedImportPlanHash).NotEmpty().Length(64);
        RuleFor(x => x.BatchSize).Equal(50);
        RuleFor(x => x.LineValuesInvalidPolicy).Equal("SKIP");
        RuleFor(x => x.MarkingAlreadyUsedPolicy)
            .Equal("MARK_DUPLICATE_IF_ALL_SAME_PURCHASE_ELSE_SKIP");
    }
}

public sealed class EdoImportPieceTrackingApplyRequestDtoValidator
    : AbstractValidator<EdoImportPieceTrackingApplyRequestDto>
{
    public EdoImportPieceTrackingApplyRequestDtoValidator()
    {
        RuleFor(x => x.Confirm).Equal(true);
        RuleFor(x => x.ExpectedPlanHash).NotEmpty().Length(64);
        RuleFor(x => x.ProductIds).NotEmpty()
            .Must(ids => ids.All(id => id > 0) && ids.Distinct().Count() == ids.Count)
            .WithMessage("Product IDs must be positive and unique.");
    }
}
