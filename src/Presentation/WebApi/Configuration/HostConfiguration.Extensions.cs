using Application.Common.Markers;
using Application.Features.Auth;
using Application.Features.BankOperations;
using Application.Features.Branches;
using Application.Features.CashBoxes;
using Application.Features.CashOperations;
using Application.Features.ChartAccounts;
using Application.Features.CounterpartyBankAccounts;
using Application.Features.CounterpartyCards;
using Application.Features.CounterpartyContacts;
using Application.Features.Departments;
using Application.Features.Manual;
using Application.Features.Organizations;
using Application.Features.OrgBankAccounts;
using Application.Features.Positions;
using Application.Features.ProductGroups;
using Application.Features.ProductPrices;
using Application.Features.Products;
using Application.Features.PurchaseDocs;
using Application.Features.PurchaseDocTables;
using Application.Features.AccountingRegisterEntries;
using Application.Features.CounterpartyRegisterBalances;
using Application.Features.InventoryRegisterBalances;
using Application.Features.MoneyRegisterBalances;
using Application.Features.Roles;
using Application.Features.SaleDocs;
using Application.Features.SaleDocTables;
using Application.Features.Users;
using Application.Features.Users.Services;
using Application.Features.Warehouses;
using FluentValidation;
using Infrastructure;
using Infrastructure.Options;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SharedKernel.Query;
using System.Text;
using System.Text.Json.Serialization;
using WebApi.Infrastructure;

namespace WebApi.Configuration
{
    public static partial class HostConfiguration
    {
        private static WebApplicationBuilder AddDevTools(this WebApplicationBuilder builder)
        {
            builder.Services.AddEndpointsApiExplorer();

            return builder;
        }

        private static WebApplicationBuilder AddExposers(this WebApplicationBuilder builder)
        {
            builder.Services
                .AddControllers(options =>
                {
                    options.Filters.AddService<FluentValidationFilter>(order: -3000);
                })
                .AddJsonOptions(options => 
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = true;
                });

            builder.Services.AddValidatorsFromAssemblyContaining<ApplicationAssemblyMarker>(includeInternalTypes: true);
            builder.Services.AddScoped<FluentValidationFilter>();

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

            builder.Services.AddRouting(options => options.LowercaseUrls = true);

            return builder;
        }

        private static WebApplicationBuilder AddSwagger(this WebApplicationBuilder builder)
        {
            builder.Services.AddSwaggerGen(c =>
            {

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Description = @"JWT Authorization header using the Bearer scheme. <br />
                                    Enter 'Bearer' [space] and then your token in the text input below. <br /><br />
                                    Example: 'Bearer 12345abcdef'"
                });

                c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
            });

            return builder;
        }

        private static WebApplicationBuilder AddApplication(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IOrganizationService, OrganizationService>();
            builder.Services.AddScoped<IBranchService, BranchService>();
            builder.Services.AddScoped<IDepartmentService, DepartmentService>();
            builder.Services.AddScoped<IPositionService, PositionService>();
            builder.Services.AddScoped<ICounterpartyCardService, CounterpartyCardService>();
            builder.Services.AddScoped<ICounterpartyBankAccountService, CounterpartyBankAccountService>();
            builder.Services.AddScoped<ICounterpartyContactService, CounterpartyContactService>();
            builder.Services.AddScoped<IProductGroupService, ProductGroupService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IWarehouseService, WarehouseService>();
            builder.Services.AddScoped<IProductPriceService, ProductPriceService>();
            builder.Services.AddScoped<IOrgBankAccountService, OrgBankAccountService>();
            builder.Services.AddScoped<IBankOperationService, BankOperationService>();
            builder.Services.AddScoped<ICashBoxService, CashBoxService>();
            builder.Services.AddScoped<ICashOperationService, CashOperationService>();
            builder.Services.AddScoped<IPurchaseDocService, PurchaseDocService>();
            builder.Services.AddScoped<IPurchaseDocTableService, PurchaseDocTableService>();
            builder.Services.AddScoped<ISaleDocService, SaleDocService>();
            builder.Services.AddScoped<ISaleDocTableService, SaleDocTableService>();
            builder.Services.AddScoped<IChartAccountService, ChartAccountService>();
            builder.Services.AddScoped<IAccountingRegisterEntryService, AccountingRegisterEntryService>();
            builder.Services.AddScoped<ICounterpartyRegisterBalanceService, CounterpartyRegisterBalanceService>();
            builder.Services.AddScoped<IInventoryRegisterBalanceService, InventoryRegisterBalanceService>();
            builder.Services.AddScoped<IMoneyRegisterBalanceService, MoneyRegisterBalanceService>();
            builder.Services.AddScoped<IManualService, ManualService>();
            builder.Services.AddScoped<IAccountingRegisterEntryService, AccountingRegisterEntryService>();
            builder.Services.AddScoped<ICounterpartyRegisterBalanceService, CounterpartyRegisterBalanceService>();
            builder.Services.AddScoped<IInventoryRegisterBalanceService, InventoryRegisterBalanceService>();
            builder.Services.AddScoped<IMoneyRegisterBalanceService, MoneyRegisterBalanceService>();

            builder.Services.Scan(scan => scan
                .FromAssemblies(typeof(ApplicationAssemblyMarker).Assembly)
                .AddClasses(c => c.AssignableTo(typeof(ICriteriaBuilder<,>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
                .AddClasses(c => c.AssignableTo(typeof(IProjectionBuilder<,>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            );

            return builder;
        }

        private static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
        {
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddInfrastructure();
            return builder;
        }

        private static WebApplicationBuilder AddPersistence(this WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("Default");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'Default' is not configured.");
            }

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                });
            });

            return builder;
        }

        private static WebApplicationBuilder AddJwtToken(this WebApplicationBuilder builder)
        {
            var jwtSection = builder.Configuration.GetSection("Jwt");

            builder.Services.Configure<JwtOptions>(jwtSection);

            var jwt = jwtSection.Get<JwtOptions>()!;

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = jwt.ValidateIssuer,
                        ValidateAudience = jwt.ValidateAudience,
                        ValidateLifetime = jwt.ValidateLifetime,
                        ValidateIssuerSigningKey = jwt.ValidateIssuerSigningKey,
                        ValidIssuer = jwt.Issuer,
                        ValidAudience = jwt.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwt.Key)
                        ),

                        ClockSkew = TimeSpan.FromSeconds(jwt.ClockSkewSeconds)
                    };
                });

            builder.Services.AddAuthorization();

            return builder;
        }

        private static WebApplication UseExposers(this WebApplication app)
        {
            app.MapControllers();

            return app;
        }

        private static WebApplication UseDevTools(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
                c.DisplayRequestDuration();
            });

            return app;
        }

        private static WebApplication UseMiddlewares(this WebApplication app)
        {
            app.UseHttpsRedirection();

            app.UseCors(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });

            app.UseAuthentication();
            app.UseAuthorization();

            return app;
        }
    }
}
