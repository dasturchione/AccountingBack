using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Money;
using SharedKernel.Query;
using System.Text.Json;

namespace Application.Features.Register.PostingEngines
{
    public class SaleDocContextBuilder : IPostingContextBuilder<SaleDoc>
    {
        private readonly IQueryBuilder _queryBuilder;
        private readonly IQueryRepository<Product> _productQuery;
        private readonly IQueryRepository<CounterpartyCard> _counterpartyQuery;
        private readonly IOrganizationAccountingPolicyResolver _accountingPolicyResolver;

        public SaleDocContextBuilder(IQueryBuilder queryBuilder,
                                     IQueryRepository<Product> productQuery,
                                     IQueryRepository<CounterpartyCard> counterpartyQuery,
                                     IOrganizationAccountingPolicyResolver accountingPolicyResolver)
        {
            _queryBuilder = queryBuilder;
            _productQuery = productQuery;
            _counterpartyQuery = counterpartyQuery;
            _accountingPolicyResolver = accountingPolicyResolver;
        }

        public async Task<List<PostingContext>> BuildAsync(SaleDoc document)
        {
            var result = new List<PostingContext>();
            var productLines = document.SaleDocProducts?.ToList() ?? new List<SaleDocProduct>();
            var productMap = await GetProductMapAsync(productLines.Select(x => x.ProductId).Distinct().ToList());

            var counterpartyName = await GetCounterpartyNameAsync(document.CounterpartyId);
            var accountingPolicyId = await _accountingPolicyResolver.ResolveAsync(document.OrganizationId);

            result.AddRange(BuildLineContexts(document, productLines, productMap, counterpartyName, accountingPolicyId));

            return result;
        }

        private List<PostingContext> BuildLineContexts(
            SaleDoc document,
            List<SaleDocProduct> productLines,
            Dictionary<int, ProductPostingDto> productMap,
            string counterpartyName,
            short accountingPolicyId)
        {
            return productLines
                .GroupBy(x => new
                {
                    x.VatRateId,
                    document.CustomerAccountId,
                    document.VatAccountId,
                    x.InventoryAccountId,
                    x.IncomeAccountId,
                    x.CostAccountId,
                    ProductGroupId = productMap.TryGetValue(x.ProductId, out var groupProduct)
                        ? groupProduct.ProductGroupId
                        : null,
                    ProductGroupName = productMap.TryGetValue(x.ProductId, out var groupProductName)
                        ? groupProductName.ProductGroupName
                        : null
                })
                .Select(group =>
                {
                    var baseAmount = group.Sum(x => x.Amount);
                    var vatAmount = group.Sum(x => x.VatAmount);
                    var goodsLines = group
                        .Where(x => productMap.TryGetValue(x.ProductId, out var product) &&
                                    !product.IsService)
                        .ToList();
                    var costAmount = DocumentMoney.Round(goodsLines.Sum(x => x.CostPrice * x.Quantity));
                    var goodsQuantity = goodsLines.Sum(x => x.Quantity);
                    var hasGoods = goodsQuantity > 0m;

                    var context = new PostingContext
                    {
                        OrganizationId = document.OrganizationId,
                        DocumentTypeId = DocumentTypeIdConst.SALE,
                        AccountingPolicyId = accountingPolicyId,
                        DocumentId = document.Id,
                        DocDate = document.DocDate,
                        CurrencyId = document.CurrencyId,
                        JournalNumber = document.DocNumber,
                        Entries = BuildSaleEntries(
                            customerAccountId: group.Key.CustomerAccountId,
                            incomeAccountId: group.Key.IncomeAccountId,
                            vatAccountId: group.Key.VatAccountId,
                            costAccountId: group.Key.CostAccountId,
                            inventoryAccountId: group.Key.InventoryAccountId,
                            baseAmount: baseAmount,
                            vatAmount: vatAmount,
                            costAmount: costAmount,
                            costQuantity: hasGoods ? goodsQuantity : null,
                            contentPrefix: hasGoods ? "Sale goods" : "Sale service"),
                        Subkontos = BuildSaleDocumentSubkontos(document, counterpartyName, group.Key.VatRateId)
                    };

                    if (document.ContractId.HasValue)
                        AddContractSubkonto(context, document.ContractId.Value, 5);

                    AddProductGroupSubkonto(context, group.Key.ProductGroupId, group.Key.ProductGroupName, 6);

                    return context;
                })
                .ToList();
        }

        private static List<PostingEntryContext> BuildSaleEntries(
            int? customerAccountId,
            int? incomeAccountId,
            int? vatAccountId,
            int? costAccountId,
            int? inventoryAccountId,
            decimal baseAmount,
            decimal vatAmount,
            decimal costAmount,
            decimal? costQuantity,
            string contentPrefix)
        {
            var entries = new List<PostingEntryContext>();

            if (baseAmount != 0m)
            {
                entries.Add(new PostingEntryContext
                {
                    DebitAccountId = customerAccountId,
                    CreditAccountId = incomeAccountId,
                    Amount = baseAmount,
                    Content = contentPrefix
                });
            }

            if (vatAmount != 0m)
            {
                entries.Add(new PostingEntryContext
                {
                    DebitAccountId = customerAccountId,
                    CreditAccountId = vatAccountId,
                    Amount = vatAmount,
                    Content = $"{contentPrefix} VAT"
                });
            }

            if (costAmount != 0m)
            {
                entries.Add(new PostingEntryContext
                {
                    DebitAccountId = costAccountId,
                    CreditAccountId = inventoryAccountId,
                    Amount = costAmount,
                    CreditQuantity = costQuantity,
                    Content = $"{contentPrefix} cost"
                });
            }

            return entries;
        }

        private static List<SubkontoValue> BuildSaleDocumentSubkontos(SaleDoc document, string counterpartyName, short? vatRateId)
        {
            var subkontos = new List<SubkontoValue>
            {
                new()
                {
                    SubkontoTypeId = SubkontoTypeIdConst.Counterparties,
                    DisplayValue = counterpartyName,
                    EntityId = document.CounterpartyId,
                    SortOrder = 1
                },
                new()
                {
                    SubkontoTypeId = SubkontoTypeIdConst.SalesDocumentsTurnover,
                    DisplayValue = JsonSerializer.Serialize(new
                    {
                        number = document.DocNumber,
                        date = document.DocDate,
                        vatRateId
                    }),
                    EntityId = document.Id,
                    SortOrder = 2
                },
                new()
                {
                    SubkontoTypeId = SubkontoTypeIdConst.CounterpartySettlementDocuments,
                    DisplayValue = JsonSerializer.Serialize(new
                    {
                        number = document.DocNumber,
                        date = document.DocDate
                    }),
                    EntityId = document.Id,
                    SortOrder = 3
                }
            };

            if (vatRateId.HasValue)
            {
                subkontos.Add(new SubkontoValue
                {
                    SubkontoTypeId = SubkontoTypeIdConst.VatRates,
                    DisplayValue = vatRateId.Value.ToString(),
                    EntityId = vatRateId.Value,
                    SortOrder = 4
                });
            }

            return subkontos;
        }

        private async Task<Dictionary<int, ProductPostingDto>> GetProductMapAsync(List<int> productIds)
        {
            if (productIds.Count == 0)
                return new Dictionary<int, ProductPostingDto>();

            var query = _queryBuilder.For<Product>()
                .Where(x => productIds.Contains(x.Id))
                .As(x => new ProductPostingDto
                {
                    ProductId = x.Id,
                    IsService = x.IsService,
                    ProductGroupId = x.ProductGroupId,
                    ProductGroupName = x.ProductGroup != null ? x.ProductGroup.Name : null
                })
                .Build();

            var serviceProducts = await _productQuery.GetAllAsync(query);
            return serviceProducts.ToDictionary(x => x.ProductId);
        }

        private async Task<string> GetCounterpartyNameAsync(int counterpartyId)
        {
            var query = _queryBuilder.For<CounterpartyCard>().Where(x => x.Id == counterpartyId).Build();
            var entity = await _counterpartyQuery.GetAsync(query);
            return entity?.FullName ?? "";
        }

        private static void AddProductGroupSubkonto(PostingContext context, int? productGroupId, string? productGroupName, int sortOrder)
        {
            if (!productGroupId.HasValue)
                return;

            context.Subkontos.Add(new SubkontoValue
            {
                SubkontoTypeId = SubkontoTypeIdConst.ProductGroups,
                DisplayValue = productGroupName ?? productGroupId.Value.ToString(),
                EntityId = productGroupId.Value,
                SortOrder = sortOrder
            });
        }

        private static void AddContractSubkonto(PostingContext context, long contractId, int sortOrder)
        {
            context.Subkontos.Add(new SubkontoValue
            {
                SubkontoTypeId = SubkontoTypeIdConst.Contracts,
                DisplayValue = contractId.ToString(),
                EntityId = contractId,
                SortOrder = sortOrder
            });
        }

        private sealed class ProductPostingDto
        {
            public int ProductId { get; set; }
            public bool IsService { get; set; }
            public int? ProductGroupId { get; set; }
            public string? ProductGroupName { get; set; }
        }

    }
}
