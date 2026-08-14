using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaCommissionings;

public class FaCommissioningListDtoProjection : IProjectionBuilder<FaCommissioningDoc, FaCommissioningListDto>
{
    public Expression<Func<FaCommissioningDoc, FaCommissioningListDto>> Build() =>
        document => new FaCommissioningListDto
        {
            Id = document.Id,
            OrganizationId = document.OrganizationId,
            OrganizationName = document.Organization.ShortName,
            DocNumber = document.DocNumber,
            DocDate = document.DocDate,
            StatusId = document.StatusId,
            StatusName = document.Status.Name,
            Note = document.Note,
            AssetCount = document.Lines.Count,
            TotalCapitalizedAmount = document.Lines.Sum(line => line.CapitalizedAmount),
            StateId = document.StateId,
            StateName = document.State.FullName,
            UpdatedDate = document.UpdatedDate
        };
}
