using Application.Common.Markers;
using Application.Features.AccountingRegisterEntries;
using Application.Features.Auth;
using Application.Features.BankOperations;
using Application.Features.Branches;
using Application.Features.CashBoxes;
using Application.Features.CashOperations;
using Application.Features.ChartAccounts;
using Application.Features.CounterpartyBankAccounts;
using Application.Features.CounterpartyCards;
using Application.Features.CounterpartyContacts;
using Application.Features.CounterpartyRegisterBalances;
using Application.Features.Departments;
using Application.Features.InventoryRegisterBalances;
using Application.Features.Manual;
using Application.Features.MoneyRegisterBalances;
using Application.Features.OrgBankAccounts;
using Application.Features.Organizations;
using Application.Features.Positions;
using Application.Features.ProductGroups;
using Application.Features.ProductPrices;
using Application.Features.Products;
using Application.Features.PurchaseDocTables;
using Application.Features.PurchaseDocs;
using Application.Features.Roles;
using Application.Features.SaleDocTables;
using Application.Features.SaleDocs;
using Application.Features.Users;
using Application.Features.Users.Services;
using Application.Features.Warehouses;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Query;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // sys
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRoleService, RoleService>();

            // org
            services.AddScoped<IOrganizationService, OrganizationService>();
            services.AddScoped<IBranchService, BranchService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IPositionService, PositionService>();

            // bank
            services.AddScoped<IOrgBankAccountService, OrgBankAccountService>();
            services.AddScoped<IBankOperationService, BankOperationService>();

            // cash
            services.AddScoped<ICashBoxService, CashBoxService>();
            services.AddScoped<ICashOperationService, CashOperationService>();

            // counterparty
            services.AddScoped<ICounterpartyCardService, CounterpartyCardService>();
            services.AddScoped<ICounterpartyBankAccountService, CounterpartyBankAccountService>();
            services.AddScoped<ICounterpartyContactService, CounterpartyContactService>();

            // inv
            services.AddScoped<IProductGroupService, ProductGroupService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IWarehouseService, WarehouseService>();
            services.AddScoped<IProductPriceService, ProductPriceService>();

            // purchase
            services.AddScoped<IPurchaseDocService, PurchaseDocService>();
            services.AddScoped<IPurchaseDocTableService, PurchaseDocTableService>();

            // sale
            services.AddScoped<ISaleDocService, SaleDocService>();
            services.AddScoped<ISaleDocTableService, SaleDocTableService>();

            // acc
            services.AddScoped<IChartAccountService, ChartAccountService>();

            // register (read-only)
            services.AddScoped<IAccountingRegisterEntryService, AccountingRegisterEntryService>();
            services.AddScoped<ICounterpartyRegisterBalanceService, CounterpartyRegisterBalanceService>();
            services.AddScoped<IInventoryRegisterBalanceService, InventoryRegisterBalanceService>();
            services.AddScoped<IMoneyRegisterBalanceService, MoneyRegisterBalanceService>();

            // manual (select lists)
            services.AddScoped<IManualService, ManualService>();

            // auto-register ICriteriaBuilder<,> and IProjectionBuilder<,>
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
