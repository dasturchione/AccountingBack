using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;

namespace Application.Features.Register.PostingEngines;

public class FaReceiptContextBuilder : IPostingContextBuilder<FaReceiptDoc>
{
    private readonly IOrganizationAccountingPolicyResolver _accountingPolicyResolver;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<CounterpartyCard> _counterpartyCardQuery;

    public FaReceiptContextBuilder(
        IOrganizationAccountingPolicyResolver accountingPolicyResolver,
        IQueryBuilder queryBuilder,
        IQueryRepository<CounterpartyCard> counterpartyCardQuery)
    {
        _accountingPolicyResolver = accountingPolicyResolver;
        _queryBuilder = queryBuilder;
        _counterpartyCardQuery = counterpartyCardQuery;
    }

    public async Task<List<PostingContext>> BuildAsync(FaReceiptDoc document)
    {
        var result = new List<PostingContext>();
        var accountingPolicyId = await _accountingPolicyResolver.ResolveAsync(document.OrganizationId);
        var counterpartyName = document.CounterpartyId.HasValue
            ? await GetCounterpartyNameAsync(document.CounterpartyId.Value)
            : string.Empty;

        foreach (var line in document.Lines)
        {
            foreach (var asset in line.Assets)
            {
                var context = new PostingContext
                {
                    OrganizationId = document.OrganizationId,
                    DocumentTypeId = DocumentTypeIdConst.FARECEIPT,
                    AccountingPolicyId = accountingPolicyId,
                    DocumentId = document.Id,
                    DocDate = document.DocDate,
                    CurrencyId = document.CurrencyId,
                    JournalNumber = document.DocNumber,
                    SourceLineId = line.Id,
                    FixedAssetId = asset.FaAssetId.HasValue
                        ? checked((int)asset.FaAssetId.Value)
                        : null,
                    Entries =
                    [
                        new PostingEntryContext
                        {
                            DebitAccountId = line.CapitalInvestmentAccountId,
                            CreditAccountId = document.SupplierAccountId,
                            Amount = asset.InitialCost,
                            Content = "Fixed asset receipt",
                            SourceLineId = line.Id
                        }
                    ],
                    Subkontos =
                    [
                        new SubkontoValue
                        {
                            SubkontoTypeId = SubkontoTypeIdConst.FixedAssets,
                            DisplayValue = asset.Name,
                            EntityId = asset.FaAssetId,
                            SortOrder = 1
                        }
                    ]
                };

                AddCounterpartySubkonto(
                    context,
                    document.CounterpartyId,
                    counterpartyName,
                    2);
                result.Add(context);
            }

            if (line.VatAmount <= 0m)
                continue;

            var vatContext = new PostingContext
            {
                OrganizationId = document.OrganizationId,
                DocumentTypeId = DocumentTypeIdConst.FARECEIPT,
                AccountingPolicyId = accountingPolicyId,
                DocumentId = document.Id,
                DocDate = document.DocDate,
                CurrencyId = document.CurrencyId,
                JournalNumber = document.DocNumber,
                SourceLineId = line.Id,
                Entries =
                [
                    new PostingEntryContext
                    {
                        DebitAccountId = line.VatAccountId,
                        CreditAccountId = document.SupplierAccountId,
                        Amount = line.VatAmount,
                        Content = "Fixed asset receipt VAT",
                        SourceLineId = line.Id
                    }
                ]
            };

            AddCounterpartySubkonto(
                vatContext,
                document.CounterpartyId,
                counterpartyName,
                1);
            result.Add(vatContext);
        }

        return result;
    }

    private static void AddCounterpartySubkonto(
        PostingContext context,
        int? counterpartyId,
        string counterpartyName,
        int sortOrder)
    {
        if (!counterpartyId.HasValue)
            return;

        context.Subkontos.Add(new SubkontoValue
        {
            SubkontoTypeId = SubkontoTypeIdConst.Counterparties,
            DisplayValue = counterpartyName,
            EntityId = counterpartyId,
            SortOrder = sortOrder
        });
    }

    private async Task<string> GetCounterpartyNameAsync(int counterpartyId)
    {
        var query = _queryBuilder.For<CounterpartyCard>()
            .Where(counterparty => counterparty.Id == counterpartyId)
            .As(counterparty => counterparty.FullName)
            .Build();
        return await _counterpartyCardQuery.GetAsync(query) ?? string.Empty;
    }
}
