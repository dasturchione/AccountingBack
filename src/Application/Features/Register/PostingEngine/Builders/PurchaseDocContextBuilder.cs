using Application.Abstractions;
using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using System.Text.Json;

namespace Application.Features.Register.PostingEngine
{
    public class PurchaseDocContextBuilder : IPostingContextBuilder<PurchaseDoc>
    {
        private readonly IUserContext _userContext;
        private readonly IQueryBuilder _queryBuilder;
        private readonly IQueryRepository<Contract> _contractQuery;
        private readonly IQueryRepository<Warehouse> _warehouseQuery;
        private readonly IQueryRepository<ProductTable> _productTableQuery;
        private readonly IQueryRepository<CounterpartyCard> _counterpartyCardQuery;
        public PurchaseDocContextBuilder(IUserContext userContext, 
                                         IQueryBuilder queryBuilder,
                                         IQueryRepository<Contract> contractQuery,
                                         IQueryRepository<Warehouse> warehouseQuery,
                                         IQueryRepository<ProductTable> productTableQuery,
                                         IQueryRepository<CounterpartyCard> counterpartyCard)
        {
            _userContext = userContext;
            _queryBuilder = queryBuilder;
            _contractQuery = contractQuery;
            _warehouseQuery = warehouseQuery;
            _productTableQuery = productTableQuery;
            _counterpartyCardQuery = counterpartyCard;
        }

        public async Task<List<PostingContext>> BuildAsync(PurchaseDoc document)
        {
            var result = new List<PostingContext>();

            var productDatas = await GetProductsAsync(document);
            var wareHouseName = await GetWarehouseNameAsync(document.WarehouseId);
            var counterpartyName = await GetCounterpartyNameAsync(document.CounterpartyId);
            var contractData = await GetContractDataAsync(document.ContractId);

            foreach (var productData in productDatas)
            {
                var context = new PostingContext
                {
                    OrganizationId = document.OrganizationId,
                    AccountingPolicyId = AccountingPolicyIdConst.STANDARD_UZ,
                    DocumentTypeId = PostingOperationTypeIdConst.PURCHASE_GOODS,
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
                            SubkontoTypeCode = SubkontoTypeCodeConst.Product,
                            DisplayValue = productData.ProductName,
                            EntityId = productData.ProductId,
                            SortOrder = 1,
                        },
                        new()
                        {
                            SubkontoTypeCode = SubkontoTypeCodeConst.Warehouse,
                            DisplayValue = wareHouseName,
                            EntityId = document.WarehouseId,
                            SortOrder = 2,
                        },
                        new()
                        {
                            SubkontoTypeCode = SubkontoTypeCodeConst.Purchase,
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
                            SubkontoTypeCode = SubkontoTypeCodeConst.Counterparty,
                            DisplayValue = counterpartyName,
                            EntityId = document.CounterpartyId,
                            SortOrder = 2,
                        },
                        new()
                        {
                            SubkontoTypeCode = SubkontoTypeCodeConst.Contract,
                            DisplayValue = JsonSerializer.Serialize(new
                            {
                                number = contractData!.Value.ContractNumber,
                                date = contractData!.Value.ContractDate
                            }),
                            EntityId = document.CounterpartyId,
                            SortOrder = 3,
                        },
                    }
                };
            }

            foreach (var lines in document.PurchaseDocTables.Where(x => x.ItemTypeId == 2))
            {

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
            var productTableIds = document.PurchaseDocTables.Where(x => x.ProductTableId != null).Select(s => s.ProductTableId!);

            var productQuery = _queryBuilder.For<ProductTable>()
                                            .Where(x => productTableIds.Contains(x.Id))
                                            .As(s => new
                                            {
                                                TableId = s.Id,
                                                ProductId = s.ProductId,
                                                ProductName = s.Product.Name,
                                            })
                                            .Build();

            var products = await _productTableQuery.GetAllAsync(productQuery);

            return products.GroupBy(g => g.ProductId).Select(grouped => new ProductTempDto
            {
                ProductId = grouped.Key,
                ProductName = grouped.First().ProductName,
                Quantity = document.PurchaseDocTables.Where(x => x.ProductTableId == grouped.Key).Sum(s => s.Quantity),
                Amount = document.PurchaseDocTables.Where(x => x.ProductTableId == grouped.Key).Sum(s => s.Amount),
                VatAmount = document.PurchaseDocTables.Where(x => x.ProductTableId == grouped.Key).Sum(s => s.VatAmount)
            }).ToList();
        }

        private class ProductTempDto
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = null!;
            public decimal Quantity { get; set; }
            public decimal Amount { get; set; }
            public decimal VatAmount { get; set; }
        }
    }
}
