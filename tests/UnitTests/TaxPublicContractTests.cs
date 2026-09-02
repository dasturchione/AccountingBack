using Application.Features.Cmn.Taxes.Integration.DTOs;

namespace UnitTests;

public sealed class TaxPublicContractTests
{
    [Fact]
    public void IntegrationDtoProperties_AreStable()
    {
        AssertProperties<TaxLookupRequestDto>(
            ("EffectiveDate", typeof(DateOnly?)),
            ("OrganizationId", typeof(int?)),
            ("ProviderCode", typeof(string)),
            ("Query", typeof(string)));
        AssertProperties<TaxDocumentRequestDto>(
            ("DocumentNumber", typeof(string)),
            ("ExternalDocumentId", typeof(string)),
            ("OrganizationId", typeof(int?)),
            ("Payload", typeof(string)),
            ("ProviderCode", typeof(string)));
        AssertProperties<TaxLookupItemDto>(
            ("Code", typeof(string)),
            ("Description", typeof(string)),
            ("IsActive", typeof(bool)),
            ("Metadata", typeof(IDictionary<string, string?>)),
            ("Name", typeof(string)));
        AssertProperties<TaxDocumentResultDto>(
            ("ExternalDocumentId", typeof(string)),
            ("IsSuccessful", typeof(bool)),
            ("Message", typeof(string)),
            ("Operation", typeof(string)),
            ("ProviderCode", typeof(string)),
            ("RequestedAt", typeof(DateTime)),
            ("StatusCode", typeof(string)),
            ("StatusName", typeof(string)));
    }

    private static void AssertProperties<T>(params (string Name, Type Type)[] expected)
    {
        var actual = typeof(T).GetProperties()
            .Select(property => (property.Name, property.PropertyType))
            .OrderBy(property => property.Name)
            .ToArray();
        var orderedExpected = expected.OrderBy(property => property.Name).ToArray();

        Assert.Equal(orderedExpected, actual);
    }
}
