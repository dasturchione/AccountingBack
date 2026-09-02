using Application.Features.PricingConditions;

namespace UnitTests;

public sealed class PricingConditionListFilterValidatorTests
{
    private readonly PricingConditionListFilterValidator _validator = new();

    [Fact]
    public void DefaultFilter_IsValid()
    {
        Assert.True(_validator.Validate(new PricingConditionListFilter()).IsValid);
    }

    [Theory]
    [InlineData(0, null, null, null, null, null)]
    [InlineData(1, 0, null, null, null, null)]
    [InlineData(1, null, 0, null, null, null)]
    [InlineData(1, null, null, 0, null, null)]
    [InlineData(1, null, null, null, 0, null)]
    [InlineData(1, null, null, null, null, "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx")]
    public void InvalidBounds_AreRejected(
        int page,
        int? pageSize,
        int? pricingMethodId,
        int? roundingMethodId,
        int? stateId,
        string? search)
    {
        var result = _validator.Validate(new PricingConditionListFilter
        {
            Page = page,
            PageSize = pageSize,
            PricingMethodId = (short?)pricingMethodId,
            RoundingMethodId = (short?)roundingMethodId,
            StateId = (short?)stateId,
            Search = search
        });

        Assert.False(result.IsValid);
    }
}
