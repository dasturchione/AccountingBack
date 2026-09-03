using Application.Features.Cmn.CurrencyRevaluations;

namespace UnitTests;

public sealed class CurrencyRevaluationContractTests
{
    [Fact]
    public void PublicDtoProperties_AreStable()
    {
        AssertProperties<CurrencyRevaluationBaseDto>(
            ("ProviderRateDate", typeof(DateTime?)),
            ("RevaluationDate", typeof(DateTime)),
            ("RevalueAllForeignCurrencies", typeof(bool)),
            ("TargetCurrencyId", typeof(short?)));
        AssertProperties<CurrencyRevaluationCreateDto>(
            ("ProviderRateDate", typeof(DateTime?)),
            ("RevaluationDate", typeof(DateTime)),
            ("RevalueAllForeignCurrencies", typeof(bool)),
            ("TargetCurrencyId", typeof(short?)));
        AssertProperties<CurrencyRevaluationPreviewDto>(
            ("ProviderRateDate", typeof(DateTime?)),
            ("RevaluationDate", typeof(DateTime)),
            ("RevalueAllForeignCurrencies", typeof(bool)),
            ("TargetCurrencyId", typeof(short?)));
        AssertProperties<CurrencyRevaluationLineDto>(
            ("BalanceAmount", typeof(decimal)),
            ("BaseCurrencyId", typeof(short)),
            ("CurrentRate", typeof(decimal)),
            ("DifferenceAmount", typeof(decimal)),
            ("OpeningRate", typeof(decimal)),
            ("TargetCurrencyCode", typeof(string)),
            ("TargetCurrencyId", typeof(short)));
        AssertProperties<CurrencyRevaluationDto>(
            ("CancelledAt", typeof(DateTime?)),
            ("ConfirmedAt", typeof(DateTime?)),
            ("CreatedDate", typeof(DateTime)),
            ("Id", typeof(long)),
            ("Lines", typeof(List<CurrencyRevaluationLineDto>)),
            ("OrganizationId", typeof(int)),
            ("ProviderRateDate", typeof(DateTime?)),
            ("RevaluationDate", typeof(DateTime)),
            ("RevalueAllForeignCurrencies", typeof(bool)),
            ("StateId", typeof(short)),
            ("StatusId", typeof(short)),
            ("TargetCurrencyId", typeof(short?)));
        AssertProperties<CurrencyRevaluationListDto>(
            ("CancelledAt", typeof(DateTime?)),
            ("ConfirmedAt", typeof(DateTime?)),
            ("CreatedDate", typeof(DateTime)),
            ("Id", typeof(long)),
            ("Lines", typeof(List<CurrencyRevaluationLineDto>)),
            ("OrganizationId", typeof(int)),
            ("ProviderRateDate", typeof(DateTime?)),
            ("RevaluationDate", typeof(DateTime)),
            ("RevalueAllForeignCurrencies", typeof(bool)),
            ("StateId", typeof(short)),
            ("StatusId", typeof(short)),
            ("TargetCurrencyId", typeof(short?)));
    }

    [Fact]
    public void CreateAndPreviewValidators_PreserveBaseDateRules()
    {
        var invalidCreate = new CurrencyRevaluationCreateDto
        {
            RevaluationDate = new DateTime(2026, 9, 1),
            ProviderRateDate = new DateTime(2026, 9, 2)
        };
        var invalidPreview = new CurrencyRevaluationPreviewDto();

        Assert.False(new CurrencyRevaluationCreateDtoValidator().Validate(invalidCreate).IsValid);
        Assert.False(new CurrencyRevaluationPreviewDtoValidator().Validate(invalidPreview).IsValid);
        Assert.True(new CurrencyRevaluationCreateDtoValidator().Validate(new CurrencyRevaluationCreateDto
        {
            RevaluationDate = new DateTime(2026, 9, 2),
            ProviderRateDate = new DateTime(2026, 9, 1)
        }).IsValid);
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
