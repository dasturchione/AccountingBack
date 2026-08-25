using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

public sealed class InventoryAdjustmentDirectionModelContractTests
{
    [Fact]
    public void InventoryAdjustmentDocumentMapsDirectionIdToRequiredColumn()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"inventory-adjustment-direction-{Guid.NewGuid():N}")
            .Options;

        using var context = new AppDbContext(options);
        var entity = context.Model.FindEntityType(typeof(InventoryAdjustmentDoc));
        var property = entity?.FindProperty(nameof(InventoryAdjustmentDoc.DirectionId));

        Assert.NotNull(property);
        var column = property!.PropertyInfo?.GetCustomAttributes(typeof(ColumnAttribute), inherit: true)
            .OfType<ColumnAttribute>()
            .SingleOrDefault();

        Assert.NotNull(column);
        Assert.Equal("direction_id", column!.Name);
        Assert.False(property.IsNullable);

        var direction = context.Model.FindEntityType(typeof(MovementDirection));
        Assert.NotNull(direction);
        var table = typeof(MovementDirection).GetCustomAttributes(typeof(TableAttribute), inherit: true)
            .OfType<TableAttribute>()
            .SingleOrDefault();
        Assert.NotNull(table);
        Assert.Equal("cmn_movement_direction", table!.Name);
        Assert.Contains(entity!.GetForeignKeys(), foreignKey =>
            foreignKey.Properties.Select(x => x.Name).SequenceEqual([nameof(InventoryAdjustmentDoc.DirectionId)]) &&
            foreignKey.PrincipalEntityType.ClrType == typeof(MovementDirection));
    }
}
