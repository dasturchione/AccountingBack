using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.ChartAccountPresetAccounts
{
    public class ChartAccountPresetAccountGroupedListDtoProjection : IProjectionBuilder<ChartAccountPresetAccount, ChartAccountPresetAccountGroupedListDto>
    {
        private readonly IUserContext _userContext;
        public ChartAccountPresetAccountGroupedListDtoProjection(IUserContext userContext)
        {
            _userContext = userContext;
        }

        public Expression<Func<ChartAccountPresetAccount, ChartAccountPresetAccountGroupedListDto>> Build()
        {
            var languageId = _userContext.LanguageId ?? LanguageIdConst.UZ;

            return x => new ChartAccountPresetAccountGroupedListDto
            {
                Id = x.Id,
                AccountTypeCode = x.AccountType.Code,
                AccountTypeId = x.AccountTypeId,
                AccountTypeName = x.AccountType.Name,
                DisplayOrder = x.DisplayOrder,
                Code = x.Code,
                IsCurrency = x.IsCurrency,
                IsDepartment = x.IsDepartment,
                IsGroup = x.IsGroup,
                IsOffBalance = x.IsOffBalance,
                IsQuantity = x.IsQuantity,
                IsTaxAccounting = x.IsTaxAccounting,
                PresetId = x.PresetId,
                Name = x.ChartAccountPresetAccountTranslations.Any()
                            ? x.ChartAccountPresetAccountTranslations.First(f => f.LanguageId == languageId).Name
                            : x.Name,
                StateId = x.StateId,
                StateName = x.State.FullName,
                Number = x.Number,
                ParentNumber = x.ChartAccountPresetAccountNavigation != null ? x.ChartAccountPresetAccountNavigation.Number : null,
                ParentPresetAccountId = x.ParentPresetAccountId,
                CreatedDate = x.CreatedDate,
                Lines = x.InverseChartAccountPresetAccountNavigation
                            .Select(y => new ChartAccountPresetAccountListDto
                            {
                                Id = y.Id,
                                AccountTypeCode = y.AccountType.Code,
                                AccountTypeId = y.AccountTypeId,
                                AccountTypeName = y.AccountType.Name,
                                DisplayOrder = y.DisplayOrder,
                                Code = y.Code,
                                IsCurrency = y.IsCurrency,
                                IsDepartment = y.IsDepartment,
                                IsGroup = y.IsGroup,
                                IsOffBalance = y.IsOffBalance,
                                IsQuantity = y.IsQuantity,
                                IsTaxAccounting = y.IsTaxAccounting,
                                PresetId = y.PresetId,
                                Name = y.ChartAccountPresetAccountTranslations.Any()
                                            ? y.ChartAccountPresetAccountTranslations.First(f => f.LanguageId == languageId).Name
                                            : y.Name,
                                StateId = y.StateId,
                                StateName = y.State.FullName,
                                Number = y.Number,
                                ParentNumber = y.ChartAccountPresetAccountNavigation != null ? y.ChartAccountPresetAccountNavigation.Number : null,
                                ParentPresetAccountId = y.ParentPresetAccountId,
                                CreatedDate = y.CreatedDate
                            })
                            .ToList()
            };
        }
    }
}
