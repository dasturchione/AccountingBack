using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace UnitTests;

public sealed class OpeningInventoryModelTests
{
    [Fact]
    public void EfModel_ContainsOpeningInventoryAndSourceTrackedOpeningBalanceDetails()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=model_validation;Username=model_validation;Password=model_validation")
            .Options;

        using var context = new AppDbContext(options);

        Assert.NotNull(context.Model.FindEntityType(typeof(OpeningInventory)));
        var detail = context.Model.FindEntityType(typeof(OpeningBalanceAccountDetail));
        Assert.NotNull(detail);
        Assert.Equal(
            "source_document_type_id",
            detail.FindProperty(nameof(OpeningBalanceAccountDetail.SourceDocumentTypeId))?.GetColumnName());
        Assert.Equal(
            "source_document_id",
            detail.FindProperty(nameof(OpeningBalanceAccountDetail.SourceDocumentId))?.GetColumnName());
        Assert.Equal(
            "source_line_id",
            detail.FindProperty(nameof(OpeningBalanceAccountDetail.SourceLineId))?.GetColumnName());
    }
}
