using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace UnitTests;

public sealed class LegacyInventoryRegisterRemovalTests
{
    [Fact]
    public void AppDbContext_DoesNotMapInvRegBalance()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=model_metadata_only")
            .Options;

        using var context = new AppDbContext(options);

        Assert.DoesNotContain(
            context.Model.GetEntityTypes(),
            entityType => entityType.GetTableName() == "inv_reg_balance");
    }
}
