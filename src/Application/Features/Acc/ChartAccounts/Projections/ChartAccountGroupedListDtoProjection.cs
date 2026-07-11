using Application.Features.ChartAccounts;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Acc.ChartAccounts
{
    public class ChartAccountGroupedListDtoProjection : IProjectionBuilder<ChartAccount, ChartAccountGroupedListDto>
    {
        public Expression<Func<ChartAccount, ChartAccountGroupedListDto>> Build() => 
            x => new ChartAccountGroupedListDto
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
                CreatedDate = x.CreatedDate,
                Lines = x.InverseParent.Select(s => new ChartAccountListDto
                {
                    Id = s.Id,
                    Code = s.Code,
                    Name = s.Name,
                    Number = s.Number,
                    ParentId = s.ParentId,
                    ParentName = s.Parent != null ? s.Parent.Name : null,
                    ParentCode = s.Parent != null ? s.Parent.Code : null,
                    ParentNumber = s.Parent != null ? s.Parent.Number : null,
                    AccountTypeId = s.AccountTypeId,
                    AccountTypeName = s.AccountType != null ? s.AccountType.Name : null,
                    AccountTypeCode = s.AccountType != null ? s.AccountType.Code : null,
                    OrganizationId = s.OrganizationId,
                    OrganizationName = s.Organization.FullName,
                    IsGroup = s.IsGroup,
                    IsCurrency = s.IsCurrency,
                    IsDepartment = s.IsDepartment,
                    IsOffBalance = s.IsOffBalance,
                    IsQuantity = s.IsQuantity,
                    IsTaxAccounting = s.IsTaxAccounting,
                    StateId = s.StateId,
                    StateName = s.State.FullName,
                    CreatedDate = s.CreatedDate,
                }).ToList()
            };
    }
}
