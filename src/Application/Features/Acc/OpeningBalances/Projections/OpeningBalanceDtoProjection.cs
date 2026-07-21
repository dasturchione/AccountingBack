using Application.Abstractions.Authentication;
using Application.Features.OrgBankAccounts;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Acc.OpeningBalances
{
    public class OpeningBalanceDtoProjection : IProjectionBuilder<OpeningBalance, OpeningBalanceDto>
    {
        private readonly IUserContext _userContext;
        public OpeningBalanceDtoProjection(IUserContext userContext)
        {
            _userContext = userContext;
        }

        public Expression<Func<OpeningBalance, OpeningBalanceDto>> Build()
        {
            var languageId = _userContext.LanguageId ?? LanguageIdConst.UZ;

            return x => new OpeningBalanceDto
            {
                Id = x.Id,
                BalanceDate = x.BalanceDate,
                Description = x.Description,
                StateId = x.StateId,
                OrganizationId = x.OrganizationId,
                OrganizationName = x.Organization.FullName,
                CreatedDate = x.CreatedDate,
                StateName = x.State.FullName,
                Accounts = x.OpeningBalanceAccounts.OrderBy(s => s.ChartAccount.Number).Select(s => new OpeningBalanceAccountDto
                {
                    Id = s.Id, 
                    ChartAccountCode = s.ChartAccount.Code,
                    ChartAccountId = s.ChartAccountId,
                    CreatedDate = s.CreatedDate,
                    ChartAccountName = s.ChartAccount.Name,
                    ChartAccountNumber = s.ChartAccount.Number,
                    CreditAmount = s.CreditAmount,
                    DebitAmount = s.DebitAmount,
                }).ToList()
            };
        }
    }
}
