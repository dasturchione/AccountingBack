using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Constants;

namespace Application.Features.Register.PostingEngines;

public class FaCommissioningContextBuilder :
    IPostingContextBuilder<FaCommissioningDoc>
{
    private readonly IOrganizationAccountingPolicyResolver _accountingPolicyResolver;

    public FaCommissioningContextBuilder(
        IOrganizationAccountingPolicyResolver accountingPolicyResolver)
    {
        _accountingPolicyResolver = accountingPolicyResolver;
    }

    public async Task<List<PostingContext>> BuildAsync(
        FaCommissioningDoc document)
    {
        var accountingPolicyId =
            await _accountingPolicyResolver.ResolveAsync(document.OrganizationId);

        return document.Lines.Select(line => new PostingContext
        {
            OrganizationId = document.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.FACOMMISSIONING,
            AccountingPolicyId = accountingPolicyId,
            DocumentId = document.Id,
            DocDate = document.DocDate,
            CurrencyId = CurrencyIdConst.UZS,
            JournalNumber = document.DocNumber,
            SourceLineId = line.Id,
            FixedAssetId = checked((int)line.FaAssetId),
            Entries =
            [
                new PostingEntryContext
                {
                    DebitAccountId =
                        line.FaAsset.FaAssetAccounting!.AssetAccountId,
                    CreditAccountId = line.CapitalInvestmentAccountId,
                    Amount = line.CapitalizedAmount,
                    Content = "Fixed asset commissioning",
                    SourceLineId = line.Id
                }
            ],
            Subkontos =
            [
                new SubkontoValue
                {
                    SubkontoTypeId = SubkontoTypeIdConst.FixedAssets,
                    DisplayValue = line.FaAsset.Name,
                    EntityId = line.FaAssetId,
                    SortOrder = 1
                }
            ]
        }).ToList();
    }
}
