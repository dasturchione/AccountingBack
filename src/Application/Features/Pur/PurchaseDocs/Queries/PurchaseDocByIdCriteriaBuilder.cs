using Application.Options;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.PurchaseDocs;

public class PurchaseDocByIdCriteriaBuilder : ICriteriaBuilder<PurchaseDoc, GetByIdOptions<long>>
{
    public Expression<Func<PurchaseDoc, bool>> Build(GetByIdOptions<long> options) => x => x.Id == options.Id;
}
