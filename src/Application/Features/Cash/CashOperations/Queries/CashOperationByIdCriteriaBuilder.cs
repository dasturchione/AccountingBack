using Application.Options;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.CashOperations;

public class CashOperationByIdCriteriaBuilder : ICriteriaBuilder<CashOperation, GetByIdOptions<long>>
{
    public Expression<Func<CashOperation, bool>> Build(GetByIdOptions<long> options) => x => x.Id == options.Id;
}
