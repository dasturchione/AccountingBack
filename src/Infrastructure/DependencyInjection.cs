using Application;
using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Markers;
using Application.Features.AuditLogs;
using Application.Features.Acc.PostingTemplateViews;
using Application.Features.Acc.AccountingPeriods;
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
using Application.Features.Inv;
using Application.Features.InventoryCounts;
using Application.Features.InventoryAdjustments;
using Application.Features.Inv.ProductPrices;
using Application.Features.WarehouseTransfers;
using Application.Features.Manual;
using Application.Features.MoneyRegisterBalances;
using Application.Features.Organizations;
using Application.Features.OrganizationSetup;
using Application.Features.OrgBankAccounts;
using Application.Features.Platform;
using Application.Features.Positions;
using Application.Features.PricingConditions;
using Application.Features.ProductGroups;
using Application.Features.Products;
using Application.Features.PurchaseDocs;
using Application.Features.PurchaseDocTables;
using Application.Features.Register.AccountingRegisterEntries;
using Application.Features.Register.PostingEngines;
using Application.Features.Register.PostingEngines.Builders;
using Application.Features.Roles;
using Application.Features.SaleConditions;
using Application.Features.SaleDocs;
using Application.Features.SaleDocTables;
using Application.Features.Users;
using Application.Features.Users.Services;
using Application.Features.Warehouses;
using Application.Features.Inv.ProductStocks;
using Domain.Entities;
using Infrastructure.Authentication;
using Infrastructure.Context;
using Infrastructure.Query;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Security;
using Integration.Faktura.Configs;
using Integration.GoogleDrive.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Query;
using Scrutor;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, ConfigurationManager config)
        {
            services.AddScoped(typeof(IQueryRepository<>), typeof(QueryRepository<>));
            services.AddScoped(typeof(ICommandRepository<>), typeof(CommandRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IDocumentPostingLock, DocumentPostingLock>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ITokenProvider, TokenProvider>();
            services.AddScoped<IRequestContext, RequestContext>();
            services.AddScoped<IUserContext, UserContext>();
            services.AddScoped<IInventoryReadDbContext, InventoryReadDbContext>();
            services.AddScoped<IPermissionChecker, PermissionChecker>();
            services.AddScoped<IPostingTemplateViewService, PostingTemplateViewService>();

            services.AddScoped<IDocNumberGenerator, DocNumberGenerator>();
            services.AddScoped<IProductTableReservationService, ProductTableReservationService>();

            services.AddScoped<IQueryBuilder, QueryBuilder>();
            services.AddScoped<IQueryBuilderResolver, QueryBuilderResolver>();

            services.AddMemoryCache();

            services.AddFaktura(config);
            services.AddGoogleDriveIntegration(config);

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IOrganizationService, OrganizationService>();
            services.AddScoped<IOrganizationSetupService, OrganizationSetupService>();
            services.AddScoped<IPlatformService, PlatformService>();
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
            services.AddScoped<IWarehouseTransferService, WarehouseTransferService>();
            services.AddScoped<IInventoryAdjustmentService, InventoryAdjustmentService>();
            services.AddScoped<IInventoryCountService, InventoryCountService>();
            services.AddScoped<IActiveInventoryCountGuardService, ActiveInventoryCountGuardService>();
            services.AddScoped<IWarehouseTransferLifecycleService, WarehouseTransferLifecycleService>();
            services.AddScoped<IInventoryAdjustmentLifecycleService, InventoryAdjustmentLifecycleService>();
            services.AddScoped<IInventoryCountLifecycleService, InventoryCountLifecycleService>();
            services.AddScoped<IProductStockService, ProductStockService>();
            services.AddScoped<IProductPriceCalculateService, ProductPriceCalculateService>();
            services.AddScoped<IProductPriceService, ProductPriceService>();
            services.AddScoped<IPricingConditionService, PricingConditionService>();
            services.AddScoped<ISaleConditionService, SaleConditionService>();
            services.AddScoped<IOrgBankAccountService, OrgBankAccountService>();
            services.AddScoped<IBankOperationService, BankOperationService>();
            services.AddScoped<IBankLifecycleService, BankLifecycleService>();
            services.AddScoped<IBankStatementParserService, BankStatementParserService>();
            services.AddScoped<IBankService, BankService>();
            services.AddScoped<ICashBoxService, CashBoxService>();
            services.AddScoped<ICashOperationService, CashOperationService>();
            services.AddScoped<ICashLifecycleService, CashLifecycleService>();
            services.AddScoped<ICashCounterpartyRegisterService, CashCounterpartyRegisterService>();
            services.AddScoped<ICashMoneyRegisterService, CashMoneyRegisterService>();
            services.AddScoped<IPurchaseDocService, PurchaseDocService>();
            services.AddScoped<IPurchaseLifecycleService, PurchaseLifecycleService>();
            services.AddScoped<IPurchaseDocTableService, PurchaseDocTableService>();
            services.AddScoped<ISaleDocService, SaleDocService>();
            services.AddScoped<ISaleLifecycleService, SaleLifecycleService>();
            services.AddScoped<ISaleDocTableService, SaleDocTableService>();
            services.AddScoped<IChartAccountService, ChartAccountService>();
            services.AddScoped<IAccountingRegisterEntryService, AccountingRegisterEntryService>();
            services.AddScoped<ICounterpartyRegisterBalanceService, CounterpartyRegisterBalanceService>();
            services.AddScoped<IBankCounterpartyRegisterService, BankCounterpartyRegisterService>();
            services.AddScoped<IPurchaseCounterpartyRegisterService, PurchaseCounterpartyRegisterService>();
            services.AddScoped<ISaleCounterpartyRegisterService, SaleCounterpartyRegisterService>();
            services.AddScoped<IInventoryRegisterBalanceService, InventoryRegisterBalanceService>();
            services.AddScoped<IMoneyRegisterBalanceService, MoneyRegisterBalanceService>();
            services.AddScoped<IBankMoneyRegisterService, BankMoneyRegisterService>();
            services.AddScoped<ISaleMoneyRegisterService, SaleMoneyRegisterService>();
            services.AddScoped<IManualService, ManualService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<IAccountingDispatcher, AccountingDispatcher>();
            services.AddScoped<IPostingContextDispatcher, PostingContextDispatcher>();
            services.AddScoped<IPostingService, PostingService>();
            services.AddScoped<IAccountingPostingValidator, AccountingPostingValidator>();
            services.AddScoped<IAccountingPeriodValidator, AccountingPeriodValidator>();
            services.AddScoped<IPostingContextBuilder<PurchaseDoc>, PurchaseDocContextBuilder>();
            services.AddScoped<IPostingContextBuilder<SaleDoc>, SaleDocContextBuilder>();
            services.AddScoped<IPostingContextBuilder<List<BankOperation>>, BankOperationContextBuilder>();
            services.AddScoped<IPostingContextBuilder<CashOperation>, CashOperationContextBuilder>();
            services.AddScoped<IPostingContextBuilder<BankOperation>, BankOperationContextBuilder>();

            services.AddScoped<IInventoryDispatcher, InventoryDispatcher>();
            services.AddScoped<IInventoryDocumentHandler<PurchaseDoc>, PurchaseInventoryHandler>();
            services.AddScoped<IInventoryDocumentHandler<SaleDoc>, SaleInventoryHandler>();
            services.AddScoped<IInventoryDocumentHandler<WarehouseTransferDoc>, WarehouseTransferInventoryHandler>();
            services.AddScoped<IInventoryDocumentHandler<InventoryAdjustmentDoc>, InventoryAdjustmentInventoryHandler>();

            services.Scan(scan => scan
                .FromAssemblies(typeof(ApplicationAssemblyMarker).Assembly)
                .AddClasses(c => c.AssignableTo(typeof(ICriteriaBuilder<,>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
                .AddClasses(c => c.AssignableTo(typeof(IProjectionBuilder<,>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime());

            return services;
        }
    }
}
