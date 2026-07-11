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
}

