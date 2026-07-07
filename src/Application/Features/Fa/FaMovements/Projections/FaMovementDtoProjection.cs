using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaMovements;

public class FaMovementDtoProjection : IProjectionBuilder<FaMovementDoc, FaMovementDto>
{
    public Expression<Func<FaMovementDoc, FaMovementDto>> Build() =>
        x => new FaMovementDto
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
            CreatedDate = x.CreatedDate,
            CreatedByUserId = x.CreatedByUserId,
            UpdatedDate = x.UpdatedDate,
            UpdatedByUserId = x.UpdatedByUserId,
            PostedAt = x.PostedAt,
            PostedByUserId = x.PostedByUserId,
            CancelledAt = x.CancelledAt,
            CancelledByUserId = x.CancelledByUserId,
            Lines = x.Lines.Select(line => new FaMovementLineDto
            {
                Id = line.Id,
                MovementDocId = line.MovementDocId,
                FaAssetId = line.FaAssetId,
                InventoryNumber = line.FaAsset.InventoryNumber,
                AssetName = line.FaAsset.Name,
                DepartmentId = line.FaAsset.DepartmentId,
                DepartmentName = line.FaAsset.Department != null ? line.FaAsset.Department.Name : null,
                ResponsibleUserId = line.FaAsset.ResponsibleUserId,
                ResponsibleUserName = line.FaAsset.ResponsibleUser != null ? line.FaAsset.ResponsibleUser.FirstName + " " + line.FaAsset.ResponsibleUser.LastName : null,
                AssetStatusId = line.FaAsset.StatusId,
                AssetStatusName = line.FaAsset.Status.Name,
                Note = line.Note
            }).ToList()
        };
}
