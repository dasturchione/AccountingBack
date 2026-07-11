using Application.Abstractions;
using Application.Features.Register;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;

namespace Application.Features.Register.PostingEngines
{
    /// <summary>
    /// Строит контекст проводок для документа "Поступление основных средств" (fa_receipt_doc)
    /// по схеме 1С (правило FA_RECEIPT):
    ///   Dr 0800 → Cr 6010  (капитализация, сумма без НДС)
    ///   Dr 4410.1 → Cr 6010 (входной НДС при приобретении ОС)
    ///   Dr 0100 → Cr 0800  (ввод в эксплуатацию)
    ///
    /// Стоимость (Base) считается по каждому активу отдельно, чтобы прикрепить субконто
    /// fixed_asset к счетам ОС/капвложений; НДС — по строке документа.
    /// </summary>
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
                // Капитализация + ввод в эксплуатацию — по каждому активу строки отдельно.
                foreach (var asset in line.Assets)
                {
                    var baseContext = new PostingContext
                    {
                        OrganizationId = document.OrganizationId,
                        DocumentTypeId = DocumentTypeIdConst.FARECEIPT,
                        AccountingPolicyId = accountingPolicyId,
                        DocumentId = document.Id,
                        DocDate = document.DocDate,
                        CurrencyId = document.CurrencyId,
                        JournalNumber = document.DocNumber,
                        SourceLineId = line.Id,
                        FixedAssetId = asset.FaAssetId is { } faAssetId ? (int)faAssetId : null,
                        Subkontos = new List<SubkontoValue>
                        {
                            new()
                            {
                                SubkontoTypeId = SubkontoTypeIdConst.FixedAssets,
                                DisplayValue = asset.Name,
                                EntityId = asset.FaAssetId,
                                SortOrder = 1,
                            }
                        }
                    };

                    if (document.CounterpartyId.HasValue)
                    {
                        baseContext.Subkontos.Add(new SubkontoValue
                        {
                            SubkontoTypeId = SubkontoTypeIdConst.Counterparties,
                            DisplayValue = counterpartyName,
                            EntityId = document.CounterpartyId,
                            SortOrder = 2,
                        });
                    }

                    result.Add(baseContext);
                }

                // Входной НДС — по строке (одна проводка Dr 4410.1 → Cr 6010).
                if (line.VatAmount > 0m)
                {
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
                        Subkontos = new List<SubkontoValue>()
                    };

                    if (document.CounterpartyId.HasValue)
                    {
                        vatContext.Subkontos.Add(new SubkontoValue
                        {
                            SubkontoTypeId = SubkontoTypeIdConst.Counterparties,
                            DisplayValue = counterpartyName,
                            EntityId = document.CounterpartyId,
                            SortOrder = 1,
                        });
                    }

                    result.Add(vatContext);
                }
            }

            return result;
        }

        private async Task<string> GetCounterpartyNameAsync(int counterpartyId)
        {
            var query = _queryBuilder.For<CounterpartyCard>().Where(x => x.Id == counterpartyId).Build();
            var entity = await _counterpartyCardQuery.GetAsync(query);
            return entity?.FullName ?? string.Empty;
        }
    }
}

