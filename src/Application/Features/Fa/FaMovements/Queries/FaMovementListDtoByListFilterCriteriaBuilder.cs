using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.FaMovements;

public class FaMovementListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<FaMovementListDto, FaMovementListFilter>
{
    public Expression<Func<FaMovementListDto, bool>> Build(FaMovementListFilter options) =>
        x => (!options.StatusId.HasValue || x.StatusId == options.StatusId.Value) &&
             (!options.DepartmentId.HasValue || x.FromDepartmentId == options.DepartmentId.Value || x.ToDepartmentId == options.DepartmentId.Value) &&
             (!options.ResponsibleUserId.HasValue || x.FromResponsibleUserId == options.ResponsibleUserId.Value || x.ToResponsibleUserId == options.ResponsibleUserId.Value) &&
             (!options.DateFrom.HasValue || x.DocDate >= options.DateFrom.Value) &&
             (!options.DateTo.HasValue || x.DocDate <= options.DateTo.Value) &&
             (string.IsNullOrWhiteSpace(options.Search) ||
              x.DocNumber.ToLower().Contains(options.Search.ToLower()) ||
              (x.FromDepartmentName != null && x.FromDepartmentName.ToLower().Contains(options.Search.ToLower())) ||
              (x.ToDepartmentName != null && x.ToDepartmentName.ToLower().Contains(options.Search.ToLower())) ||
              (x.FromResponsibleUserName != null && x.FromResponsibleUserName.ToLower().Contains(options.Search.ToLower())) ||
              (x.ToResponsibleUserName != null && x.ToResponsibleUserName.ToLower().Contains(options.Search.ToLower())) ||
              (x.Note != null && x.Note.ToLower().Contains(options.Search.ToLower())) ||
              x.StatusName.ToLower().Contains(options.Search.ToLower()));
}
