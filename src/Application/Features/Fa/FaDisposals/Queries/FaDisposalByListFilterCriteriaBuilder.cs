using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaDisposals;

public class FaDisposalByListFilterCriteriaBuilder : ICriteriaBuilder<FaDisposalDoc, FaDisposalListFilter>
{
    public Expression<Func<FaDisposalDoc, bool>> Build(FaDisposalListFilter options) =>
        x => (!options.StatusId.HasValue || x.StatusId == options.StatusId.Value) &&
             (string.IsNullOrWhiteSpace(options.DisposalType) || x.DisposalType == options.DisposalType) &&
             (!options.DateFrom.HasValue || x.DisposalDate >= options.DateFrom.Value) &&
             (!options.DateTo.HasValue || x.DisposalDate <= options.DateTo.Value);
}
