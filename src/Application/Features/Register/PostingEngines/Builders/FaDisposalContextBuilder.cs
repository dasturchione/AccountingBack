using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Constants;

namespace Application.Features.Register.PostingEngines;

public class FaDisposalContextBuilder : IPostingContextBuilder<FaDisposalDoc>
{
    private readonly IOrganizationAccountingPolicyResolver _accountingPolicyResolver;

    public FaDisposalContextBuilder(IOrganizationAccountingPolicyResolver accountingPolicyResolver)
    {
        _accountingPolicyResolver = accountingPolicyResolver;
    }

    public async Task<List<PostingContext>> BuildAsync(FaDisposalDoc document)
    {
        var accountingPolicyId = await _accountingPolicyResolver.ResolveAsync(document.OrganizationId);
        var result = new List<PostingContext>();

        foreach (var line in document.Lines)
        {
            var baseContext = new PostingContext
            {
                OrganizationId = document.OrganizationId,
                DocumentTypeId = DocumentTypeIdConst.FADISPOSAL,
                AccountingPolicyId = accountingPolicyId,
                DocumentId = document.Id,
                DocDate = document.DisposalDate,
                CurrencyId = CurrencyIdConst.UZS,
                JournalNumber = document.DocNumber,
                SourceLineId = line.Id,
                FixedAssetId = (int)line.FaAssetId,
                Entries = BuildEntries(document, line),
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
            };

            result.Add(baseContext);
        }

        return result;
    }

    private static List<PostingEntryContext> BuildEntries(FaDisposalDoc document, FaDisposalDocLine line)
    {
        var entries = new List<PostingEntryContext>();
        var accumulatedDepreciation = Math.Max(0m, line.FaAsset.InitialCost - line.BookValue);

        if (accumulatedDepreciation != 0m)
        {
            entries.Add(new PostingEntryContext
            {
                DebitAccountId = line.AccumulatedDepreciationAccountId,
                CreditAccountId = line.AssetAccountId,
                Amount = accumulatedDepreciation,
                Content = "Fixed asset accumulated depreciation disposal",
                SourceLineId = line.Id
            });
        }

        if (line.BookValue != 0m)
        {
            entries.Add(new PostingEntryContext
            {
                DebitAccountId = document.DisposalAccountId,
                CreditAccountId = line.AssetAccountId,
                Amount = line.BookValue,
                Content = "Fixed asset disposal",
                SourceLineId = line.Id
            });
        }

        if (line.SaleAmount != 0m)
        {
            entries.Add(new PostingEntryContext
            {
                DebitAccountId = document.CustomerAccountId,
                CreditAccountId = document.DisposalAccountId,
                Amount = line.SaleAmount,
                Content = "Fixed asset disposal sale",
                SourceLineId = line.Id
            });
        }

        if (line.GainLoss > 0m)
        {
            entries.Add(new PostingEntryContext
            {
                DebitAccountId = document.DisposalAccountId,
                CreditAccountId = document.GainAccountId,
                Amount = line.GainLoss,
                Content = "Fixed asset disposal gain",
                SourceLineId = line.Id
            });
        }
        else if (line.GainLoss < 0m)
        {
            entries.Add(new PostingEntryContext
            {
                DebitAccountId = document.LossAccountId,
                CreditAccountId = document.DisposalAccountId,
                Amount = Math.Abs(line.GainLoss),
                Content = "Fixed asset disposal loss",
                SourceLineId = line.Id
            });
        }

        return entries;
    }
}
