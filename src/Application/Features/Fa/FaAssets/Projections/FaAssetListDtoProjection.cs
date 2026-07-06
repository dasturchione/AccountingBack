using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaAssets;

public class FaAssetListDtoProjection : IProjectionBuilder<FaAsset, FaAssetListDto>
{
    public Expression<Func<FaAsset, FaAssetListDto>> Build() =>
        x => new FaAssetListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            InventoryNumber = x.InventoryNumber,
            Name = x.Name,
            FaGroupId = x.FaGroupId,
            FaGroupName = x.FaGroup.Name,
            DepreciationMethodId = x.DepreciationMethodId,
            DepreciationMethodName = x.DepreciationMethod.Name,
            InitialCost = x.InitialCost,
            CommissioningDate = x.CommissioningDate,
            DepartmentId = x.DepartmentId,
            DepartmentName = x.Department != null ? x.Department.Name : null,
            ResponsibleUserId = x.ResponsibleUserId,
            ResponsibleUserName = x.ResponsibleUser != null
                ? x.ResponsibleUser.FirstName + " " + x.ResponsibleUser.LastName
                : null,
            StatusId = x.StatusId,
            StatusName = x.Status.Name,
            StateId = x.StateId,
            StateName = x.State.FullName,
            UpdatedDate = x.UpdatedDate
        };
}
