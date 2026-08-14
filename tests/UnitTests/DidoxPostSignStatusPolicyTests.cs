using System.Text.Json;
using Integration.Didox.Facturas;

namespace UnitTests;

public sealed class DidoxPostSignStatusPolicyTests
{
    [Theory]
    [InlineData("3", 3)]
    [InlineData("\"3\"", 3)]
    public void TryReadDidoxStatus_AcceptsStrictIntegerRepresentations(string json, int expected)
    {
        using var document = JsonDocument.Parse(json);

        var parsed = DidoxFacturaService.TryReadDidoxStatus(document.RootElement, out var actual);

        Assert.True(parsed);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("\"signed\"")]
    [InlineData("null")]
    [InlineData("{}")]
    public void TryReadDidoxStatus_RejectsNonIntegerValues(string json)
    {
        using var document = JsonDocument.Parse(json);

        var parsed = DidoxFacturaService.TryReadDidoxStatus(document.RootElement, out _);

        Assert.False(parsed);
    }
}
