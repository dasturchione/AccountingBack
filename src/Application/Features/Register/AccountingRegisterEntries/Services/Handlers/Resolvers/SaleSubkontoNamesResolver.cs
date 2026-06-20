using Application.Abstractions;
using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.Register.AccountingRegisterEntries.Services
{
    public class SaleSubkontoNamesResolver : ISaleSubkontoNamesResolver
    {
        private readonly IQueryBuilder _queryBuilder;
        private readonly IQueryRepository<Product> _productQuery;
        private readonly IQueryRepository<Contract> _contractQuery;
        private readonly IQueryRepository<Warehouse> _warehouseQuery;
        private readonly IQueryRepository<ProductTable> _productTableQuery;
        private readonly IQueryRepository<CounterpartyCard> _counterpartyCardQuery;
        private readonly IQueryRepository<PurchaseDocTable> _purchaseDocTableQuery;
        public SaleSubkontoNamesResolver(IUserContext userContext,
                                         IQueryBuilder queryBuilder,
                                         IQueryRepository<Product> productQuery,
                                         IQueryRepository<Contract> contractQuery,
                                         IQueryRepository<Warehouse> warehouseQuery,
                                         IQueryRepository<ProductTable> productTableQuery,
                                         IQueryRepository<CounterpartyCard> counterpartyCardQuery,
                                         IQueryRepository<PurchaseDocTable> purchaseDocTableQuery)
        {
            _queryBuilder = queryBuilder;
            _productQuery = productQuery;
            _contractQuery = contractQuery;
            _warehouseQuery = warehouseQuery;
            _productTableQuery = productTableQuery;
            _counterpartyCardQuery = counterpartyCardQuery;
            _purchaseDocTableQuery = purchaseDocTableQuery;
        }

        public async Task<SaleSubkontoContext> FillSubkontoContext(SaleDoc document)
        {
            var context = CreateContext(document);

            await FillWarehouseAsync(context);

            await FillCounterpartyAsync(context);

            await FillProductsAsync(document, context);

            return context;
        }

        private SaleSubkontoContext CreateContext(SaleDoc document)
        {
            return new SaleSubkontoContext
            {
                Id = document.Id,
                CounterpartyId = document.CounterpartyId, 
                WarehouseId = document.WarehouseId,
                CurrencyId = document.CurrencyId,
                DocDate = document.DocDate,
                DocNumber = document.DocNumber,
                OrganizationId = document.OrganizationId,
            };
        }

        private async Task FillWarehouseAsync(SaleSubkontoContext context)
        {
            var query = _queryBuilder.For<Warehouse>().Where(x => x.Id == context.WarehouseId).Build();
            var warehouse = await _warehouseQuery.GetAsync(query);
            context.WarehouseName = warehouse?.Name;
        }

        private async Task FillCounterpartyAsync(SaleSubkontoContext context)
        {
            var query = _queryBuilder.For<CounterpartyCard>().Where(x => x.Id == context.CounterpartyId).Build();
            var counterparty = await _counterpartyCardQuery.GetAsync(query);
            context.CounterpartyName = counterparty?.FullName;
        }

        private async Task FillProductsAsync(SaleDoc document, SaleSubkontoContext context)
        {
            var productTableIds = document.SaleDocTables.Select(s => s.ProductTableId).Distinct().ToList();

            var purchaseQuery = _queryBuilder.For<PurchaseDocTable>()
                                    .Where(x => productTableIds.Contains(x.ProductTableId))
                                    .As(s => new 
                                    {
                                        ProductTableId = s.ProductTableId,
                                        PurchaseId = s.OwnerId,
                                        Date = s.Owner.DocDate,
                                        Amount = s.Amount,
                                        DocNumber = s.Owner.DocNumber,
                                        WarehouseId = s.Owner.WarehouseId, 
                                        WarehouseName = s.Owner.Warehouse.Name,
                                        Quantity = s.Quantity
                                    }).Build();

            var purchases = await _purchaseDocTableQuery.GetAllAsync(purchaseQuery);

            var productsQuery = _queryBuilder.For<ProductTable>()
                                .Where(x => productTableIds.Contains(x.Id))
                                .As(s => new
                                {
                                    TableId = s.Id,
                                    ProductName = s.Product.Name,
                                    ProductId = s.ProductId
                                }).Build();

            var productTables = await _productTableQuery.GetAllAsync(productsQuery);

            var lastPurchases = purchases
                .GroupBy(x => x.ProductTableId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.Date).First());

            var saleDocTablesByProductTable = document.SaleDocTables.ToDictionary(x => x.ProductTableId);

            context.Products = productTables
                        .GroupBy(x => new { x.ProductId, x.ProductName })
                        .Select(group =>
                        {
                            var saleRows = group
                                .Select(x => saleDocTablesByProductTable[x.TableId])
                                .ToList();

                            return new SaleProductSubkontoContext
                            {
                                ProductId = group.Key.ProductId,
                                ProductName = group.Key.ProductName,
                                Quantity = saleRows.Sum(x => x.Quantity),
                                Amount = saleRows.Sum(x => x.Amount),
                                VatAmount = saleRows.Sum(x => x.VatAmount),
                                VatRateId = saleRows.First().VatRateId,
                                
                                Purchases = group
                                    .Select(x => lastPurchases.GetValueOrDefault(x.TableId))
                                    .Where(x => x != null)
                                    .Select(x => new SaleProductPurchaseSubkontoContext
                                    {
                                        WarehouseId = x!.WarehouseId,
                                        Warehouse = x.WarehouseName,
                                        PurchaseId = x.PurchaseId,
                                        PurchaseDocNumber = x.DocNumber,
                                        PurchaseDate = x.Date,
                                        PurchaseAmount = x.Amount,
                                        Quantity = x.Quantity
                                    })
                                    .ToList()
                            };
                        })
                        .ToList();
        }
    }
}
