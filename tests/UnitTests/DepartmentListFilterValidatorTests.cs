using Application.Features.Departments;

namespace UnitTests;

public sealed class DepartmentListFilterValidatorTests
{
    private readonly DepartmentListFilterValidator _validator = new();

    [Fact]
    public void DefaultFilter_IsValid()
    {
        Assert.True(_validator.Validate(new DepartmentListFilter()).IsValid);
    }

    [Theory]
    [InlineData(0, null, null, null)]
    [InlineData(1, 0, null, null)]
    [InlineData(1, null, 0, null)]
    [InlineData(1, null, null, "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx")]
    public void InvalidBounds_AreRejected(int page, int? pageSize, int? branchId, string? search)
    {
        var result = _validator.Validate(new DepartmentListFilter
        {
            Page = page,
            PageSize = pageSize,
            BranchId = branchId,
            Search = search
        });

        Assert.False(result.IsValid);
    }
}
