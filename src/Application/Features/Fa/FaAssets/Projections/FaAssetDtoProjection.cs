using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaAssets;

public class FaAssetDtoProjection : IProjectionBuilder<FaAsset, FaAssetDto>
{
    public Expression<Func<FaAsset, FaAssetDto>> Build() =>
        x => new FaAssetDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            InventoryNumber = x.InventoryNumber,
            Name = x.Name,
            FaGroupId = x.FaGroupId,
            FaGroupCode = x.FaGroup.Code,
            FaGroupName = x.FaGroup.Name,
            OkofId = x.OkofId,
            OkofCode = x.Okof != null ? x.Okof.Code : null,
            OkofName = x.Okof != null ? x.Okof.Name : null,
            DepreciationMethodId = x.DepreciationMethodId,
            DepreciationMethodCode = x.DepreciationMethod.Code,
            DepreciationMethodName = x.DepreciationMethod.Name,
            UsefulLifeMonths = x.UsefulLifeMonths,
            InitialCost = x.InitialCost,
            SalvageValue = x.SalvageValue,
            CommissioningDate = x.CommissioningDate,
            DeprStartDate = x.DeprStartDate,
            PlannedUnitsTotal = x.PlannedUnitsTotal,
            DepartmentId = x.DepartmentId,
            DepartmentName = x.Department != null ? x.Department.Name : null,
            ResponsibleUserId = x.ResponsibleUserId,
            AssetAccountId = x.AssetAccountId,
            AccumulatedDepreciationAccountId = x.AccumulatedDepreciationAccountId,
            DepreciationExpenseAccountId = x.DepreciationExpenseAccountId,
            ResponsibleUserName = x.ResponsibleUser != null
                ? x.ResponsibleUser.FirstName + " " + x.ResponsibleUser.LastName
                : null,
            StatusId = x.StatusId,
            StatusCode = x.Status.Code,
            StatusName = x.Status.Name,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate,
            UpdatedDate = x.UpdatedDate
        };
}
