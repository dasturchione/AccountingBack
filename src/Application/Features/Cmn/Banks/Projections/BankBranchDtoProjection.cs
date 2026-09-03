using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Banks;

public class BankBranchDtoProjection : IProjectionBuilder<BankBranch, BankBranchDto>
{
    public Expression<Func<BankBranch, BankBranchDto>> Build() =>
        x => new BankBranchDto
        {
            Id = x.Id,
            BankId = x.BankId,
            BankCode = x.Bank.Code,
            BankName = x.Bank.Name,
            Mfo = x.Mfo,
            BranchType = x.BranchType,
            Name = x.Name,
            Address = x.Address,
            OpenedDate = x.OpenedDate,
            SourceUpdatedDate = x.SourceUpdatedDate,
            RegionId = x.RegionId,
            RegionName = x.Region != null ? x.Region.FullName : null,
            DistrictId = x.DistrictId,
            DistrictName = x.District != null ? x.District.FullName : null,
            City = x.City,
            Inn = x.Inn,
            Website = x.Website,
            Latitude = x.Latitude,
            Longitude = x.Longitude,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
