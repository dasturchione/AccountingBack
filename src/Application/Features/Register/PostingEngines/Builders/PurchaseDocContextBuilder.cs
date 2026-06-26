using Application.Abstractions;
using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using System.Text.Json;

namespace Application.Features.Register.PostingEngines
{
    public class PurchaseDocContextBuilder : IPostingContextBuilder<PurchaseDoc>
    {
        private readonly IUserContext _userContext;
        private readonly IQueryBuilder _queryBuilder;
        private readonly IQueryRepository<Contract> _contractQuery;
        private readonly IQueryRepository<Warehouse> _warehouseQuery;
        private readonly IQueryRepository<ProductTable> _productTableQuery;
        private readonly IQueryRepository<PurchaseService> _purchaseServiceQuery;
        private readonly IQueryRepository<CounterpartyCard> _counterpartyCardQuery;
        public PurchaseDocContextBuilder(IUserContext userContext, 
                                         IQueryBuilder queryBuilder,
                                         IQueryRepository<Contract> contractQuery,
                                         IQueryRepository<Warehouse> warehouseQuery,
                                         IQueryRepository<ProductTable> productTableQuery,
                                         IQueryRepository<PurchaseService> purchaseServiceQuery,
                                         IQueryRepository<CounterpartyCard> counterpartyCardQuery)
        {
            _userContext = userContext;
            _queryBuilder = queryBuilder;
            _contractQuery = contractQuery;
            _warehouseQuery = warehouseQuery;
            _productTableQuery = productTableQuery;
            _purchaseServiceQuery = purchaseServiceQuery;
            _counterpartyCardQuery = counterpartyCardQuery;
        }

        public async Task<List<PostingContext>> BuildAsync(PurchaseDoc document)
        {
            var result = new List<PostingContext>();

            var productDatas = await GetProductsAsync(document);
            var purchasedServices = await GetServicesAsync(document);
            var wareHouseName = await GetWarehouseNameAsync(document.WarehouseId);
            var counterpartyName = await GetCounterpartyNameAsync(document.CounterpartyId);
            var contractData = await GetContractDataAsync(document.ContractId);

            foreach (var productData in productDatas)
            {
                var context = new PostingContext
                {
                    OrganizationId = document.OrganizationId,
                    AccountingPolicyId = AccountingPolicyIdConst.STANDARD_UZ,
                    RuleId = PostingRuleIdConst.PURCHASE_GOODS,
                    DocumentId = document.Id,
                    DocDate = document.DocDate,
                    CurrencyId = document.CurrencyId,
                    JournalNumber = document.DocNumber,
                    ProductCategory = null,
                    DebitQuantity = productData.Quantity,

                    Amounts = new Dictionary<string, decimal>
                    {
                        [AmountSourceConst.Base] = productData.Amount,
                        [AmountSourceConst.VAT] = productData.VatAmount
                    },

                    Subkontos = new List<SubkontoValue>
                    {
                        new()
                        {
                            SubkontoTypeId = SubkontoTypeIdConst.PRODUCT,
                            DisplayValue = productData.ProductName,
                            EntityId = productData.ProductId,
                            SortOrder = 1,
                        },
                        new()
                        {
                            SubkontoTypeId = SubkontoTypeIdConst.WAREHOUSE,
                            DisplayValue = wareHouseName,
                            EntityId = document.WarehouseId,
                            SortOrder = 2,
                        },
                        new()
                        {
                            SubkontoTypeId = SubkontoTypeIdConst.PURCHASE,
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
                            SubkontoTypeId = SubkontoTypeIdConst.COUNTER_PARTY,
                            DisplayValue = counterpartyName,
                            EntityId = document.CounterpartyId,
                            SortOrder = 4,
                        },
                    }
                };

                if (contractData is not null)
                {
                    context.Subkontos.Add(new SubkontoValue
                    {
                        SubkontoTypeId = SubkontoTypeIdConst.CONTRACT,
                        DisplayValue = JsonSerializer.Serialize(new
                        {
                            number = contractData.Value.ContractNumber,
                            date = contractData.Value.ContractDate
                        }),
                        EntityId = document.ContractId,
                        SortOrder = 5,
                    });
                }

                result.Add(context);
            }

            foreach (var service in purchasedServices)
            {
                var context = new PostingContext
                {
                    OrganizationId = document.OrganizationId,
                    AccountingPolicyId = AccountingPolicyIdConst.STANDARD_UZ,
                    RuleId = PostingRuleIdConst.PURCHASE_SERVICE,
                    DocumentId = document.Id,
                    DocDate = document.DocDate,
                    CurrencyId = document.CurrencyId,
                    JournalNumber = document.DocNumber,
                    ServiceType = service.ServiceTypeId switch
                    {
                        ServiceTypeIdConst.Production => "production",
                        ServiceTypeIdConst.Administrative => "admin",
                        _ => "_default"
                    },

                    Amounts = new Dictionary<string, decimal>
                    {
                        [AmountSourceConst.Base] = service.Amount,
                        [AmountSourceConst.VAT] = service.VatAmount
                    },

                    Subkontos = new List<SubkontoValue>
                    {
                        new()
                        {
                            SubkontoTypeId = SubkontoTypeIdConst.PURCHASE,
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
                            SubkontoTypeId = SubkontoTypeIdConst.COUNTER_PARTY,
                            DisplayValue = counterpartyName,
                            EntityId = document.CounterpartyId,
                            SortOrder = 2,
                        },
                    }
                };

                if (contractData is not null)
                {
                    context.Subkontos.Add(new SubkontoValue
                    {
                        SubkontoTypeId = SubkontoTypeIdConst.CONTRACT,
                        DisplayValue = JsonSerializer.Serialize(new
                        {
                            number = contractData.Value.ContractNumber,
                            date = contractData.Value.ContractDate
                        }),
                        EntityId = document.ContractId,
                        SortOrder = 3,
                    });
                }

                result.Add(context);
            }

            return result;
        }

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
            var productLines = document.PurchaseDocProducts
                .Where(x => x.ItemTypeId == PurchaseItemTypeIdConst.PRODUCT)
                .ToList();
            await Task.CompletedTask;

            return productLines.GroupBy(g => g.ProductId).Select(grouped => new ProductTempDto
            {
                ProductId = grouped.Key,
                ProductName = grouped.First().Product?.Name ?? "",
                Quantity = grouped.Sum(s => s.Quantity),
                Amount = grouped.Sum(s => s.Amount),
                VatAmount = grouped.Sum(s => s.VatAmount)
            }).ToList();
        }

        private async Task<List<ServiceTempDto>> GetServicesAsync(PurchaseDoc document)
        {
            await Task.CompletedTask;
            return new List<ServiceTempDto>();
        }

        private async Task<List<PurchaseService>> GetServiceDetailsAync(PurchaseDoc document)
        {
            var serviceIds = new List<long>();
            var query = _queryBuilder.For<PurchaseService>().Where(x => serviceIds.Contains(x.Id)).Build();
            return await _purchaseServiceQuery.GetAllAsync(query);
        }

        private class ProductTempDto
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = null!;
            public decimal Quantity { get; set; }
            public decimal Amount { get; set; }
            public decimal VatAmount { get; set; }
        }

        private class ServiceTempDto
        {
            public long ServiceId { get; set; }
            public string ServiceName { get; set; } = null!;
            public int ServiceTypeId { get; set; }
            public decimal Amount { get; set; }
            public decimal VatAmount { get; set; }
            public bool VatApplicable { get; set; } = true;
        }
    }
}
