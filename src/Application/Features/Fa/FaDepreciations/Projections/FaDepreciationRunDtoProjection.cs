using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaDepreciations;

public class FaDepreciationRunDtoProjection : IProjectionBuilder<FaDepreciationRun, FaDepreciationRunDto>
{
    public Expression<Func<FaDepreciationRun, FaDepreciationRunDto>> Build() =>
        x => new FaDepreciationRunDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            DocNumber = x.DocNumber,
            PeriodMonth = x.PeriodMonth,
            StatusId = x.StatusId,
            StatusName = x.Status.Name,
            Note = x.Note,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate,
            CreatedByUserId = x.CreatedByUserId,
            UpdatedDate = x.UpdatedDate,
            UpdatedByUserId = x.UpdatedByUserId,
            PostedAt = x.PostedAt,
            PostedByUserId = x.PostedByUserId,
            CancelledAt = x.CancelledAt,
            CancelledByUserId = x.CancelledByUserId,
            TotalAmount = x.Lines.Sum(line => line.Amount),
            Lines = x.Lines.Select(line => new FaDepreciationRunLineDto
            {
                Id = line.Id,
                DepreciationRunId = line.DepreciationRunId,
                FaAssetId = line.FaAssetId,
                InventoryNumber = line.FaAsset.InventoryNumber,
                AssetName = line.FaAsset.Name,
                DepreciationMethodId = line.FaAsset.FaAssetAccounting!.DepreciationMethodId!.Value,
                DepreciationMethodCode = line.FaAsset.FaAssetAccounting.DepreciationMethod!.Code,
                DepreciationMethodName = line.FaAsset.FaAssetAccounting.DepreciationMethod.Name,
                Amount = line.Amount,
                ExpenseAccountId = line.ExpenseAccountId,
                AccumulatedDepreciationAccountId = line.AccumulatedDepreciationAccountId,
                Note = line.Note
            }).ToList()
        };
}
