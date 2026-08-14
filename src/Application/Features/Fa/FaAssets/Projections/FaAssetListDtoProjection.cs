using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaAssets;

public class FaAssetListDtoProjection : IProjectionBuilder<FaAsset, FaAssetListDto>
{
    public Expression<Func<FaAsset, FaAssetListDto>> Build() =>
        asset => new FaAssetListDto
        {
            Id = asset.Id,
            OrganizationId = asset.OrganizationId,
            OrganizationName = asset.Organization.ShortName,
            InventoryNumber = asset.InventoryNumber,
            Name = asset.Name,
            FaGroupId = asset.FaGroupId,
            FaGroupName = asset.FaGroup.Name,
            DepreciationMethodId = asset.FaAssetAccounting != null ? asset.FaAssetAccounting.DepreciationMethodId : null,
            DepreciationMethodName = asset.FaAssetAccounting != null && asset.FaAssetAccounting.DepreciationMethod != null
                ? asset.FaAssetAccounting.DepreciationMethod.Name
                : null,
            InitialCost = asset.FaAssetAccounting != null ? asset.FaAssetAccounting.InitialCost : 0m,
            CommissioningDate = asset.FaCommissioningDocLines
                .Where(line => line.CommissioningDoc.StatusId == DocumentStatusIdConst.POSTED)
                .OrderByDescending(line => line.CommissioningDoc.DocDate)
                .Select(line => (DateTime?)line.CommissioningDoc.DocDate)
                .FirstOrDefault(),
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
            StatusName = asset.Status.Name,
            StateId = asset.StateId,
            StateName = asset.State.FullName,
            UpdatedDate = asset.UpdatedDate
        };
}
