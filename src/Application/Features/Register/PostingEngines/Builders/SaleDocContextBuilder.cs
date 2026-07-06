using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using System.Text.Json;

namespace Application.Features.Register.PostingEngines
{
    public class SaleDocContextBuilder : IPostingContextBuilder<SaleDoc>
    {
        private readonly IQueryBuilder _queryBuilder;
        private readonly IQueryRepository<ProductTable> _productTableQuery;
        private readonly IQueryRepository<Product> _productQuery;
        private readonly IQueryRepository<CounterpartyCard> _counterpartyQuery;
        private readonly IQueryRepository<PurchaseDocTable> _purchaseDocTableQuery;
        private readonly IOrganizationAccountingPolicyResolver _accountingPolicyResolver;

        public SaleDocContextBuilder(IQueryBuilder queryBuilder,
                                     IQueryRepository<ProductTable> productTableQuery,
                                     IQueryRepository<Product> productQuery,
                                     IQueryRepository<CounterpartyCard> counterpartyQuery,
                                     IQueryRepository<PurchaseDocTable> purchaseDocTableQuery,
                                     IOrganizationAccountingPolicyResolver accountingPolicyResolver)
        {
            _queryBuilder = queryBuilder;
            _productTableQuery = productTableQuery;
            _productQuery = productQuery;
            _counterpartyQuery = counterpartyQuery;
            _purchaseDocTableQuery = purchaseDocTableQuery;
            _accountingPolicyResolver = accountingPolicyResolver;
        }

        public async Task<List<PostingContext>> BuildAsync(SaleDoc document)
        {
            var result = new List<PostingContext>();
            var productLines = document.SaleDocProducts?.ToList() ?? new List<SaleDocProduct>();
            var serviceProductMap = await GetServiceProductMapAsync(productLines.Select(x => x.ProductId).Distinct().ToList());
            var serviceProductIds = serviceProductMap.Keys.ToHashSet();
            var saleTables = productLines
                .Where(x => !serviceProductIds.Contains(x.ProductId))
                .SelectMany(x => x.SaleDocTables)
                .ToList() ?? new List<SaleDocTable>();
            var serviceLines = productLines
                .Where(x => serviceProductIds.Contains(x.ProductId))
                .ToList();

            var counterpartyName = await GetCounterpartyNameAsync(document.CounterpartyId);
            var productTableIds = saleTables.Select(x => x.ProductTableId).Distinct().ToList();
            var productTableMap = await GetProductTableMapAsync(productTableIds);
            var lastPurchases = await GetLastPurchasesAsync(document, productTableIds);
            var accountingPolicyId = await _accountingPolicyResolver.ResolveAsync(document.OrganizationId);

            result.AddRange(BuildCostContexts(document, saleTables, productTableMap, lastPurchases, accountingPolicyId));
            result.AddRange(BuildSaleContexts(document, saleTables, counterpartyName, accountingPolicyId));
            result.AddRange(BuildServiceContexts(document, serviceLines, serviceProductMap, counterpartyName, accountingPolicyId));

            return result;
        }

        private List<PostingContext> BuildCostContexts(
            SaleDoc document,
            List<SaleDocTable> saleTables,
            Dictionary<int, ProductTableTempDto> productTableMap,
            Dictionary<int, PurchaseBatchDto> lastPurchases,
            short accountingPolicyId)
        {
            var salePurchaseSources = saleTables
                .Select(table =>
                {
                    if (!productTableMap.TryGetValue(table.ProductTableId, out var productTable))
                        return null;

                    if (!lastPurchases.TryGetValue(table.ProductTableId, out var lastPurchase))
                        return null;

                    return new SalePurchaseSourceDto
                    {
                        ProductTableId = table.ProductTableId,
                        ProductId = productTable.ProductId,
                        ProductName = productTable.ProductName,
                        ProductCategory = ProductTypeDimensionValueResolver.Resolve(productTable.ProductTypeId),
                        PurchaseId = lastPurchase.PurchaseId,
                        PurchaseDocNumber = lastPurchase.DocNumber,
                        PurchaseDate = lastPurchase.Date,
                        WarehouseId = lastPurchase.WarehouseId,
                        WarehouseName = lastPurchase.WarehouseName,
                        CostPrice = table.CostPrice > 0 ? table.CostPrice : lastPurchase.CostAmount
                    };
                })
                .Where(x => x is not null)
                .Select(x => x!)
                .ToList();

            return salePurchaseSources
                .GroupBy(x => new { x.ProductId, x.PurchaseId })
                .Select(group => BuildCostContext(document, group.ToList(), accountingPolicyId))
                .ToList();
        }

        private List<PostingContext> BuildServiceContexts(
            SaleDoc document,
            List<SaleDocProduct> serviceLines,
            Dictionary<int, ServiceProductTempDto> serviceProductMap,
            string counterpartyName,
            short accountingPolicyId)
        {
            return serviceLines
                .GroupBy(x => new
                {
                    x.VatRateId,
                    ServiceType = serviceProductMap.TryGetValue(x.ProductId, out var product)
                        ? ProductTypeDimensionValueResolver.Resolve(product.ProductTypeId)
                        : RegisterDefaultsConst.DefaultDimensionValue
                })
                .Select(group =>
                {
                    var baseAmount = group.Sum(x => x.Amount);
                    var vatAmount = group.Sum(x => x.VatAmount);
                    var costAmount = group.Sum(x => x.CostPrice);
                    var amounts = new Dictionary<string, decimal>
                    {
                        [AmountSourceConst.Base] = baseAmount,
                        [AmountSourceConst.VAT] = vatAmount
                    };

                    if (costAmount > 0m)
                        amounts[AmountSourceConst.Cost] = costAmount;

                    return new PostingContext
                    {
                        OrganizationId = document.OrganizationId,
                        DocumentTypeId = DocumentTypeIdConst.SALE,
                        AccountingPolicyId = accountingPolicyId,
                        RuleId = PostingRuleIdConst.SALE_SERVICE,
                        DocumentId = document.Id,
                        DocDate = document.DocDate,
                        CurrencyId = document.CurrencyId,
                        JournalNumber = document.DocNumber,
                        ServiceType = group.Key.ServiceType,
                        Amounts = amounts,
                        Subkontos = new List<SubkontoValue>
                        {
                            new()
                            {
                                SubkontoTypeId = SubkontoTypeIdConst.COUNTER_PARTY,
                                DisplayValue = counterpartyName,
                                EntityId = document.CounterpartyId,
                                SortOrder = 1
                            },
                            new()
                            {
                                SubkontoTypeId = SubkontoTypeIdConst.SALE,
                                DisplayValue = JsonSerializer.Serialize(new
                                {
                                    number = document.DocNumber,
                                    date = document.DocDate,
                                    vatRateId = group.Key.VatRateId
                                }),
                                EntityId = document.Id,
                                SortOrder = 2
                            }
                        }
                    };
                })
                .ToList();
        }

        private PostingContext BuildCostContext(SaleDoc document, List<SalePurchaseSourceDto> group, short accountingPolicyId)
        {
            var first = group.First();
            var costAmount = group.Sum(x => x.CostPrice);

            return new PostingContext
            {
                OrganizationId = document.OrganizationId,
                DocumentTypeId = DocumentTypeIdConst.SALE,
                AccountingPolicyId = accountingPolicyId,
                RuleId = PostingRuleIdConst.SALE_GOODS,
                DocumentId = document.Id,
                DocDate = document.DocDate,
                CurrencyId = document.CurrencyId,
                JournalNumber = document.DocNumber,
                ProductCategory = first.ProductCategory,
                CreditQuantity = group.Count,
                Amounts = new Dictionary<string, decimal>
                {
                    [AmountSourceConst.Cost] = costAmount
                },
                SkippedAmountSources = new[] { AmountSourceConst.Base, AmountSourceConst.VAT },
                Subkontos = new List<SubkontoValue>
                {
                    new()
                    {
                        SubkontoTypeId = SubkontoTypeIdConst.PRODUCT,
                        DisplayValue = first.ProductName,
                        EntityId = first.ProductId,
                        SortOrder = 1
                    },
                    new()
                    {
                        SubkontoTypeId = SubkontoTypeIdConst.WAREHOUSE,
                        DisplayValue = first.WarehouseName,
                        EntityId = first.WarehouseId,
                        SortOrder = 2
                    },
                    new()
                    {
                        SubkontoTypeId = SubkontoTypeIdConst.PURCHASE,
                        DisplayValue = JsonSerializer.Serialize(new
                        {
                            number = first.PurchaseDocNumber,
                            date = first.PurchaseDate
                        }),
                        EntityId = first.PurchaseId,
                        SortOrder = 3
                    },
                    new()
                    {
                        SubkontoTypeId = SubkontoTypeIdConst.SALE,
                        DisplayValue = JsonSerializer.Serialize(new
                        {
                            number = document.DocNumber,
                            date = document.DocDate
                        }),
                        EntityId = document.Id,
                        SortOrder = 4
                    }
                }
            };
        }

        private List<PostingContext> BuildSaleContexts(
            SaleDoc document,
            List<SaleDocTable> saleTables,
            string counterpartyName,
            short accountingPolicyId)
        {
            return saleTables
                .GroupBy(x => x.VatRateId)
                .Select(group =>
                {
                    var baseAmount = group.Sum(x => x.Amount);
                    var vatAmount = group.Sum(x => x.VatAmount);

                    return new PostingContext
                    {
                        OrganizationId = document.OrganizationId,
                        DocumentTypeId = DocumentTypeIdConst.SALE,
                        AccountingPolicyId = accountingPolicyId,
                        RuleId = PostingRuleIdConst.SALE_GOODS,
                        DocumentId = document.Id,
                        DocDate = document.DocDate,
                        CurrencyId = document.CurrencyId,
                        JournalNumber = document.DocNumber,
                        Amounts = new Dictionary<string, decimal>
                        {
                            [AmountSourceConst.Base] = baseAmount,
                            [AmountSourceConst.VAT] = vatAmount
                        },
                        SkippedAmountSources = new[] { AmountSourceConst.Cost },
                        Subkontos = new List<SubkontoValue>
                        {
                            new()
                            {
                                SubkontoTypeId = SubkontoTypeIdConst.COUNTER_PARTY,
                                DisplayValue = counterpartyName,
                                EntityId = document.CounterpartyId,
                                SortOrder = 1
                            },
                            new()
                            {
                                SubkontoTypeId = SubkontoTypeIdConst.SALE,
                                DisplayValue = JsonSerializer.Serialize(new
                                {
                                    number = document.DocNumber,
                                    date = document.DocDate,
                                    vatRateId = group.Key
                                }),
                                EntityId = document.Id,
                                SortOrder = 2
                            }
                        }
                    };
                })
                .ToList();
        }

        private async Task<Dictionary<int, ProductTableTempDto>> GetProductTableMapAsync(List<int> productTableIds)
        {
            if (productTableIds.Count == 0)
                return new Dictionary<int, ProductTableTempDto>();

            var productTableQuery = _queryBuilder.For<ProductTable>()
                .Where(x => productTableIds.Contains(x.Id))
                .As(x => new ProductTableTempDto
                {
                    TableId = x.Id,
                    ProductId = x.ProductId,
                    ProductName = x.Product.Name,
                    ProductTypeId = x.Product.ProductTypeId
                })
                .Build();

            var productTables = await _productTableQuery.GetAllAsync(productTableQuery);
            return productTables.ToDictionary(x => x.TableId, x => x);
        }

        private async Task<Dictionary<int, ServiceProductTempDto>> GetServiceProductMapAsync(List<int> productIds)
        {
            if (productIds.Count == 0)
                return new Dictionary<int, ServiceProductTempDto>();

            var query = _queryBuilder.For<Product>()
                .Where(x => productIds.Contains(x.Id) && x.IsService)
                .As(x => new ServiceProductTempDto
                {
                    ProductId = x.Id,
                    ProductTypeId = x.ProductTypeId
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

        private async Task<Dictionary<int, PurchaseBatchDto>> GetLastPurchasesAsync(SaleDoc document, List<int> productTableIds)
        {
            if (productTableIds.Count == 0)
                return new Dictionary<int, PurchaseBatchDto>();

            var query = _queryBuilder.For<PurchaseDocTable>()
                .Where(x => productTableIds.Contains(x.ProductTableId)
                            && x.Owner.Owner.OrganizationId == document.OrganizationId
                            && x.Owner.Owner.StatusId == DocumentStatusIdConst.POSTED
                            && x.Owner.Owner.StateId == StateIdConst.ACTIVE
                            && x.Owner.Owner.DocDate <= document.DocDate)
                .As(x => new PurchaseBatchDto
                {
                    ProductTableId = x.ProductTableId,
                    PurchaseId = x.Owner.OwnerId,
                    Date = x.Owner.Owner.DocDate,
                    DocNumber = x.Owner.Owner.DocNumber,
                    WarehouseId = x.Owner.Owner.WarehouseId,
                    WarehouseName = x.Owner.Owner.Warehouse.Name,
                    CostAmount = x.TotalAmount
                })
                .Build();

            var candidates = await _purchaseDocTableQuery.GetAllAsync(query);

            return candidates
                .GroupBy(x => x.ProductTableId)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        return g.OrderByDescending(x => x.Date)
                                .ThenByDescending(x => x.PurchaseId)
                                .First();
                    });
        }

        private sealed class ProductTableTempDto
        {
            public int TableId { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; } = null!;
            public short ProductTypeId { get; set; }
        }

        private sealed class ServiceProductTempDto
        {
            public int ProductId { get; set; }
            public short ProductTypeId { get; set; }
        }

        private sealed class PurchaseBatchDto
        {
            public int ProductTableId { get; set; }
            public long PurchaseId { get; set; }
            public DateTime Date { get; set; }
            public string DocNumber { get; set; } = null!;
            public int WarehouseId { get; set; }
            public string WarehouseName { get; set; } = null!;
            public decimal CostAmount { get; set; }
        }

        private sealed class SalePurchaseSourceDto
        {
            public int ProductTableId { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; } = null!;
            public string ProductCategory { get; set; } = RegisterDefaultsConst.DefaultDimensionValue;
            public long PurchaseId { get; set; }
            public string PurchaseDocNumber { get; set; } = null!;
            public DateTime PurchaseDate { get; set; }
            public int WarehouseId { get; set; }
            public string WarehouseName { get; set; } = null!;
            public decimal CostPrice { get; set; }
        }
    }
}
