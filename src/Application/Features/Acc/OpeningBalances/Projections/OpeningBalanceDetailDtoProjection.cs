using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Acc.OpeningBalances
{
    public class OpeningBalanceDetailDtoProjection : IProjectionBuilder<OpeningBalanceAccount, OpeningBalanceDetailDto>
    {
        private readonly IUserContext _userContext;
        public OpeningBalanceDetailDtoProjection(IUserContext userContext)
        {
            _userContext = userContext;
        }

        public Expression<Func<OpeningBalanceAccount, OpeningBalanceDetailDto>> Build()
        {
            var languageId = _userContext.LanguageId ?? LanguageIdConst.UZ;

            return x => new OpeningBalanceDetailDto
            {
                Id = x.Id,
                ChartAccountId = x.ChartAccountId,
                ChartAccountCode = x.ChartAccount.Code,
                ChartAccountName = x.ChartAccount.Name,
                ChartAccountNumber = x.ChartAccount.Number,
                CreatedDate = x.CreatedDate,
                CreditAmount = x.CreditAmount,
                DebitAmount = x.DebitAmount,
                Details = x.OpeningBalanceAccountDetails.Select(s => new OpeningBalanceAccountDetailDto
                {
                    Id = s.Id,
                    CreditAmount = s.CreditAmount,
                    DebitAmount = s.DebitAmount,
                    CurrencyId = s.CurrencyId,
                    CurrencyAmount = s.CurrencyAmount,
                    CurrencyCode = s.Currency.Code,
                    CurrencyName = s.Currency.Name,
                    Description = s.Description,
                    ExchangeRate = s.ExchangeRate,
                    OpeningBalanceAccountId = s.OpeningBalanceAccountId,
                    Quantity = s.Quantity,
                    SortOrder = s.SortOrder,
                    CreatedDate = s.CreatedDate,
                    Subkontos = s.OpeningBalanceAccountDetailSubkontos.Select(t => new OpeningBalanceAccountDetailSubkontoDto
                    {
                        SubkontoId = t.SubkontoId,
                        SubkontoTypeId = t.SubkontoTypeId,
                        CreatedDate = t.CreatedDate,
                        SortOrder = t.SortOrder,
                        SubkontoTypeCode = t.SubkontoType.Code,
                        SubkontoTypeName = t.SubkontoType.SubkontoTypeTranslations
                                                .Where(p => p.LanguageId == languageId)
                                                .Select(p => p.Name)
                                                .FirstOrDefault() ?? t.SubkontoType.Name
                    }).ToList()
                }).ToList()
            };
        }
    }
}
