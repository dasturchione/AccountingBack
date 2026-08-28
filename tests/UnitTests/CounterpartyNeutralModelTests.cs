using Application.Features.CounterpartyCards;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace UnitTests;

public sealed class CounterpartyNeutralModelTests
{
    [Fact]
    public void CreateValidator_AcceptsCounterpartyWithoutRoleClassification()
    {
        var dto = new CounterpartyCardCreateDto
        {
            ShortName = "Neutral counterparty"
        };

        var result = new CounterpartyCardCreateDtoValidator().Validate(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void AppDbContext_MapsCounterpartyWithoutTypeOrRoleFlags()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=model_metadata_only")
            .Options;

        using var context = new AppDbContext(options);
        var counterparty = context.Model.FindEntityType(typeof(CounterpartyCard));

        Assert.NotNull(counterparty);
        Assert.Null(counterparty.FindProperty("CounterpartyTypeId"));
        Assert.Null(counterparty.FindProperty("IsCustomer"));
        Assert.Null(counterparty.FindProperty("IsSupplier"));
        Assert.DoesNotContain(
            context.Model.GetEntityTypes(),
            entityType => entityType.GetTableName() is
                "cmn_counterparty_type" or "cmn_counterparty_type_translation");
    }
}
