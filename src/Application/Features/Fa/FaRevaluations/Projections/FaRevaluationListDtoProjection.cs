using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaRevaluations;

public class FaRevaluationListDtoProjection : IProjectionBuilder<FaRevaluationDoc, FaRevaluationListDto>
{
    public Expression<Func<FaRevaluationDoc, FaRevaluationListDto>> Build() =>
        x => new FaRevaluationListDto
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
            Lines = new List<FaRevaluationLineDto>()
        };
}
