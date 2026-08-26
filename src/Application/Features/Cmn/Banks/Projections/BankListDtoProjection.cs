using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Banks;

public class BankListDtoProjection : IProjectionBuilder<Bank, BankListDto>
{
    public Expression<Func<Bank, BankListDto>> Build() =>
        x => new BankListDto
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            LegalName = x.LegalName,
            LicenseNumber = x.LicenseNumber,
            LicenseDate = x.LicenseDate,
            Inn = x.Inn,
            Website = x.Website,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
