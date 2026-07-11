using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.ChartAccountPresetAccounts
{
    public class ChartAccountPresetAccountByListFilterCriteriaBuilder : ICriteriaBuilder<ChartAccountPresetAccount, ChartAccountPresetAccountListFilter>
    {
        public Expression<Func<ChartAccountPresetAccount, bool>> Build(ChartAccountPresetAccountListFilter options)
        {
            return x => (options.PresetId == null || x.PresetId == options.PresetId) && 
                        (string.IsNullOrEmpty(options.Search) || x.Number.Contains(options.Search));
        }
    }
}
