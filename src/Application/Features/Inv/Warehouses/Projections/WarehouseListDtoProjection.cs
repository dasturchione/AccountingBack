using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Warehouses;

public class WarehouseListDtoProjection : IProjectionBuilder<Warehouse, WarehouseListDto>
{
    public Expression<Func<Warehouse, WarehouseListDto>> Build() =>
        x => new WarehouseListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            BranchId = x.BranchId,
            BranchName = x.Branch != null ? x.Branch.Name : null,
            Code = x.Code,
            Name = x.Name,
            Address = x.Address,
            IsMain = x.IsMain,
            ResponsibleUserId = x.ResponsibleUserId,
            ResponsibleUserName = x.ResponsibleUser != null ? x.ResponsibleUser.FirstName + " " + x.ResponsibleUser.LastName : null,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
