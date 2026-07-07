using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaDepreciations;

public class FaDepreciationRunListDtoProjection : IProjectionBuilder<FaDepreciationRun, FaDepreciationRunListDto>
{
    public Expression<Func<FaDepreciationRun, FaDepreciationRunListDto>> Build() =>
        x => new FaDepreciationRunListDto
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
            Lines = new List<FaDepreciationRunLineDto>()
        };
}
