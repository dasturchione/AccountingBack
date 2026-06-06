using Application.Options;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.SaleDocs;

public class SaleDocByIdCriteriaBuilder : ICriteriaBuilder<SaleDoc, GetByIdOptions<long>>
{
    public Expression<Func<SaleDoc, bool>> Build(GetByIdOptions<long> options) => x => x.Id == options.Id;
}
