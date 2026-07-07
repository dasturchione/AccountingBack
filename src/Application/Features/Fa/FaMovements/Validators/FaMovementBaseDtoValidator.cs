using FluentValidation;

namespace Application.Features.FaMovements;

public class FaMovementBaseDtoValidator : AbstractValidator<FaMovementBaseDto>
{
    public FaMovementBaseDtoValidator()
    {
        RuleFor(x => x.DocDate).NotEmpty();
        RuleFor(x => x.Note).MaximumLength(500);
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).SetValidator(new FaMovementLineWriteDtoValidator());

        RuleFor(x => x)
            .Must(x => x.ToDepartmentId.HasValue || x.ToResponsibleUserId.HasValue)
            .WithMessage("Movement target must contain at least one destination value.");
    }
}

public class FaMovementLineWriteDtoValidator : AbstractValidator<FaMovementLineWriteDto>
{
    public FaMovementLineWriteDtoValidator()
    {
        RuleFor(x => x.FaAssetId).GreaterThan(0);
        RuleFor(x => x.Note).MaximumLength(500);
    }
}
