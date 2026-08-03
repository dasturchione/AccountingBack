using FluentValidation;
using SharedKernel.Constants;

namespace Application.Features.FaDisposals;

public class FaDisposalBaseDtoValidator : AbstractValidator<FaDisposalBaseDto>
{
    public FaDisposalBaseDtoValidator()
    {
        RuleFor(x => x.DisposalDate).NotEmpty();
        RuleFor(x => x.DisposalTypeId)
            .Must(x => x is FaDisposalTypeIdConst.SALE or FaDisposalTypeIdConst.WRITEOFF or FaDisposalTypeIdConst.BREAKDOWN);
        RuleFor(x => x.Reason).MaximumLength(500).When(x => x.Reason is not null);
        RuleFor(x => x.StateId).GreaterThan((short)0);
        RuleFor(x => x.Lines).NotEmpty();
        RuleFor(x => x.Lines)
            .Must(lines => lines.Any(line => line.SaleAmount > 0m))
            .When(x => x.DisposalTypeId == FaDisposalTypeIdConst.SALE);
        RuleForEach(x => x.Lines).SetValidator(new FaDisposalLineWriteDtoValidator());
    }
}

public class FaDisposalLineWriteDtoValidator : AbstractValidator<FaDisposalLineWriteDto>
{
    public FaDisposalLineWriteDtoValidator()
    {
        RuleFor(x => x.FaAssetId).GreaterThan(0);
        RuleFor(x => x.SaleAmount).GreaterThanOrEqualTo(0m);
        RuleFor(x => x.Note).MaximumLength(500).When(x => x.Note is not null);
    }
}
