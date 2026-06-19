using Application.Abstractions;
using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.Register.AccountingRegisterEntries
{
    public class PurchaseSubkontoNamesResolver : IPurchaseSubkontoNamesResolver
    {
        private readonly IQueryBuilder _queryBuilder;
        private readonly IQueryRepository<Product> _productQuery;
        private readonly IQueryRepository<Contract> _contractQuery;
        private readonly IQueryRepository<Warehouse> _warehouseQuery;
        private readonly IQueryRepository<ProductTable> _productTableQuery;
        private readonly IQueryRepository<CounterpartyCard> _counterpartyCardQuery;
        public PurchaseSubkontoNamesResolver(IUserContext userContext,
                                             IQueryBuilder queryBuilder,
                                             IQueryRepository<Product> productQuery,
                                             IQueryRepository<Contract> contractQuery,
                                             IQueryRepository<Warehouse> warehouseQuery,
                                             IQueryRepository<ProductTable> productTableQuery,
                                             IQueryRepository<CounterpartyCard> counterpartyCardQuery)
        {
            _queryBuilder = queryBuilder;
            _productQuery = productQuery;
            _contractQuery = contractQuery;
            _warehouseQuery = warehouseQuery;
            _productTableQuery = productTableQuery;
            _counterpartyCardQuery = counterpartyCardQuery;
        }

        public async Task<PurchaseSubkontoContext> FillSubkontoContext(PurchaseDoc document)
        {
            var context = CreateContext(document);

            await FillContractAsync(context);

            await FillWarehouseAsync(context);

            await FillCounterpartyAsync(context);

            await FillProductsAsync(document, context);

            return context;
        }

        private PurchaseSubkontoContext CreateContext(PurchaseDoc document)
        {
            return new PurchaseSubkontoContext
            {
                Id = document.Id,
                DocDate = document.DocDate,
                CurrencyId = document.CurrencyId,
                OrganizationId = document.OrganizationId,
                WarehouseId = document.WarehouseId,
                CounterpartyId = document.CounterpartyId,
                DocNumber = document.DocNumber,
                ContractId = document.ContractId,
            };
        }

        private async Task FillProductsAsync(PurchaseDoc document, PurchaseSubkontoContext context)
        {
            var productTableIds = document.PurchaseDocTables.Select(s => s.ProductTableId).Distinct().ToList();

            var productsQuery = _queryBuilder.For<ProductTable>()
                                .Where(x => productTableIds.Contains(x.Id))
                                .As(s => new
                                {
                                    Id = s.Id,
                                    ProductName = s.Product.Name,
                                    ProductId = s.ProductId
                                }).Build();

            var products = await _productTableQuery.GetAllAsync(productsQuery);

            var qtyByTableId = document.PurchaseDocTables
                    .GroupBy(x => x.ProductTableId)
                    .ToDictionary(
                        g => g.Key,
                        g => new
                        {
                            Quantity = g.Sum(x => x.Quantity),
                            Amount = g.Sum(x => x.Amount),
                            VatAmount = g.Sum(x => x.VatAmount),
                        }
                    );

            context.Products = products
                    .GroupBy(g => g.ProductId)
                    .Select(grouped =>
                    {
                        var tableIds = grouped.Select(x => x.Id);

                        decimal quantity = 0;
                        decimal amount = 0;
                        decimal vatAmount = 0;

                        foreach (var id in tableIds)
                        {
                            if (qtyByTableId.TryGetValue(id, out var value))
                            {
                                quantity += value.Quantity;
                                amount += value.Amount;
                                vatAmount += value.VatAmount;
                            }
                        }

                        return new ProductPurchaseSubkontoContext
                        {
                            ProductId = grouped.Key,
                            ProductTableIds = grouped.Select(x => x.Id).ToList(),
                            ProductName = grouped.First().ProductName,
                            Quantity = quantity,
                            Amount = amount,
                            VatAmount = vatAmount,
                        };
                    })
                    .ToList();
        }

        private async Task FillContractAsync(PurchaseSubkontoContext context)
        {
            if (context.ContractId == null)
                return;

            var contractQuery = _queryBuilder.For<Contract>().Where(x => x.Id == context.ContractId).Build();
            var contract = await _contractQuery.GetAsync(contractQuery);
            if (contract is not null)
            {
                context.ContractNumber = contract.ContractNumber;
                context.ContractDate = contract.ContractDate;
            }
        }

        private async Task FillWarehouseAsync(PurchaseSubkontoContext context)
        {
            var query = _queryBuilder.For<Warehouse>().Where(x => x.Id == context.WarehouseId).Build();
            var warehouse = await _warehouseQuery.GetAsync(query);
            context?.WarehouseName = warehouse?.Name;
        }

        private async Task FillCounterpartyAsync(PurchaseSubkontoContext context)
        {
            var query = _queryBuilder.For<CounterpartyCard>().Where(x => x.Id == context.CounterpartyId).Build();
            var counterparty = await _counterpartyCardQuery.GetAsync(query);
            context.CounterpartyName = counterparty?.FullName;
        }
    }
}
