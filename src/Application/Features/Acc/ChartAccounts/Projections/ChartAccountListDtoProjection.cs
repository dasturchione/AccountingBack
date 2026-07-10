using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.ChartAccounts;

public class ChartAccountListDtoProjection : IProjectionBuilder<ChartAccount, ChartAccountListDto>
{
    public Expression<Func<ChartAccount, ChartAccountListDto>> Build() =>
        x => new ChartAccountListDto
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            Number = x.Number,
            ParentId = x.ParentId,
            ParentName = x.Parent != null ? x.Parent.Name : null,
            ParentCode = x.Parent != null ? x.Parent.Code : null,
            ParentNumber = x.Parent != null ? x.Parent.Number : null,
            AccountTypeId = x.AccountTypeId,
            AccountTypeName = x.AccountType != null ? x.AccountType.Name : null,
            AccountTypeCode = x.AccountType != null ? x.AccountType.Code : null,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.FullName,
            IsGroup = x.IsGroup,
            IsCurrency = x.IsCurrency,
            IsDepartment = x.IsDepartment,
            IsOffBalance = x.IsOffBalance,
            IsQuantity = x.IsQuantity,
            IsTaxAccounting = x.IsTaxAccounting,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
