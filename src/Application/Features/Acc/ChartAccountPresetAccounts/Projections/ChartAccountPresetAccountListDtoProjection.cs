using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.ChartAccountPresetAccounts
{
    public class ChartAccountPresetAccountListDtoProjection : IProjectionBuilder<ChartAccountPresetAccount, ChartAccountPresetAccountListDto>
    {
        private readonly IUserContext _userContext;
        public ChartAccountPresetAccountListDtoProjection(IUserContext userContext)
        {
            _userContext = userContext;
        }

        public Expression<Func<ChartAccountPresetAccount, ChartAccountPresetAccountListDto>> Build()
        {
            var languageId = _userContext.LanguageId ?? LanguageIdConst.UZ;

            return x => new ChartAccountPresetAccountListDto
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
                            ? x.ChartAccountPresetAccountTranslations.First().Name 
                            : x.Name,
                StateId = x.StateId,
                StateName = x.State.FullName,
                Number = x.Number,
                ParentNumber = x.ChartAccountPresetAccountNavigation != null ? x.ChartAccountPresetAccountNavigation.Number : null,
                ParentPresetAccountId = x.ParentPresetAccountId,
                CreatedDate = x.CreatedDate,
            };
        }
    }
}
