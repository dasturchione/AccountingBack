using Application.Features.Contracts;

namespace UnitTests;

public sealed class ContractListFilterValidatorTests
{
    private readonly ContractListFilterValidator _validator = new();

    [Fact]
    public void DefaultFilter_IsValid()
    {
        var result = _validator.Validate(new ContractListFilter());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0, null, null, null, null)]
    [InlineData(1, 0, null, null, null)]
    [InlineData(1, null, 0, null, null)]
    [InlineData(1, null, null, 0, null)]
    [InlineData(1, null, null, null, "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx")]
    public void InvalidBounds_AreRejected(
        int page,
        int? pageSize,
        int? counterpartyId,
        int? contractTypeId,
        string? search)
    {
        var result = _validator.Validate(new ContractListFilter
        {
            Page = page,
            PageSize = pageSize,
            CounterpartyId = counterpartyId,
            ContractTypeId = (short?)contractTypeId,
            Search = search
        });

        Assert.False(result.IsValid);
    }
}
