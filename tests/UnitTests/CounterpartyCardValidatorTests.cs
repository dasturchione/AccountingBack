using Application.Features.CounterpartyCards;

namespace UnitTests;

public sealed class CounterpartyCardValidatorTests
{
    [Fact]
    public void ListFilter_DefaultIsValidAndInvalidBoundsAreRejected()
    {
        var validator = new CounterpartyCardListFilterValidator();

        Assert.True(validator.Validate(new CounterpartyCardListFilter()).IsValid);
        Assert.False(validator.Validate(new CounterpartyCardListFilter { Page = 0 }).IsValid);
        Assert.False(validator.Validate(new CounterpartyCardListFilter { PageSize = 0 }).IsValid);
        Assert.False(validator.Validate(new CounterpartyCardListFilter { Search = new string('x', 101) }).IsValid);
    }

    [Fact]
    public void BaseValidator_MatchesDomainStringLengthsAndOptionalIdBounds()
    {
        var validator = new CounterpartyCardBaseDtoValidator();
        var valid = new CounterpartyCardBaseDto
        {
            ShortName = "Valid counterparty",
            Code = new string('c', 100),
            Email = $"{new string('e', 237)}@example.com",
            Address = new string('a', 1000),
            Oked = new string('o', 20),
            ExternalId = new string('x', 100),
            RegionId = 1,
            DistrictId = 1
        };

        Assert.True(validator.Validate(valid).IsValid);
        Assert.False(validator.Validate(Clone(valid, code: new string('c', 101))).IsValid);
        Assert.False(validator.Validate(Clone(valid, address: new string('a', 1001))).IsValid);
        Assert.False(validator.Validate(Clone(valid, oked: new string('o', 21))).IsValid);
        Assert.False(validator.Validate(Clone(valid, externalId: new string('x', 101))).IsValid);
        Assert.False(validator.Validate(Clone(valid, regionId: 0)).IsValid);
        Assert.False(validator.Validate(Clone(valid, districtId: 0)).IsValid);
    }

    private static CounterpartyCardBaseDto Clone(
        CounterpartyCardBaseDto source,
        string? code = null,
        string? address = null,
        string? oked = null,
        string? externalId = null,
        int? regionId = null,
        int? districtId = null) => new()
        {
            ShortName = source.ShortName,
            Code = code ?? source.Code,
            Email = source.Email,
            Address = address ?? source.Address,
            Oked = oked ?? source.Oked,
            ExternalId = externalId ?? source.ExternalId,
            RegionId = regionId ?? source.RegionId,
            DistrictId = districtId ?? source.DistrictId
        };
}
