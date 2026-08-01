using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaRevaluations;

public class FaRevaluationDtoProjection : IProjectionBuilder<FaRevaluationDoc, FaRevaluationDto>
{
    public Expression<Func<FaRevaluationDoc, FaRevaluationDto>> Build() =>
        x => new FaRevaluationDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            DocNumber = x.DocNumber,
            RevaluationDate = x.RevaluationDate,
            StatusId = x.StatusId,
            StatusName = x.Status.Name,
            Reason = x.Reason,
            RevaluationReserveAccountId = x.RevaluationReserveAccountId,
            RevaluationLossAccountId = x.RevaluationLossAccountId,
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
            TotalRevaluationAmount = x.Lines.Sum(line => line.RevaluationAmount),
            Lines = x.Lines.Select(line => new FaRevaluationLineDto
            {
                Id = line.Id,
                RevaluationDocId = line.RevaluationDocId,
                FaAssetId = line.FaAssetId,
                InventoryNumber = line.FaAsset.InventoryNumber,
                AssetName = line.FaAsset.Name,
                OldValue = line.OldValue,
                NewValue = line.NewValue,
                RevaluationAmount = line.RevaluationAmount,
                Note = line.Note,
                AssetAccountId = line.AssetAccountId,
                AccumulatedDepreciationAccountId = line.AccumulatedDepreciationAccountId
            }).ToList()
        };
}
