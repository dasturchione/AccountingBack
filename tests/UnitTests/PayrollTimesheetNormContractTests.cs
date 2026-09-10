namespace UnitTests;

public sealed class PayrollTimesheetNormContractTests
{
    [Fact]
    public void TimesheetCreation_DoesNotRejectValuesAbovePeriodNorms()
    {
        var source = ReadEmbedded("PayrollTimesheetService.cs");

        Assert.DoesNotContain("TimesheetDaysExceeded", source, StringComparison.Ordinal);
        Assert.DoesNotContain("TimesheetHoursExceeded", source, StringComparison.Ordinal);
    }

    private static string ReadEmbedded(string suffix)
    {
        var assembly = typeof(PayrollTimesheetNormContractTests).Assembly;
        var name = assembly.GetManifestResourceNames().Single(x => x.EndsWith(suffix));
        using var stream = assembly.GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
