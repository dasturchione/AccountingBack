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
            DocumentId = document.Id,
            DocDate = document.PeriodMonth,
            CurrencyId = CurrencyIdConst.UZS,
            JournalNumber = document.DocNumber,
            SourceLineId = line.Id,
            FixedAssetId = (int)line.FaAssetId,
            Entries = new List<PostingEntryContext>
            {
                new()
                {
                    DebitAccountId = line.ExpenseAccountId,
                    CreditAccountId = line.AccumulatedDepreciationAccountId,
                    Amount = line.Amount,
                    Content = "Fixed asset depreciation",
                    SourceLineId = line.Id
                }
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
