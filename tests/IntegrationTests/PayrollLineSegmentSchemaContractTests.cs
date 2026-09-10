namespace IntegrationTests;

public sealed class PayrollLineSegmentSchemaContractTests
{
    [Fact]
    public void Migration_DefinesImmutablePayrollInputSegments()
    {
        var sql = ReadEmbedded("1630_add_payroll_line_segments.sql");

        Assert.Contains("create table if not exists pay_payroll_line_segment", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("component_snapshot_json jsonb", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ux_pay_payroll_line_segment_line_start", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("segment_end_date >= segment_start_date", sql, StringComparison.OrdinalIgnoreCase);
    }

    private static string ReadEmbedded(string suffix)
    {
        var assembly = typeof(PayrollLineSegmentSchemaContractTests).Assembly;
        var name = assembly.GetManifestResourceNames().Single(x => x.EndsWith(suffix));
        using var stream = assembly.GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
