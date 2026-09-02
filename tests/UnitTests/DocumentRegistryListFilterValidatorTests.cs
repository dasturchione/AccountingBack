using Application.Features.Cmn.Documents;

namespace UnitTests;

public sealed class DocumentRegistryListFilterValidatorTests
{
    private readonly DocumentRegistryListFilterValidator _validator = new();

    [Fact]
    public void DefaultFilter_IsValid()
    {
        Assert.True(_validator.Validate(new DocumentRegistryListFilter()).IsValid);
    }

    [Theory]
    [InlineData(null, 0, null, null, null)]
    [InlineData(null, null, 0, null, null)]
    [InlineData(null, null, null, 0, null)]
    [InlineData("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", null, null, null, null)]
    [InlineData(null, null, null, null, "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx")]
    public void InvalidBounds_AreRejected(
        string? documentTypeCode,
        int? currencyId,
        int? statusId,
        int? stateId,
        string? search)
    {
        var result = _validator.Validate(new DocumentRegistryListFilter
        {
            DocumentTypeCode = documentTypeCode,
            CurrencyId = (short?)currencyId,
            StatusId = (short?)statusId,
            StateId = (short?)stateId,
            Search = search
        });

        Assert.False(result.IsValid);
    }
}
