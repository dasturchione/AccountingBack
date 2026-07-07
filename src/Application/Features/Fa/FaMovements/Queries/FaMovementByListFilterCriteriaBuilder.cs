using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaMovements;

public class FaMovementByListFilterCriteriaBuilder : ICriteriaBuilder<FaMovementDoc, FaMovementListFilter>
{
    public Expression<Func<FaMovementDoc, bool>> Build(FaMovementListFilter options) =>
        x => (!options.StatusId.HasValue || x.StatusId == options.StatusId.Value) &&
             (!options.DepartmentId.HasValue || x.FromDepartmentId == options.DepartmentId.Value || x.ToDepartmentId == options.DepartmentId.Value) &&
             (!options.ResponsibleUserId.HasValue || x.FromResponsibleUserId == options.ResponsibleUserId.Value || x.ToResponsibleUserId == options.ResponsibleUserId.Value) &&
             (!options.DateFrom.HasValue || x.DocDate >= options.DateFrom.Value) &&
             (!options.DateTo.HasValue || x.DocDate <= options.DateTo.Value);
}
