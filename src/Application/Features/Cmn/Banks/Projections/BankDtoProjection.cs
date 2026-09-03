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
            LegalName = x.LegalName,
            LicenseNumber = x.LicenseNumber,
            LicenseDate = x.LicenseDate,
            Address = x.Address,
            OpenedDate = x.OpenedDate,
            SourceUpdatedDate = x.SourceUpdatedDate,
            Inn = x.Inn,
            Website = x.Website,
            Latitude = x.Latitude,
            Longitude = x.Longitude,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
