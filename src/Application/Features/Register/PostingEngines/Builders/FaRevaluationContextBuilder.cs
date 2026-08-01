using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Constants;

namespace Application.Features.Register.PostingEngines;

public class FaRevaluationContextBuilder : IPostingContextBuilder<FaRevaluationDoc>
{
    private readonly IOrganizationAccountingPolicyResolver _accountingPolicyResolver;

    public FaRevaluationContextBuilder(IOrganizationAccountingPolicyResolver accountingPolicyResolver)
    {
        _accountingPolicyResolver = accountingPolicyResolver;
    }

    public async Task<List<PostingContext>> BuildAsync(FaRevaluationDoc document)
    {
        var accountingPolicyId = await _accountingPolicyResolver.ResolveAsync(document.OrganizationId);
        var result = new List<PostingContext>();

        foreach (var line in document.Lines)
        {
            var amount = Math.Abs(line.RevaluationAmount);
            if (amount == 0m)
                continue;

            result.Add(new PostingContext
            {
                OrganizationId = document.OrganizationId,
                DocumentTypeId = DocumentTypeIdConst.FAREVALUATION,
                AccountingPolicyId = accountingPolicyId,
                DocumentId = document.Id,
                DocDate = document.RevaluationDate,
                CurrencyId = CurrencyIdConst.UZS,
                JournalNumber = document.DocNumber,
                SourceLineId = line.Id,
                FixedAssetId = (int)line.FaAssetId,
                Entries = new List<PostingEntryContext>
                {
                    line.RevaluationAmount > 0m
                        ? new PostingEntryContext
                        {
                            DebitAccountId = line.AssetAccountId,
                            CreditAccountId = document.RevaluationReserveAccountId,
                            Amount = amount,
                            Content = "Fixed asset revaluation increase",
                            SourceLineId = line.Id
                        }
                        : new PostingEntryContext
                        {
                            DebitAccountId = document.RevaluationLossAccountId,
                            CreditAccountId = line.AssetAccountId,
                            Amount = amount,
                            Content = "Fixed asset revaluation decrease",
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
            });
        }

        return result;
    }
}
