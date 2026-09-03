using Application.Features.Organizations;

namespace UnitTests;

public sealed class OrganizationListFilterValidatorTests
{
    private readonly OrganizationListFilterValidator _validator = new();

    [Fact]
    public void DefaultFilter_IsValid()
    {
        Assert.True(_validator.Validate(new OrganizationListFilter()).IsValid);
    }

    [Theory]
    [InlineData(0, null, null, null)]
    [InlineData(1, 0, null, null)]
    [InlineData(1, null, 0, null)]
    [InlineData(1, null, null, "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx")]
    public void InvalidBounds_AreRejected(int page, int? pageSize, int? regionId, string? search)
    {
        var result = _validator.Validate(new OrganizationListFilter
        {
            Page = page,
            PageSize = pageSize,
            RegionId = regionId,
            Search = search
        });

        Assert.False(result.IsValid);
    }
}
