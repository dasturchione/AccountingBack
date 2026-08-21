using Application.Features.SaleDocs.EdoSalePreflight;
using Domain.Entities;
using SharedKernel.Constants;

public sealed class EdoSaleCurrencySelectionPolicyTests
{
    [Fact]
    public void ProviderCurrencyMatchingSelectedActiveCurrencySucceeds()
    {
        var error = EdoSaleCurrencySelectionPolicy.Validate(Currency("UZS", StateIdConst.ACTIVE), "UZS");

        Assert.Null(error);
    }

    [Fact]
    public void ProviderCurrencyMismatchFails()
    {
        var error = EdoSaleCurrencySelectionPolicy.Validate(Currency("USD", StateIdConst.ACTIVE), "UZS");

        Assert.Equal("CURRENCY_MAPPING_INVALID", error);
    }

    [Fact]
    public void AbsentProviderCurrencyAcceptsOnlyExplicitActiveCurrency()
    {
        var activeError = EdoSaleCurrencySelectionPolicy.Validate(Currency("UZS", StateIdConst.ACTIVE), null);
        var inactiveError = EdoSaleCurrencySelectionPolicy.Validate(Currency("UZS", StateIdConst.PASSIVE), null);
        var missingError = EdoSaleCurrencySelectionPolicy.Validate(null, null);

        Assert.Null(activeError);
        Assert.Equal("CURRENCY_MAPPING_INVALID", inactiveError);
        Assert.Equal("CURRENCY_MAPPING_INVALID", missingError);
    }

    private static Currency Currency(string code, short stateId) => new()
    {
        Id = 1,
        Code = code,
        Name = code,
        StateId = stateId
    };
}
