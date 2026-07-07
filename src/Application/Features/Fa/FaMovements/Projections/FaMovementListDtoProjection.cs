using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaMovements;

public class FaMovementListDtoProjection : IProjectionBuilder<FaMovementDoc, FaMovementListDto>
{
    public Expression<Func<FaMovementDoc, FaMovementListDto>> Build() =>
        x => new FaMovementListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            DocNumber = x.DocNumber,
            DocDate = x.DocDate,
            StatusId = x.StatusId,
            StatusName = x.Status.Name,
            FromDepartmentId = x.FromDepartmentId,
            FromDepartmentName = x.FromDepartment != null ? x.FromDepartment.Name : null,
            ToDepartmentId = x.ToDepartmentId,
            ToDepartmentName = x.ToDepartment != null ? x.ToDepartment.Name : null,
            FromResponsibleUserId = x.FromResponsibleUserId,
            FromResponsibleUserName = x.FromResponsibleUser != null ? x.FromResponsibleUser.FirstName + " " + x.FromResponsibleUser.LastName : null,
            ToResponsibleUserId = x.ToResponsibleUserId,
            ToResponsibleUserName = x.ToResponsibleUser != null ? x.ToResponsibleUser.FirstName + " " + x.ToResponsibleUser.LastName : null,
            Note = x.Note,
            StateId = x.StateId,
            StateName = x.State.FullName,
            UpdatedDate = x.UpdatedDate
        };
}
