using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using System.Text.Json;

namespace Application.Features.Register.PostingEngines
{
    public class PurchaseDocContextBuilder : IPostingContextBuilder<PurchaseDoc>
    {
        private readonly IOrganizationAccountingPolicyResolver _accountingPolicyResolver;
        private readonly IQueryBuilder _queryBuilder;
        private readonly IQueryRepository<Product> _productQuery;
        private readonly IQueryRepository<Contract> _contractQuery;
        private readonly IQueryRepository<Warehouse> _warehouseQuery;
        private readonly IQueryRepository<CounterpartyCard> _counterpartyCardQuery;

        public PurchaseDocContextBuilder(
            IOrganizationAccountingPolicyResolver accountingPolicyResolver,
            IQueryBuilder queryBuilder,
            IQueryRepository<Product> productQuery,
            IQueryRepository<Contract> contractQuery,
            IQueryRepository<Warehouse> warehouseQuery,
            IQueryRepository<CounterpartyCard> counterpartyCardQuery)
        {
            _accountingPolicyResolver = accountingPolicyResolver;
            _queryBuilder = queryBuilder;
            _productQuery = productQuery;
            _contractQuery = contractQuery;
            _warehouseQuery = warehouseQuery;
            _counterpartyCardQuery = counterpartyCardQuery;
        }

        public async Task<List<PostingContext>> BuildAsync(PurchaseDoc document)
        {
            var result = new List<PostingContext>();

            var productDatas = await GetProductsAsync(document);
            var wareHouseName = await GetWarehouseNameAsync(document.WarehouseId);
            var counterpartyName = await GetCounterpartyNameAsync(document.CounterpartyId);
            var contractData = await GetContractDataAsync(document.ContractId);
            var accountingPolicyId = await _accountingPolicyResolver.ResolveAsync(document.OrganizationId);

            foreach (var productData in productDatas.Where(x => !x.IsService))
            {
                var context = new PostingContext
                {
                    OrganizationId = document.OrganizationId,
                    DocumentTypeId = DocumentTypeIdConst.PURCHASE,
                    AccountingPolicyId = accountingPolicyId,
                    DocumentId = document.Id,
                    DocDate = document.DocDate,
                    CurrencyId = document.CurrencyId,
                    JournalNumber = document.DocNumber,
                    Entries = BuildPurchaseEntries(
                        debitAccountId: productData.DebitAccountId,
                        vatAccountId: productData.VatAccountId,
                        supplierAccountId: document.SupplierAccountId,
                        amount: productData.Amount,
                        vatAmount: productData.VatAmount,
                        quantity: productData.Quantity,
                        sourceLineId: null,
                        content: "Purchase goods"),
                    Subkontos = BuildBaseSubkontos(document, productData.ProductName, productData.ProductId, wareHouseName, counterpartyName)
                };

                AddProductGroupSubkonto(context, productData.ProductGroupId, productData.ProductGroupName, 2);
                AddVatAccountingMethodSubkonto(context, RegisterDefaultsConst.VatKindGoods, 8);
                AddVatRateSubkonto(context, productData.VatRateId, 6);
                AddContractSubkonto(context, document.ContractId, contractData, 7);

                result.Add(context);
            }

            foreach (var service in productDatas.Where(x => x.IsService))
            {
                var context = new PostingContext
                {
                    OrganizationId = document.OrganizationId,
                    DocumentTypeId = DocumentTypeIdConst.PURCHASE,
                    AccountingPolicyId = accountingPolicyId,
                    DocumentId = document.Id,
                    DocDate = document.DocDate,
                    CurrencyId = document.CurrencyId,
                    JournalNumber = document.DocNumber,
                    Entries = BuildPurchaseEntries(
                        debitAccountId: service.DebitAccountId,
                        vatAccountId: service.VatAccountId,
                        supplierAccountId: document.SupplierAccountId,
                        amount: service.Amount,
                        vatAmount: service.VatAmount,
                        quantity: null,
                        sourceLineId: null,
                        content: "Purchase services"),
                    Subkontos = BuildServiceSubkontos(document, counterpartyName)
                };

                AddProductGroupSubkonto(context, service.ProductGroupId, service.ProductGroupName, 4);
                AddVatAccountingMethodSubkonto(context, RegisterDefaultsConst.VatKindServices, 6);
                AddVatRateSubkonto(context, service.VatRateId, 4);
                AddContractSubkonto(context, document.ContractId, contractData, 5);

                result.Add(context);
            }

            return result;
        }

        private static List<PostingEntryContext> BuildPurchaseEntries(
            int? debitAccountId,
            int? vatAccountId,
            int? supplierAccountId,
            decimal amount,
            decimal vatAmount,
            decimal? quantity,
            long? sourceLineId,
            string content)
        {
            var entries = new List<PostingEntryContext>();

            if (amount != 0m)
            {
                entries.Add(new PostingEntryContext
                {
                    DebitAccountId = debitAccountId,
                    CreditAccountId = supplierAccountId,
                    Amount = amount,
                    DebitQuantity = quantity,
                    SourceLineId = sourceLineId,
                    Content = content
                });
            }

            if (vatAmount != 0m)
            {
                entries.Add(new PostingEntryContext
                {
                    DebitAccountId = vatAccountId,
                    CreditAccountId = supplierAccountId,
                    Amount = vatAmount,
                    SourceLineId = sourceLineId,
                    Content = $"{content} VAT"
                });
            }

            return entries;
        }

        private static List<SubkontoValue> BuildBaseSubkontos(
            PurchaseDoc document,
            string productName,
            int productId,
            string warehouseName,
            string counterpartyName) =>
        [
            new()
            {
                SubkontoTypeId = SubkontoTypeIdConst.InventoryItems,
                DisplayValue = productName,
                EntityId = productId,
                SortOrder = 1,
            },
            new()
            {
                SubkontoTypeId = SubkontoTypeIdConst.Warehouses,
                DisplayValue = warehouseName,
                EntityId = document.WarehouseId,
                SortOrder = 2,
            },
            new()
            {
                SubkontoTypeId = SubkontoTypeIdConst.Batches,
                DisplayValue = JsonSerializer.Serialize(new
                {
                    number = document.DocNumber,
                    date = document.DocDate
                }),
                EntityId = document.Id,
                SortOrder = 3,
            },
            new()
            {
                SubkontoTypeId = SubkontoTypeIdConst.Counterparties,
                DisplayValue = counterpartyName,
                EntityId = document.CounterpartyId,
                SortOrder = 4,
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
                SortOrder = 5,
            }
        ];

        private static List<SubkontoValue> BuildServiceSubkontos(PurchaseDoc document, string counterpartyName) =>
        [
            new()
            {
                SubkontoTypeId = SubkontoTypeIdConst.Batches,
                DisplayValue = JsonSerializer.Serialize(new
                {
                    number = document.DocNumber,
                    date = document.DocDate
                }),
                EntityId = document.Id,
                SortOrder = 1,
            },
            new()
            {
                SubkontoTypeId = SubkontoTypeIdConst.Counterparties,
                DisplayValue = counterpartyName,
                EntityId = document.CounterpartyId,
                SortOrder = 2,
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
                SortOrder = 3,
            },
        ];

        private async Task<string> GetCounterpartyNameAsync(int counterpartyId)
        {
            var query = _queryBuilder.For<CounterpartyCard>().Where(x => x.Id == counterpartyId).Build();
            var entity = await _counterpartyCardQuery.GetAsync(query);
            return entity?.FullName ?? "";
        }

        private async Task<string> GetWarehouseNameAsync(int warehouseId)
        {
            var query = _queryBuilder.For<Warehouse>().Where(x => x.Id == warehouseId).Build();
            var entity = await _warehouseQuery.GetAsync(query);
            return entity?.Name ?? "";
        }

        private async Task<(string ContractNumber, DateTime ContractDate)?> GetContractDataAsync(long? contractId)
        {
            var query = _queryBuilder.For<Contract>().Where(x => x.Id == contractId).Build();
            var entity = await _contractQuery.GetAsync(query);
            if (entity == null)
                return null;
            return (entity.ContractNumber, entity.ContractDate);
        }

        private async Task<List<ProductTempDto>> GetProductsAsync(PurchaseDoc document)
        {
            var productIds = document.PurchaseDocProducts.Select(s => s.ProductId).Distinct().ToList();

            var query = _queryBuilder.For<Product>()
                .Where(x => productIds.Contains(x.Id))
                .As(s => new ProductTempDto
                {
                    ProductId = s.Id,
                    ProductName = s.Name,
                    IsService = s.IsService,
                    ProductGroupId = s.ProductGroupId,
                    ProductGroupName = s.ProductGroup != null ? s.ProductGroup.Name : null,
                }).Build();

            var items = await _productQuery.GetAllAsync(query);
            var productMap = items.ToDictionary(x => x.ProductId);

            return document.PurchaseDocProducts
                .GroupBy(x => new
                {
                    x.ProductId,
                    x.DebitAccountId,
                    x.VatAccountId,
                    x.VatRateId
                })
                .Where(group => productMap.ContainsKey(group.Key.ProductId))
                .Select(group =>
                {
                    var item = productMap[group.Key.ProductId];
                    return new ProductTempDto
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        IsService = item.IsService,
                        ProductGroupId = item.ProductGroupId,
                        ProductGroupName = item.ProductGroupName,
                        DebitAccountId = group.Key.DebitAccountId,
                        VatAccountId = group.Key.VatAccountId,
                        VatRateId = group.Key.VatRateId,
                        Quantity = group.Sum(s => s.Quantity),
                        Amount = group.Sum(s => s.Amount),
                        VatAmount = group.Sum(s => s.VatAmount)
                    };
                })
                .ToList();
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
                SortOrder = sortOrder,
            });
        }

        private static void AddVatAccountingMethodSubkonto(PostingContext context, string vatKind, int sortOrder)
        {
            context.Subkontos.Add(new SubkontoValue
            {
                SubkontoTypeId = SubkontoTypeIdConst.VatAccountingMethods,
                DisplayValue = vatKind,
                SortOrder = sortOrder,
            });
        }

        private static void AddVatRateSubkonto(PostingContext context, short? vatRateId, int sortOrder)
        {
            if (!vatRateId.HasValue)
                return;

            context.Subkontos.Add(new SubkontoValue
            {
                SubkontoTypeId = SubkontoTypeIdConst.VatRates,
                DisplayValue = vatRateId.Value.ToString(),
                EntityId = vatRateId.Value,
                SortOrder = sortOrder,
            });
        }

        private static void AddContractSubkonto(
            PostingContext context,
            long? contractId,
            (string ContractNumber, DateTime ContractDate)? contractData,
            int sortOrder)
        {
            if (contractData is null)
                return;

            context.Subkontos.Add(new SubkontoValue
            {
                SubkontoTypeId = SubkontoTypeIdConst.Contracts,
                DisplayValue = JsonSerializer.Serialize(new
                {
                    number = contractData.Value.ContractNumber,
                    date = contractData.Value.ContractDate
                }),
                EntityId = contractId,
                SortOrder = sortOrder,
            });
        }

        private class ProductTempDto
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = null!;
            public bool IsService { get; set; }
            public int? ProductGroupId { get; set; }
            public string? ProductGroupName { get; set; }
            public int? DebitAccountId { get; set; }
            public int? VatAccountId { get; set; }
            public short? VatRateId { get; set; }
            public decimal Quantity { get; set; }
            public decimal Amount { get; set; }
            public decimal VatAmount { get; set; }
        }
    }
}
