using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Constants;

namespace Application.Features.Register.PostingEngines;

public class FaDepreciationRunContextBuilder : IPostingContextBuilder<FaDepreciationRun>
{
    private readonly IOrganizationAccountingPolicyResolver _accountingPolicyResolver;

    public FaDepreciationRunContextBuilder(IOrganizationAccountingPolicyResolver accountingPolicyResolver)
    {
        _accountingPolicyResolver = accountingPolicyResolver;
    }

    public async Task<List<PostingContext>> BuildAsync(FaDepreciationRun document)
    {
        var accountingPolicyId = await _accountingPolicyResolver.ResolveAsync(document.OrganizationId);

        return document.Lines.Select(line => new PostingContext
        {
            OrganizationId = document.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.FADEPRECIATION,
            AccountingPolicyId = accountingPolicyId,
            RuleId = PostingRuleIdConst.FA_DEPRECIATION,
            DocumentId = document.Id,
            DocDate = document.PeriodMonth,
            CurrencyId = CurrencyIdConst.UZS,
            JournalNumber = document.DocNumber,
            SourceLineId = line.Id,
            FixedAssetId = (int)line.FaAssetId,
            Amounts = new Dictionary<string, decimal>
            {
                [AmountSourceConst.Base] = line.Amount
            },
            Subkontos = new List<SubkontoValue>
            {
                new()
                {
                    SubkontoTypeId = SubkontoTypeIdConst.FixedAssets,
                    DisplayValue = line.FaAsset.Name,
                    EntityId = line.FaAssetId,
                    SortOrder = 1
                }
            }
        }).ToList();
    }
}
