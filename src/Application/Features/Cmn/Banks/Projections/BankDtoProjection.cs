using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Banks;

public class BankDtoProjection : IProjectionBuilder<Bank, BankDto>
{
    public Expression<Func<Bank, BankDto>> Build() =>
        x => new BankDto
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            Mfo = x.Mfo,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
