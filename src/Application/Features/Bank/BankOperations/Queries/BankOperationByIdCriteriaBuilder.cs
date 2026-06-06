using Application.Options;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.BankOperations;

public class BankOperationByIdCriteriaBuilder : ICriteriaBuilder<BankOperation, GetByIdOptions<long>>
{
    public Expression<Func<BankOperation, bool>> Build(GetByIdOptions<long> options) => x => x.Id == options.Id;
}
