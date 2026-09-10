namespace IntegrationTests;

public sealed class PayrollCalendarSchemaContractTests
{
    [Fact]
    public void Migration_DefinesPayrollCalendarConstraints()
    {
        var sql = ReadEmbedded("1618_add_period_calendar_and_timesheet_days.sql");

        Assert.Contains("daily_work_hours numeric(8,4)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("create table pay_period_work_day", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("unique (period_id, work_date)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("create table pay_timesheet_line_day", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("worked_hours numeric(8,2)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("unique (timesheet_line_id, work_date)", sql, StringComparison.OrdinalIgnoreCase);
    }

    private static string ReadEmbedded(string suffix)
    {
        var assembly = typeof(PayrollCalendarSchemaContractTests).Assembly;
        var name = assembly.GetManifestResourceNames().Single(x => x.EndsWith(suffix));
        using var stream = assembly.GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
