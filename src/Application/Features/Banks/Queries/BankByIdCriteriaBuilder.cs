using Application.Options;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Banks;

public class BankByIdCriteriaBuilder : ICriteriaBuilder<Bank, GetByIdOptions<int>>
{
    public Expression<Func<Bank, bool>> Build(GetByIdOptions<int> options) => b => b.Id == options.Id;
}
