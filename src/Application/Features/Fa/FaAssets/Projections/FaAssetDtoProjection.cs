using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaAssets;

public class FaAssetDtoProjection : IProjectionBuilder<FaAsset, FaAssetDto>
{
    public Expression<Func<FaAsset, FaAssetDto>> Build() =>
        asset => new FaAssetDto
        {
            Id = asset.Id,
            OrganizationId = asset.OrganizationId,
            OrganizationName = asset.Organization.ShortName,
            InventoryNumber = asset.InventoryNumber,
            Name = asset.Name,
            FaGroupId = asset.FaGroupId,
            FaGroupCode = asset.FaGroup.Code,
            FaGroupName = asset.FaGroup.Name,
            OkofId = asset.OkofId,
            OkofCode = asset.Okof != null ? asset.Okof.Code : null,
            OkofName = asset.Okof != null ? asset.Okof.Name : null,
            DepreciationMethodId = asset.FaAssetAccounting != null ? asset.FaAssetAccounting.DepreciationMethodId : null,
            DepreciationMethodCode = asset.FaAssetAccounting != null && asset.FaAssetAccounting.DepreciationMethod != null
                ? asset.FaAssetAccounting.DepreciationMethod.Code
                : null,
            DepreciationMethodName = asset.FaAssetAccounting != null && asset.FaAssetAccounting.DepreciationMethod != null
                ? asset.FaAssetAccounting.DepreciationMethod.Name
                : null,
            UsefulLifeMonths = asset.FaAssetAccounting != null ? asset.FaAssetAccounting.UsefulLifeMonths : null,
            InitialCost = asset.FaAssetAccounting != null ? asset.FaAssetAccounting.InitialCost : 0m,
            SalvageValue = asset.FaAssetAccounting != null ? asset.FaAssetAccounting.SalvageValue : null,
            CommissioningDate = asset.FaCommissioningDocLines
                .Where(line => line.CommissioningDoc.StatusId == DocumentStatusIdConst.POSTED)
                .OrderByDescending(line => line.CommissioningDoc.DocDate)
                .Select(line => (DateTime?)line.CommissioningDoc.DocDate)
                .FirstOrDefault(),
            DeprStartDate = asset.FaAssetAccounting != null ? asset.FaAssetAccounting.DeprStartDate : null,
            PlannedUnitsTotal = asset.FaAssetAccounting != null ? asset.FaAssetAccounting.PlannedUnitsTotal : null,
            DepartmentId = asset.DepartmentId,
            DepartmentName = asset.Department != null ? asset.Department.Name : null,
            ResponsibleUserId = asset.ResponsibleUserId,
            ResponsibleUserName = asset.ResponsibleUser != null
                ? asset.ResponsibleUser.FirstName + " " + asset.ResponsibleUser.LastName
                : null,
            AssetAccountId = asset.FaAssetAccounting != null ? asset.FaAssetAccounting.AssetAccountId : null,
            AccumulatedDepreciationAccountId = asset.FaAssetAccounting != null
                ? asset.FaAssetAccounting.AccumulatedDepreciationAccountId
                : null,
            DepreciationExpenseAccountId = asset.FaAssetAccounting != null
                ? asset.FaAssetAccounting.DepreciationExpenseAccountId
                : null,
            StatusId = asset.StatusId,
            StatusCode = asset.Status.Code,
            StatusName = asset.Status.Name,
            StateId = asset.StateId,
            StateName = asset.State.FullName,
            CreatedDate = asset.CreatedDate,
            UpdatedDate = asset.UpdatedDate
        };
}
