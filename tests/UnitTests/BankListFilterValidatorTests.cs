using Application.Features.Banks;

namespace UnitTests;

public sealed class BankListFilterValidatorTests
{
    private readonly BankListFilterValidator _validator = new();

    [Fact]
    public void DefaultFilter_IsValid()
    {
        var result = _validator.Validate(new BankListFilter());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0, null, null, null)]
    [InlineData(1, 0, null, null)]
    [InlineData(1, null, 0, null)]
    [InlineData(1, null, null, "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx")]
    public void InvalidBounds_AreRejected(int page, int? pageSize, int? stateId, string? search)
    {
        var result = _validator.Validate(new BankListFilter
        {
            Page = page,
            PageSize = pageSize,
            StateId = (short?)stateId,
            Search = search
        });

        Assert.False(result.IsValid);
    }
}
