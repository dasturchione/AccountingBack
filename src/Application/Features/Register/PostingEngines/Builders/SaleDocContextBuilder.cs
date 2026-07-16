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
            var productLineMap = productLines.ToDictionary(x => x.Id);

            var counterpartyName = await GetCounterpartyNameAsync(document.CounterpartyId);
            var productTableIds = saleTables.Select(x => x.ProductTableId).Distinct().ToList();
            var productTableMap = await GetProductTableMapAsync(productTableIds);
            var lastPurchases = await GetLastPurchasesAsync(document, productTableIds);
            var accountingPolicyId = await _accountingPolicyResolver.ResolveAsync(document.OrganizationId);

            result.AddRange(BuildCostContexts(document, saleTables, productLineMap, productTableMap, lastPurchases, accountingPolicyId));
            result.AddRange(BuildSaleContexts(document, saleTables, productLineMap, counterpartyName, accountingPolicyId));
            result.AddRange(BuildServiceContexts(document, serviceLines, serviceProductMap, counterpartyName, accountingPolicyId));

            return result;
        }

        private List<PostingContext> BuildCostContexts(
            SaleDoc document,
            List<SaleDocTable> saleTables,
            Dictionary<long, SaleDocProduct> productLineMap,
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

                    if (!productLineMap.TryGetValue(table.OwnerId, out var saleLine))
                        return null;

                    return new SalePurchaseSourceDto
                    {
                        ProductTableId = table.ProductTableId,
                        ProductId = productTable.ProductId,
                        ProductName = productTable.ProductName,
                        ProductGroupId = productTable.ProductGroupId,
                        ProductGroupName = productTable.ProductGroupName,
                        PurchaseId = lastPurchase.PurchaseId,
                        PurchaseDocNumber = lastPurchase.DocNumber,
                        PurchaseDate = lastPurchase.Date,
                        WarehouseId = lastPurchase.WarehouseId,
                        WarehouseName = lastPurchase.WarehouseName,
                        CostPrice = table.CostPrice > 0 ? table.CostPrice : lastPurchase.CostAmount,
                        InventoryAccountId = saleLine.InventoryAccountId,
                        CostAccountId = saleLine.CostAccountId
                    };
                })
                .Where(x => x is not null)
                .Select(x => x!)
                .ToList();

            return salePurchaseSources
                .GroupBy(x => new { x.ProductId, x.PurchaseId, x.InventoryAccountId, x.CostAccountId })
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
                    document.CustomerAccountId,
                    document.VatAccountId,
                    x.InventoryAccountId,
                    x.IncomeAccountId,
                    x.CostAccountId,
                    ProductGroupId = serviceProductMap.TryGetValue(x.ProductId, out var groupProduct)
                        ? groupProduct.ProductGroupId
                        : null,
                    ProductGroupName = serviceProductMap.TryGetValue(x.ProductId, out var groupProductName)
                        ? groupProductName.ProductGroupName
                        : null
                })
                .Select(group =>
                {
                    var baseAmount = group.Sum(x => x.Amount);
                    var vatAmount = group.Sum(x => x.VatAmount);
                    var costAmount = group.Sum(x => x.CostPrice);
                    var nonPieceTrackedQuantity = group
                        .Where(x => serviceProductMap.TryGetValue(x.ProductId, out var product) &&
                                    !product.IsService &&
                                    !product.IsPieceTracked)
                        .Sum(x => x.Quantity);
                    var hasNonPieceTrackedGoods = nonPieceTrackedQuantity > 0m;

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
                            costQuantity: hasNonPieceTrackedGoods ? nonPieceTrackedQuantity : null,
                            contentPrefix: hasNonPieceTrackedGoods ? "Sale goods" : "Sale service"),
                        Subkontos = BuildSaleDocumentSubkontos(document, counterpartyName, group.Key.VatRateId)
                    };

                    if (document.ContractId.HasValue)
                        AddContractSubkonto(context, document.ContractId.Value, 5);

                    AddProductGroupSubkonto(context, group.Key.ProductGroupId, group.Key.ProductGroupName, 6);

                    return context;
                })
                .ToList();
        }

        private PostingContext BuildCostContext(SaleDoc document, List<SalePurchaseSourceDto> group, short accountingPolicyId)
        {
            var first = group.First();
            var costAmount = group.Sum(x => x.CostPrice);

            var context = new PostingContext
            {
                OrganizationId = document.OrganizationId,
                DocumentTypeId = DocumentTypeIdConst.SALE,
                AccountingPolicyId = accountingPolicyId,
                DocumentId = document.Id,
                DocDate = document.DocDate,
                CurrencyId = document.CurrencyId,
                JournalNumber = document.DocNumber,
                Entries = new List<PostingEntryContext>
                {
                    new()
                    {
                        DebitAccountId = first.CostAccountId,
                        CreditAccountId = first.InventoryAccountId,
                        Amount = costAmount,
                        CreditQuantity = group.Count,
                        Content = "Sale cost"
                    }
                },
                Subkontos = new List<SubkontoValue>
                {
                    new()
                    {
                        SubkontoTypeId = SubkontoTypeIdConst.InventoryItems,
                        DisplayValue = first.ProductName,
                        EntityId = first.ProductId,
                        SortOrder = 1
                    },
                    new()
                    {
                        SubkontoTypeId = SubkontoTypeIdConst.Warehouses,
                        DisplayValue = first.WarehouseName,
                        EntityId = first.WarehouseId,
                        SortOrder = 2
                    },
                    new()
                    {
                        SubkontoTypeId = SubkontoTypeIdConst.Batches,
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
                        SubkontoTypeId = SubkontoTypeIdConst.SalesDocumentsTurnover,
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

            AddProductGroupSubkonto(context, first.ProductGroupId, first.ProductGroupName, 2);

            return context;
        }

        private List<PostingContext> BuildSaleContexts(
            SaleDoc document,
            List<SaleDocTable> saleTables,
            Dictionary<long, SaleDocProduct> productLineMap,
            string counterpartyName,
            short accountingPolicyId)
        {
            return saleTables
                .Select(table =>
                {
                    if (!productLineMap.TryGetValue(table.OwnerId, out var saleLine))
                        return null;

                    return new SaleRevenueSourceDto
                    {
                        VatRateId = table.VatRateId,
                        Amount = table.Amount,
                        VatAmount = table.VatAmount,
                        IncomeAccountId = saleLine.IncomeAccountId
                    };
                })
                .Where(x => x is not null)
                .Select(x => x!)
                .GroupBy(x => new { x.VatRateId, document.CustomerAccountId, document.VatAccountId, x.IncomeAccountId })
                .Select(group =>
                {
                    var baseAmount = group.Sum(x => x.Amount);
                    var vatAmount = group.Sum(x => x.VatAmount);

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
                            costAccountId: null,
                            inventoryAccountId: null,
                            baseAmount: baseAmount,
                            vatAmount: vatAmount,
                            costAmount: 0m,
                            costQuantity: null,
                            contentPrefix: "Sale goods"),
                        Subkontos = BuildSaleDocumentSubkontos(document, counterpartyName, group.Key.VatRateId)
                    };

                    if (document.ContractId.HasValue)
                        AddContractSubkonto(context, document.ContractId.Value, 5);

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
                    ProductGroupId = x.Product.ProductGroupId,
                    ProductGroupName = x.Product.ProductGroup != null ? x.Product.ProductGroup.Name : null
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
                .Where(x => productIds.Contains(x.Id) && (x.IsService || !x.IsPieceTracked))
                .As(x => new ServiceProductTempDto
                {
                    ProductId = x.Id,
                    IsService = x.IsService,
                    IsPieceTracked = x.IsPieceTracked,
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
                    g => g.OrderByDescending(x => x.Date)
                          .ThenByDescending(x => x.PurchaseId)
                          .First());
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

        private sealed class ProductTableTempDto
        {
            public int TableId { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; } = null!;
            public int? ProductGroupId { get; set; }
            public string? ProductGroupName { get; set; }
        }

        private sealed class ServiceProductTempDto
        {
            public int ProductId { get; set; }
            public bool IsService { get; set; }
            public bool IsPieceTracked { get; set; }
            public int? ProductGroupId { get; set; }
            public string? ProductGroupName { get; set; }
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
            public int? ProductGroupId { get; set; }
            public string? ProductGroupName { get; set; }
            public long PurchaseId { get; set; }
            public string PurchaseDocNumber { get; set; } = null!;
            public DateTime PurchaseDate { get; set; }
            public int WarehouseId { get; set; }
            public string WarehouseName { get; set; } = null!;
            public decimal CostPrice { get; set; }
            public int? InventoryAccountId { get; set; }
            public int? CostAccountId { get; set; }
        }

        private sealed class SaleRevenueSourceDto
        {
            public short? VatRateId { get; set; }
            public decimal Amount { get; set; }
            public decimal VatAmount { get; set; }
            public int? IncomeAccountId { get; set; }
        }
    }
}
