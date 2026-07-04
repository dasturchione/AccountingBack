using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Application.Features.MoneyRegisterBalances;
using Domain.Entities;
using Infrastructure.Context;
using Infrastructure.Options;
using Infrastructure.Persistence;
using Integration.Faktura.Configs;
using Integration.GoogleDrive.Configs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace IntegrationTests;

public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"integration-{Guid.NewGuid():N}";
    private readonly string _environment;
    private readonly Action<IServiceCollection>? _overrideServices;
    private readonly Dictionary<string, string?> _configuration;

    public TestWebApplicationFactory(
        string environment = "Testing",
        Dictionary<string, string?>? configuration = null,
        Action<IServiceCollection>? overrideServices = null)
    {
        _environment = environment;
        _configuration = configuration ?? CreateDefaultConfiguration();
        _overrideServices = overrideServices;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(_environment);

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(_configuration);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(AppDbContext));
            services.RemoveAll(typeof(IDbContextOptionsConfiguration<AppDbContext>));
            RemoveAssemblyServices(services, "Npgsql.EntityFrameworkCore.PostgreSQL");

            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_databaseName));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            services.AddSingleton<IPermissionChecker, AllowAllPermissionChecker>();

            _overrideServices?.Invoke(services);

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            Seed(db);
        });
    }

    private static Dictionary<string, string?> CreateDefaultConfiguration() =>
        new()
        {
            ["ConnectionStrings:Default"] = "Host=localhost;Port=5432;Database=Accounting;Username=postgres;Password=test",
            ["Jwt:Key"] = "12345678901234567890123456789012",
            ["Jwt:Issuer"] = "TestIssuer",
            ["Jwt:Audience"] = "TestAudience",
            ["Jwt:AccessTokenExpirationMinutes"] = "60",
            ["Jwt:RefreshTokenExpirationDays"] = "7",
            ["Jwt:ValidateIssuer"] = "true",
            ["Jwt:ValidateAudience"] = "true",
            ["Jwt:ValidateLifetime"] = "true",
            ["Jwt:ValidateIssuerSigningKey"] = "true",
            ["Jwt:ClockSkewSeconds"] = "10",
            ["BackupJob:EnableEmailSend"] = "false",
            ["BackupJob:PgDumpPath"] = "",
            ["BackupJob:Database:Host"] = "localhost",
            ["BackupJob:Database:Name"] = "Accounting",
            ["BackupJob:Database:User"] = "postgres",
            ["BackupJob:Database:Password"] = "test",
            ["GoogleDrive:CredentialsPath"] = "appdata/drive/credentials.json",
            ["GoogleDrive:BackupFolderId"] = "folder",
            ["FakturaAuthSettings:GrantType"] = "password",
            ["FakturaAuthSettings:Username"] = "user",
            ["FakturaAuthSettings:Password"] = "password",
            ["FakturaAuthSettings:ClientId"] = "client",
            ["FakturaAuthSettings:ClientSecret"] = "secret"
        };

    private static void RemoveAssemblyServices(IServiceCollection services, string assemblyName)
    {
        var descriptors = services
            .Where(descriptor => GetAssemblyName(descriptor) == assemblyName)
            .ToList();

        foreach (var descriptor in descriptors)
            services.Remove(descriptor);
    }

    private static string? GetAssemblyName(ServiceDescriptor descriptor)
    {
        if (descriptor.ImplementationType != null)
            return descriptor.ImplementationType.Assembly.GetName().Name;

        if (descriptor.ImplementationInstance != null)
            return descriptor.ImplementationInstance.GetType().Assembly.GetName().Name;

        if (descriptor.ImplementationFactory != null)
            return descriptor.ImplementationFactory.GetMethodInfo().ReturnType.Assembly.GetName().Name;

        return descriptor.ServiceType.Assembly.GetName().Name;
    }

    private static void Seed(AppDbContext db)
    {
        db.SetUserContext(new FakeUserContext
        {
            Id = 1,
            RoleId = 1,
            HasGlobalAccess = true
        });

        db.AuditLogs.AddRange(
            new AuditLog
            {
                Id = 1,
                OrganizationId = 1,
                SchemaName = "public",
                TableName = "pur_doc",
                RecordId = "100",
                Action = "UPDATE",
                OldData = "{\"status\":\"draft\"}",
                NewData = "{\"status\":\"posted\"}",
                ChangedDate = DateTime.UtcNow,
                ChangedUserId = 10
            },
            new AuditLog
            {
                Id = 2,
                OrganizationId = 2,
                SchemaName = "public",
                TableName = "pur_doc",
                RecordId = "100",
                Action = "UPDATE",
                OldData = "{\"status\":\"draft\"}",
                NewData = "{\"status\":\"posted\"}",
                ChangedDate = DateTime.UtcNow,
                ChangedUserId = 20
            });

        db.UserOrganizations.AddRange(
            new UserOrganization
            {
                UserId = 101,
                OrganizationId = 1,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.UtcNow,
                JoinedAt = DateTime.UtcNow
            },
            new UserOrganization
            {
                UserId = 102,
                OrganizationId = 2,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.UtcNow,
                JoinedAt = DateTime.UtcNow
            });

        db.SaveChanges();
    }
}

public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = Request.Headers.TryGetValue("X-Test-UserId", out var userIdValues)
            ? userIdValues.ToString()
            : "101";
        var roleId = Request.Headers.TryGetValue("X-Test-RoleId", out var roleIdValues)
            ? roleIdValues.ToString()
            : "1";
        var orgId = Request.Headers.TryGetValue("X-Test-OrgId", out var orgIdValues)
            ? orgIdValues.ToString()
            : null;
        var globalAccess = Request.Headers.TryGetValue("X-Test-GlobalAccess", out var globalValues)
            ? globalValues.ToString()
            : "false";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Role, roleId)
        };

        if (!string.IsNullOrWhiteSpace(orgId))
            claims.Add(new Claim("OrganizationId", orgId));

        if (bool.TryParse(globalAccess, out var isGlobal) && isGlobal)
            claims.Add(new Claim("HasGlobalAccess", "true"));

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public sealed class AllowAllPermissionChecker : IPermissionChecker
{
    public Task<bool> HasPermissionAsync(int roleId, string permissionCode, CancellationToken ct = default) =>
        Task.FromResult(true);

    public Task<bool> HasAnyPermissionAsync(int roleId, IEnumerable<string> permissionCodes, CancellationToken ct = default) =>
        Task.FromResult(true);
}

public sealed class ThrowingMoneyRegisterBalanceService : IMoneyRegisterBalanceService
{
    public Task<Result<long>> CreateAsync(MoneyRegisterBalanceCreateDto dto, CancellationToken ct = default) =>
        throw new InvalidOperationException("sensitive-stack-detail");

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        throw new InvalidOperationException("sensitive-stack-detail");

    public Task<Result<PagedResponse<MoneyRegisterBalanceListDto>>> GetAllAsync(MoneyRegisterBalanceListFilter filter, CancellationToken ct = default) =>
        throw new InvalidOperationException("sensitive-stack-detail");

    public Task<Result<MoneyRegisterBalanceDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        throw new InvalidOperationException("sensitive-stack-detail");

    public Task<Result> UpdateAsync(long id, MoneyRegisterBalanceUpdateDto dto, CancellationToken ct = default) =>
        throw new InvalidOperationException("sensitive-stack-detail");
}

public sealed class FakeUserContext : IUserContext
{
    public int? Id { get; init; }
    public int? RoleId { get; init; }
    public short? LanguageId { get; init; }
    public int? OrganizationId { get; init; }
    public List<int> AllowedOrganizationIds { get; init; } = [];
    public int? BranchId { get; init; }
    public bool HasGlobalAccess { get; init; }
}
