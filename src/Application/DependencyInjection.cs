using Application.Common.Markers;
using Application.Features.AuditLogs;
using Application.Features.AccountingRegisterEntries;
using Application.Features.Auth;
using Application.Features.BankOperations;
using Application.Features.BankParsers;
using Application.Features.Banks;
using Application.Features.Branches;
using Application.Features.CashBoxes;
using Application.Features.CashOperations;
using Application.Features.ChartAccounts;
using Application.Features.Contracts;
using Application.Features.CounterpartyBankAccounts;
using Application.Features.CounterpartyCards;
using Application.Features.CounterpartyContacts;
using Application.Features.CounterpartyRegisterBalances;
using Application.Features.Departments;
using Application.Features.InventoryRegisterBalances;
using Application.Features.Manual;
using Application.Features.MoneyRegisterBalances;
using Application.Features.Organizations;
using Application.Features.OrgBankAccounts;
using Application.Features.Positions;
using Application.Features.ProductGroups;
using Application.Features.ProductPrices;
using Application.Features.Products;
using Application.Features.PricingConditions;
using Application.Features.PurchaseDocs;
using Application.Features.PurchaseDocTables;
using Application.Features.Register.AccountingRegisterEntries;
using Application.Features.Register.PostingEngines;
using Application.Features.Roles;
using Application.Features.SaleConditions;
using Application.Features.SaleDocs;
using Application.Features.SaleDocTables;
using Application.Features.Users;
using Application.Features.Users.Services;
using Application.Features.Warehouses;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Query;
using Application.Features.Inv.ProductStocks;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IOrganizationService, OrganizationService>();
            services.AddScoped<IBranchService, BranchService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IPositionService, PositionService>();
            services.AddScoped<IContractService, ContractService>();
            services.AddScoped<ICounterpartyCardService, CounterpartyCardService>();
            services.AddScoped<ICounterpartyBankAccountService, CounterpartyBankAccountService>();
            services.AddScoped<ICounterpartyContactService, CounterpartyContactService>();
            services.AddScoped<IProductGroupService, ProductGroupService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IWarehouseService, WarehouseService>();
            services.AddScoped<IProductStockService, ProductStockService>();
            services.AddScoped<IProductSalePriceService, ProductSalePriceService>();
            services.AddScoped<IProductPriceService, ProductPriceService>();
            services.AddScoped<IPricingConditionService, PricingConditionService>();
            services.AddScoped<ISaleConditionService, SaleConditionService>();
            services.AddScoped<IOrgBankAccountService, OrgBankAccountService>();
            services.AddScoped<IBankOperationService, BankOperationService>();
            services.AddScoped<IBankStatementParserService, BankStatementParserService>();
            services.AddScoped<IBankService, BankService>();
            services.AddScoped<ICashBoxService, CashBoxService>();
            services.AddScoped<ICashOperationService, CashOperationService>();
            services.AddScoped<IPurchaseDocService, PurchaseDocService>();
            services.AddScoped<IPurchaseDocTableService, PurchaseDocTableService>();
            services.AddScoped<ISaleDocService, SaleDocService>();
            services.AddScoped<ISaleDocTableService, SaleDocTableService>();
            services.AddScoped<IChartAccountService, ChartAccountService>();
            services.AddScoped<IAccountingRegisterEntryService, AccountingRegisterEntryService>();
            services.AddScoped<ICounterpartyRegisterBalanceService, CounterpartyRegisterBalanceService>();
            services.AddScoped<IInventoryRegisterBalanceService, InventoryRegisterBalanceService>();
            services.AddScoped<IMoneyRegisterBalanceService, MoneyRegisterBalanceService>();
            services.AddScoped<IManualService, ManualService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<IAccountingRegisterEntryService, AccountingRegisterEntryService>();
            services.AddScoped<ICounterpartyRegisterBalanceService, CounterpartyRegisterBalanceService>();
            services.AddScoped<IInventoryRegisterBalanceService, InventoryRegisterBalanceService>();
            services.AddScoped<IMoneyRegisterBalanceService, MoneyRegisterBalanceService>();
            services.AddScoped<IAccountingDispatcher, AccountingDispatcher>();
            services.AddScoped<IPostingContextDispatcher, PostingContextDispatcher>();
            services.AddScoped<IPostingService, PostingService>();
            services.AddScoped<IPostingContextBuilder<PurchaseDoc>, PurchaseDocContextBuilder>();
            services.AddScoped<IPostingContextBuilder<SaleDoc>, SaleDocContextBuilder>();

            services.AddScoped<IInventoryDispatcher, InventoryDispatcher>();
            services.AddScoped<IInventoryDocumentHandler<PurchaseDoc>, PurchaseInventoryHandler>();
            services.AddScoped<IInventoryDocumentHandler<SaleDoc>, SaleInventoryHandler>();

            services.Scan(scan => scan
                .FromAssemblies(typeof(ApplicationAssemblyMarker).Assembly)
                .AddClasses(c => c.AssignableTo(typeof(ICriteriaBuilder<,>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
                .AddClasses(c => c.AssignableTo(typeof(IProjectionBuilder<,>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            );

            return services;
        }
    }
}
