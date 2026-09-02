using Application.Features.Cmn.CurrencyRevaluations;

namespace UnitTests;

public sealed class CurrencyRevaluationListFilterValidatorTests
{
    private readonly CurrencyRevaluationListFilterValidator _validator = new();

    [Fact]
    public void DefaultFilter_IsValid()
    {
        Assert.True(_validator.Validate(new CurrencyRevaluationListFilter()).IsValid);
    }

    [Theory]
    [InlineData(0, null, null, null)]
    [InlineData(1, 0, null, null)]
    [InlineData(1, null, "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", null)]
    [InlineData(1, null, null, "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx")]
    public void InvalidBounds_AreRejected(int page, int? pageSize, string? search, string? sortBy)
    {
        var result = _validator.Validate(new CurrencyRevaluationListFilter
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            SortBy = sortBy
        });

        Assert.False(result.IsValid);
    }
}
