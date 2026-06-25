using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Banks;

public class BankByListFilterCriteriaBuilder : ICriteriaBuilder<Bank, BankListFilter>
{
    public Expression<Func<Bank, bool>> Build(BankListFilter options) =>
        x => !options.StateId.HasValue || x.StateId == options.StateId.Value;
}
