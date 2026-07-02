using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Application.Common.Pagination;
using Application.Features.AuditLogs;
using Application.Features.MoneyRegisterBalances;
using Domain.Entities;
using Infrastructure.Context;
using Infrastructure.Options;
using Infrastructure.Persistence;
using Integration.Faktura.Configs;
using Integration.GoogleDrive.Configs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SharedKernel.Constants;
using WebApi.Middlewares;

namespace IntegrationTests;

public class SecurityHardeningIntegrationTests
{
    [Fact]
    public async Task ContractCreate_ShouldRejectForeignCounterparty()
    {
        await using var factory = new TestWebApplicationFactory();

        await using (var seedScope = factory.Services.CreateAsyncScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.SetUserContext(new FakeUserContext
            {
                Id = 999,
                RoleId = 1,
                AllowedOrganizationIds = [1, 2],
                HasGlobalAccess = true
            });

            db.CounterpartyCards.Add(new CounterpartyCard
            {
                Id = 5001,
                OrganizationId = 2,
                CounterpartyTypeId = 1,
                ShortName = "FOREIGN-COUNTERPARTY",
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", "101");
        client.DefaultRequestHeaders.Add("X-Test-RoleId", "1");
        client.DefaultRequestHeaders.Add("X-OrganizationId", "1");

        var payload = new
        {
            counterpartyId = 5001,
            contractTypeId = (short)1,
            contractDate = DateTime.UtcNow,
            startDate = DateTime.UtcNow,
            endDate = DateTime.UtcNow,
            comment = "Test"
        };

        var response = await client.PostAsJsonAsync("/api/contracts", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RateLimiting_ShouldReturnTooManyRequests_WhenPermitLimitExceeded()
    {
        await using var factory = new TestWebApplicationFactory();

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", "101");
        client.DefaultRequestHeaders.Add("X-Test-RoleId", "1");
        client.DefaultRequestHeaders.Add("X-OrganizationId", "1");

        var tooManyRequestsObserved = false;

        for (var i = 0; i < 1250; i++)
        {
            var response = await client.GetAsync("/api/contracts");
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                tooManyRequestsObserved = true;
                break;
            }
            response.Dispose();
        }

        Assert.True(tooManyRequestsObserved);
    }

    [Fact]
    public async Task BankStatementParser_ShouldRejectInvalidFileUploadPayload()
    {
        await using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", "101");
        client.DefaultRequestHeaders.Add("X-Test-RoleId", "1");
        client.DefaultRequestHeaders.Add("X-OrganizationId", "1");

        var content = new MultipartFormDataContent();
        var badFile = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes("not an excel file"));
        badFile.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(badFile, "file", "statement.txt");

        var response = await client.PostAsync("/api/bank-statement-parser/parse", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task BankStatementParser_ShouldRejectOversizedUpload()
    {
        await using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", "101");
        client.DefaultRequestHeaders.Add("X-Test-RoleId", "1");
        client.DefaultRequestHeaders.Add("X-OrganizationId", "1");

        var oversized = new byte[6 * 1024 * 1024];
        var content = new MultipartFormDataContent();
        var tooBigFile = new ByteArrayContent(oversized);
        tooBigFile.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(tooBigFile, "file", "statement.xlsx");

        var response = await client.PostAsync("/api/bank-statement-parser/parse", content);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
    }
    [Fact]
    public async Task AuditLogs_GlobalUser_ShouldSeeAllOrganizations()
    {
        await using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", "900");
        client.DefaultRequestHeaders.Add("X-Test-RoleId", "1");
        client.DefaultRequestHeaders.Add("X-Test-GlobalAccess", "true");

        var response = await client.GetAsync("/api/audit-logs?tableName=pur_doc&recordId=100");

        response.EnsureSuccessStatusCode();
        var logs = await response.Content.ReadFromJsonAsync<List<AuditLogDto>>();

        Assert.NotNull(logs);
        Assert.Equal(2, logs.Count);
    }

    [Fact]
    public async Task AuditLogs_UserWithOrganization_ShouldSeeOnlyOwnOrganization()
    {
        await using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", "101");
        client.DefaultRequestHeaders.Add("X-Test-RoleId", "1");
        client.DefaultRequestHeaders.Add("X-OrganizationId", "1");

        var response = await client.GetAsync("/api/audit-logs?tableName=pur_doc&recordId=100");

        response.EnsureSuccessStatusCode();
        var logs = await response.Content.ReadFromJsonAsync<List<AuditLogDto>>();

        Assert.NotNull(logs);
        Assert.Single(logs);
        Assert.All(logs, log => Assert.Equal(1, log.OrganizationId));
    }

    [Fact]
    public async Task AuditLogs_UserWithoutOrganizations_ShouldReceiveNoData()
    {
        await using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", "777");
        client.DefaultRequestHeaders.Add("X-Test-RoleId", "1");

        var response = await client.GetAsync("/api/audit-logs?tableName=pur_doc&recordId=100");

        response.EnsureSuccessStatusCode();
        var logs = await response.Content.ReadFromJsonAsync<List<AuditLogDto>>();

        Assert.NotNull(logs);
        Assert.Empty(logs);
    }

    [Fact]
    public async Task AuditLogs_UserRequestingForeignOrganization_ShouldBeForbidden()
    {
        await using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", "101");
        client.DefaultRequestHeaders.Add("X-Test-RoleId", "1");
        client.DefaultRequestHeaders.Add("X-OrganizationId", "2");

        var response = await client.GetAsync("/api/audit-logs?tableName=pur_doc&recordId=100");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/register/money-register-balances")]
    [InlineData("/api/register/money-register-balances/1")]
    [InlineData("/api/register/counterparty-register-balances")]
    [InlineData("/api/register/counterparty-register-balances/1")]
    public async Task DerivedRegisterMutationEndpoints_ShouldReturnMethodNotAllowed(string url)
    {
        await using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", "101");
        client.DefaultRequestHeaders.Add("X-Test-RoleId", "1");
        client.DefaultRequestHeaders.Add("X-OrganizationId", "1");

        var payload = new
        {
            organizationId = 1,
            documentTypeId = 1,
            documentId = 1,
            sourceType = "CASH_BOX",
            sourceId = 1,
            operationTypeId = 1,
            currencyId = 1,
            amount = 10,
            docDate = DateTime.UtcNow
        };

        var response = url.EndsWith("/1", StringComparison.Ordinal)
            ? await client.PutAsJsonAsync(url, payload)
            : await client.PostAsJsonAsync(url, payload);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/register/money-register-balances/1")]
    [InlineData("/api/register/counterparty-register-balances/1")]
    public async Task DerivedRegisterDeleteEndpoints_ShouldReturnMethodNotAllowed(string url)
    {
        await using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", "101");
        client.DefaultRequestHeaders.Add("X-Test-RoleId", "1");
        client.DefaultRequestHeaders.Add("X-OrganizationId", "1");

        var response = await client.DeleteAsync(url);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task ProductionExceptionHandling_ShouldHideInternalExceptionDetails_AndReturnCorrelationId()
    {
        await using var factory = new TestWebApplicationFactory(
            overrideServices: services =>
            {
                services.RemoveAll(typeof(IMoneyRegisterBalanceService));
                services.AddScoped<IMoneyRegisterBalanceService, ThrowingMoneyRegisterBalanceService>();
            });

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", "101");
        client.DefaultRequestHeaders.Add("X-Test-RoleId", "1");
        client.DefaultRequestHeaders.Add("X-OrganizationId", "1");

        var response = await client.GetAsync("/api/register/money-register-balances");
        var payload = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Contains("correlationId", payload);
        Assert.DoesNotContain("sensitive-stack-detail", payload, StringComparison.Ordinal);
        Assert.True(response.Headers.Contains(CorrelationIdMiddleware.HeaderName));
    }

    [Fact]
    public async Task ConfigurationBinding_ShouldLoadSecretBackedSettings()
    {
        var configuration = new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = "Host=test;Port=5432;Database=erp;Username=user;Password=pass",
            ["Jwt:Key"] = "abcdefghijklmnopqrstuvwxyz123456",
            ["Jwt:Issuer"] = "IssuerA",
            ["Jwt:Audience"] = "AudienceA",
            ["Jwt:AccessTokenExpirationMinutes"] = "15",
            ["Jwt:RefreshTokenExpirationDays"] = "30",
            ["Jwt:ValidateIssuer"] = "true",
            ["Jwt:ValidateAudience"] = "true",
            ["Jwt:ValidateLifetime"] = "true",
            ["Jwt:ValidateIssuerSigningKey"] = "true",
            ["Jwt:ClockSkewSeconds"] = "5",
            ["BackupJob:EnableEmailSend"] = "false",
            ["BackupJob:PgDumpPath"] = "pg_dump",
            ["BackupJob:Database:Host"] = "db-host",
            ["BackupJob:Database:Name"] = "erp",
            ["BackupJob:Database:User"] = "backup-user",
            ["BackupJob:Database:Password"] = "backup-pass",
            ["GoogleDrive:CredentialsPath"] = "/secure/google.json",
            ["GoogleDrive:BackupFolderId"] = "folder-123",
            ["FakturaAuthSettings:GrantType"] = "password",
            ["FakturaAuthSettings:Username"] = "f-user",
            ["FakturaAuthSettings:Password"] = "f-pass",
            ["FakturaAuthSettings:ClientId"] = "client-id",
            ["FakturaAuthSettings:ClientSecret"] = "client-secret"
        };

        await using var factory = new TestWebApplicationFactory(configuration: configuration);
        using var scope = factory.Services.CreateScope();

        var jwt = scope.ServiceProvider.GetRequiredService<IOptions<JwtOptions>>().Value;
        var google = scope.ServiceProvider.GetRequiredService<IOptions<GoogleDriveSettings>>().Value;
        var faktura = scope.ServiceProvider.GetRequiredService<IOptions<FakturaAuthSettings>>().Value;

        Assert.Equal("IssuerA", jwt.Issuer);
        Assert.Equal("/secure/google.json", google.CredentialsPath);
        Assert.Equal("client-secret", faktura.ClientSecret);
    }

    [Fact]
    public async Task OrganizationScopeSaveChanges_ShouldFailClosedForUserWithoutOrganizations()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        await using var db = new AppDbContext(options);
        db.SetUserContext(new FakeUserContext
        {
            Id = 1,
            RoleId = 1,
            AllowedOrganizationIds = [],
            HasGlobalAccess = false
        });

        db.MoneyRegisterBalances.Add(new MoneyRegisterBalance
        {
            Id = 5,
            OrganizationId = 1,
            DocumentTypeId = DocumentTypeIdConst.CASHOPERATION,
            DocumentId = 10,
            SourceType = "CASH_BOX",
            SourceId = 1,
            OperationTypeId = 1,
            CurrencyId = 1,
            Amount = 10,
            DocDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task OrganizationScopeSaveChanges_ShouldAllowGlobalUser()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        await using var db = new AppDbContext(options);
        db.SetUserContext(new FakeUserContext
        {
            Id = 1,
            RoleId = 1,
            AllowedOrganizationIds = [],
            HasGlobalAccess = true
        });

        db.MoneyRegisterBalances.Add(new MoneyRegisterBalance
        {
            Id = 6,
            OrganizationId = 2,
            DocumentTypeId = DocumentTypeIdConst.CASHOPERATION,
            DocumentId = 11,
            SourceType = "CASH_BOX",
            SourceId = 1,
            OperationTypeId = 1,
            CurrencyId = 1,
            Amount = 10,
            DocDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        Assert.Equal(1, await db.MoneyRegisterBalances.CountAsync());
    }

    [Fact]
    public async Task OrganizationScopeSaveChanges_ShouldAllowAssignedOrganizationOnly()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        await using var db = new AppDbContext(options);
        db.SetUserContext(new FakeUserContext
        {
            Id = 1,
            RoleId = 1,
            AllowedOrganizationIds = [1],
            HasGlobalAccess = false
        });

        db.MoneyRegisterBalances.Add(new MoneyRegisterBalance
        {
            Id = 7,
            OrganizationId = 1,
            DocumentTypeId = DocumentTypeIdConst.CASHOPERATION,
            DocumentId = 12,
            SourceType = "CASH_BOX",
            SourceId = 1,
            OperationTypeId = 1,
            CurrencyId = 1,
            Amount = 10,
            DocDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        db.MoneyRegisterBalances.Add(new MoneyRegisterBalance
        {
            Id = 8,
            OrganizationId = 2,
            DocumentTypeId = DocumentTypeIdConst.CASHOPERATION,
            DocumentId = 13,
            SourceType = "CASH_BOX",
            SourceId = 1,
            OperationTypeId = 1,
            CurrencyId = 1,
            Amount = 10,
            DocDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
    }
}
