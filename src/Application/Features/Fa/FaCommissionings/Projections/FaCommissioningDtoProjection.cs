using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaCommissionings;

public class FaCommissioningDtoProjection : IProjectionBuilder<FaCommissioningDoc, FaCommissioningDto>
{
    public Expression<Func<FaCommissioningDoc, FaCommissioningDto>> Build() =>
        document => new FaCommissioningDto
        {
            Id = document.Id,
            OrganizationId = document.OrganizationId,
            OrganizationName = document.Organization.ShortName,
            DocNumber = document.DocNumber,
            DocDate = document.DocDate,
            StatusId = document.StatusId,
            StatusName = document.Status.Name,
            Note = document.Note,
            StateId = document.StateId,
            StateName = document.State.FullName,
            CreatedDate = document.CreatedDate,
            CreatedByUserId = document.CreatedByUserId,
            UpdatedDate = document.UpdatedDate,
            UpdatedByUserId = document.UpdatedByUserId,
            PostedAt = document.PostedAt,
            PostedByUserId = document.PostedByUserId,
            CancelledAt = document.CancelledAt,
            CancelledByUserId = document.CancelledByUserId,
            TotalCapitalizedAmount = document.Lines.Sum(line => line.CapitalizedAmount),
            Lines = document.Lines.Select(line => new FaCommissioningLineDto
            {
                Id = line.Id,
                CommissioningDocId = line.CommissioningDocId,
                FaAssetId = line.FaAssetId,
                InventoryNumber = line.FaAsset.InventoryNumber,
                AssetName = line.FaAsset.Name,
                CapitalizedAmount = line.CapitalizedAmount,
                DeprStartDate = line.DeprStartDate,
                SalvageValue = line.SalvageValue,
                UsefulLifeMonths = line.UsefulLifeMonths,
                DepreciationMethodId = line.DepreciationMethodId,
                DepreciationMethodCode = line.DepreciationMethod.Code,
                DepreciationMethodName = line.DepreciationMethod.Name,
                PlannedUnitsTotal = line.PlannedUnitsTotal,
                DepartmentId = line.DepartmentId,
                DepartmentName = line.Department != null ? line.Department.Name : null,
                ResponsibleUserId = line.ResponsibleUserId,
                ResponsibleUserName = line.ResponsibleUser != null
                    ? line.ResponsibleUser.FirstName + " " + line.ResponsibleUser.LastName
                    : null,
                AssetAccountId = line.FaAsset.FaAssetAccounting!.AssetAccountId,
                AssetAccountNumber = line.FaAsset.FaAssetAccounting!.AssetAccount.Number,
                AssetAccountName = line.FaAsset.FaAssetAccounting!.AssetAccount.Name,
                CapitalInvestmentAccountId = line.CapitalInvestmentAccountId,
                CapitalInvestmentAccountNumber = line.CapitalInvestmentAccount.Number,
                CapitalInvestmentAccountName = line.CapitalInvestmentAccount.Name,
                AccumulatedDepreciationAccountId = line.AccumulatedDepreciationAccountId,
                AccumulatedDepreciationAccountNumber = line.AccumulatedDepreciationAccount.Number,
                AccumulatedDepreciationAccountName = line.AccumulatedDepreciationAccount.Name,
                DepreciationExpenseAccountId = line.DepreciationExpenseAccountId,
                DepreciationExpenseAccountNumber = line.DepreciationExpenseAccount.Number,
                DepreciationExpenseAccountName = line.DepreciationExpenseAccount.Name,
                Note = line.Note
            }).ToList()
        };
}
