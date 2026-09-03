using Application.Features.Manual;

namespace UnitTests;

public sealed class ManualDtoContractTests
{
    [Fact]
    public void PublicDtoProperties_AreStable()
    {
        AssertProperties<SelectListDto>(
            ("Code", typeof(string)),
            ("Id", typeof(long)),
            ("Name", typeof(string)));
        AssertProperties<BankBranchSelectListDto>(
            ("BankId", typeof(int)),
            ("Code", typeof(string)),
            ("Id", typeof(long)),
            ("Mfo", typeof(string)),
            ("Name", typeof(string)));
        AssertProperties<ProductSelectListDto>(
            ("Code", typeof(string)),
            ("Id", typeof(long)),
            ("IsPieceTracked", typeof(bool)),
            ("IsPurchased", typeof(bool)),
            ("IsService", typeof(bool)),
            ("IsSold", typeof(bool)),
            ("Mxik", typeof(string)),
            ("Name", typeof(string)),
            ("UnitCode", typeof(string)),
            ("UnitId", typeof(short)));
        AssertProperties<ModuleSubGroupSelectListDto>(
            ("Code", typeof(string)),
            ("FullName", typeof(string)),
            ("Id", typeof(int)),
            ("Modules", typeof(List<ModuleSelectListDto>)),
            ("ShortName", typeof(string)));
        AssertProperties<ModuleSelectListDto>(
            ("Code", typeof(string)),
            ("FullName", typeof(string)),
            ("Id", typeof(int)),
            ("ShortName", typeof(string)));
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
