using Application.Abstractions.Authentication;
using Domain.Entities;
using Infrastructure.Context;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests;

public sealed class DomainParityTests
{
    [Fact]
    public void InvRegBalance_runtime_mapping_matches_schema()
    {
        using var context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(InvRegBalance));

        Assert.NotNull(entity);
        Assert.Equal("inv_reg_balance", entity!.GetTableName());
        Assert.Equal("Id", entity.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal(typeof(decimal), entity.FindProperty(nameof(InvRegBalance.Quantity))!.ClrType);
        Assert.Equal(typeof(decimal), entity.FindProperty(nameof(InvRegBalance.Amount))!.ClrType);
        Assert.Equal(18, entity.FindProperty(nameof(InvRegBalance.Quantity))!.GetPrecision());
        Assert.Equal(3, entity.FindProperty(nameof(InvRegBalance.Quantity))!.GetScale());
        Assert.Equal(18, entity.FindProperty(nameof(InvRegBalance.Amount))!.GetPrecision());
        Assert.Equal(2, entity.FindProperty(nameof(InvRegBalance.Amount))!.GetScale());
        Assert.Contains(entity.GetForeignKeys(), foreignKey => foreignKey.Properties.Single().Name == nameof(InvRegBalance.OrganizationId));
        Assert.Contains(entity.GetForeignKeys(), foreignKey => foreignKey.Properties.Single().Name == nameof(InvRegBalance.ProductId));
        Assert.Contains(entity.GetForeignKeys(), foreignKey => foreignKey.Properties.Single().Name == nameof(InvRegBalance.WarehouseId));
    }

    [Fact]
    public async Task InvRegBalance_query_is_organization_scoped()
    {
        var databaseName = $"domain-parity-{Guid.NewGuid():N}";
        await using (var seedContext = CreateContext(databaseName, setUserContext: false))
        {
            seedContext.InvRegBalances.AddRange(Row(1, 11), Row(2, 12));
            await seedContext.SaveChangesAsync();
        }

        await using var context = CreateContext(databaseName);

        var rows = await context.InvRegBalances.OrderBy(row => row.Id).ToListAsync();

        Assert.Single(rows);
        Assert.Equal(11, rows[0].OrganizationId);
    }

    [Fact]
    public async Task InvRegBalance_background_scope_is_bounded_to_persisted_organization()
    {
        var backgroundScope = new BackgroundOrganizationScope();
        var databaseName = $"domain-parity-{Guid.NewGuid():N}";
        await using (var seedContext = CreateContext(databaseName, setUserContext: false))
        {
            seedContext.InvRegBalances.AddRange(Row(1, 11), Row(2, 12));
            await seedContext.SaveChangesAsync();
        }

        await using var context = CreateContext(databaseName, backgroundScope);

        using (backgroundScope.Enter(12, "domain-parity-test"))
        {
            var rows = await context.InvRegBalances.ToListAsync();
            Assert.Single(rows);
            Assert.Equal(12, rows[0].OrganizationId);
        }

        Assert.False(backgroundScope.IsActive);
    }

    [Fact]
    public async Task InvRegBalance_cross_organization_write_is_rejected()
    {
        await using var context = CreateContext();
        context.InvRegBalances.Add(Row(1, 12));

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public void TenantId_remains_mandatory_until_database_invariant_is_hardened()
    {
        Assert.Equal(typeof(int), typeof(Organization).GetProperty(nameof(Organization.TenantId))!.PropertyType);
    }

    [Fact]
    public void Tenant_hardening_migration_fails_closed_without_repairing_null_rows()
    {
        var migration = ReadSchemaContract("0147_harden_org_organization_tenant_id.sql");

        Assert.Contains("where tenant_id is null", migration, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("raise exception", migration, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("alter column tenant_id set not null", migration, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("update public.org_organization", migration, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("default", migration, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Tenant_contract_verification_checks_not_null_and_foreign_key()
    {
        var verification = ReadSchemaContract("1527_verify_org_organization_tenant_id_contract.sql");

        Assert.Contains("is_nullable", verification, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("tenant_foreign_key_exists", verification, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("platform_tenant", verification, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("raise exception", verification, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AvailableQuantity_remains_non_nullable_calculated_contract()
    {
        Assert.Equal(typeof(decimal), typeof(WarehouseProduct).GetProperty(nameof(WarehouseProduct.AvailableQuantity))!.PropertyType);
    }

    [Fact]
    public async Task Tenant_admin_sees_only_organizations_owned_by_their_tenant()
    {
        var databaseName = $"domain-parity-{Guid.NewGuid():N}";
        await using (var seedContext = CreateContext(databaseName, setUserContext: false))
        {
            seedContext.Organizations.AddRange(OrganizationRow(11, 1), OrganizationRow(12, 2), OrganizationRow(13, 1));
            await seedContext.SaveChangesAsync();
        }

        await using var context = CreateContext(
            databaseName,
            userContext: new TestUserContext(CurrentUserKind.TenantAdmin, 1, null));

        var ids = await context.Organizations.Select(organization => organization.Id).ToListAsync();

        Assert.Equal([11, 13], ids.OrderBy(id => id));
    }

    [Fact]
    public async Task Super_admin_can_see_organizations_across_tenants()
    {
        var databaseName = $"domain-parity-{Guid.NewGuid():N}";
        await using (var seedContext = CreateContext(databaseName, setUserContext: false))
        {
            seedContext.Organizations.AddRange(OrganizationRow(11, 1), OrganizationRow(12, 2));
            await seedContext.SaveChangesAsync();
        }

        await using var context = CreateContext(
            databaseName,
            userContext: new TestUserContext(CurrentUserKind.SuperAdmin, null, null));

        var ids = await context.Organizations.Select(organization => organization.Id).ToListAsync();

        Assert.Equal([11, 12], ids.OrderBy(id => id));
    }

    [Fact]
    public async Task Background_organization_scope_is_applied_to_organization_reads()
    {
        var databaseName = $"domain-parity-{Guid.NewGuid():N}";
        await using (var seedContext = CreateContext(databaseName, setUserContext: false))
        {
            seedContext.Organizations.AddRange(OrganizationRow(11, 1), OrganizationRow(12, 2));
            await seedContext.SaveChangesAsync();
        }

        var backgroundScope = new BackgroundOrganizationScope();
        await using var context = CreateContext(databaseName, backgroundScope);
        using (backgroundScope.Enter(12, "domain-parity-organization-test"))
        {
            var ids = await context.Organizations.Select(organization => organization.Id).ToListAsync();
            Assert.Equal([12], ids);
        }
    }

    private static InvRegBalance Row(long id, int organizationId) => new()
    {
        Id = id,
        OrganizationId = organizationId,
        DocumentTypeId = 1,
        DocumentId = id,
        WarehouseId = 1,
        ProductId = 1,
        OperationTypeId = 1,
        Quantity = 1m,
        Amount = 1m,
        DocDate = new DateTime(2026, 1, 1),
        CreatedDate = new DateTime(2026, 1, 1)
    };

    private static Organization OrganizationRow(int id, int tenantId) => new()
    {
        Id = id,
        ShortName = $"Organization {id}",
        FullName = $"Organization {id}",
        Inn = $"{id:000000000}",
        RegionId = 1,
        StateId = 1,
        TenantId = tenantId,
        CreatedDate = new DateTime(2026, 1, 1),
        SetupStatus = "not_started"
    };

    private static AppDbContext CreateContext(
        string? databaseName = null,
        IBackgroundOrganizationScope? backgroundScope = null,
        bool setUserContext = true,
        IUserContext? userContext = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName ?? $"domain-parity-{Guid.NewGuid():N}")
            .Options;
        var context = new AppDbContext(options, backgroundScope);
        if (setUserContext)
            context.SetUserContext(userContext ?? new TestUserContext());
        return context;
    }

    private static string ReadSchemaContract(string fileName)
    {
        var resourceName = typeof(DomainParityTests).Assembly
            .GetManifestResourceNames()
            .Single(name => name.EndsWith(fileName.Replace('\\', '.'), StringComparison.OrdinalIgnoreCase));
        using var stream = typeof(DomainParityTests).Assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private sealed class TestUserContext(
        CurrentUserKind userKind = CurrentUserKind.TenantUser,
        int? tenantId = 1,
        int? organizationId = 11) : IUserContext
    {
        public int? Id => 7;
        public int? RoleId => 1;
        public CurrentUserKind UserKind => userKind;
        public short? LanguageId => 1;
        public int? TenantId => tenantId;
        public int? OrganizationId => organizationId;
        public List<int> AllowedOrganizationIds => organizationId is { } id ? [id] : [];
        public int? BranchId => null;
    }
}
