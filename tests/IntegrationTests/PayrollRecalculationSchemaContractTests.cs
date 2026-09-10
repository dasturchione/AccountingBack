namespace IntegrationTests;

public sealed class PayrollRecalculationSchemaContractTests
{
    [Fact]
    public void Migration_DefinesIdempotentRecalculationQueue()
    {
        var sql = ReadEmbedded("1628_add_payroll_recalculation.sql");

        Assert.Contains("create table if not exists pay_payroll_recalculation", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("payroll_doc_id", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("status in ('PENDING', 'PROCESSING', 'COMPLETED', 'FAILED')", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ux_pay_payroll_recalculation_active_doc", sql, StringComparison.OrdinalIgnoreCase);
    }

    private static string ReadEmbedded(string suffix)
    {
        var assembly = typeof(PayrollRecalculationSchemaContractTests).Assembly;
        var name = assembly.GetManifestResourceNames().Single(x => x.EndsWith(suffix));
        using var stream = assembly.GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
