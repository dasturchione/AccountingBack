using Application.Features.Positions;

namespace UnitTests;

public sealed class PositionListFilterValidatorTests
{
    private readonly PositionListFilterValidator _validator = new();

    [Fact]
    public void DefaultFilter_IsValid()
    {
        Assert.True(_validator.Validate(new PositionListFilter()).IsValid);
    }

    [Theory]
    [InlineData(0, null, null)]
    [InlineData(1, 0, null)]
    [InlineData(1, null, "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx")]
    public void InvalidBounds_AreRejected(int page, int? pageSize, string? search)
    {
        var result = _validator.Validate(new PositionListFilter
        {
            Page = page,
            PageSize = pageSize,
            Search = search
        });

        Assert.False(result.IsValid);
    }
}
