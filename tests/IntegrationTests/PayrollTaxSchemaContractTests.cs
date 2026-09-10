namespace IntegrationTests;

public sealed class PayrollTaxSchemaContractTests
{
    [Fact]
    public void Migration_DefinesEffectiveTaxDefinitionsAndPayrollTaxSnapshots()
    {
        var sql = ReadEmbedded("1627_add_payroll_tax_registry.sql");

        Assert.Contains("create table if not exists pay_tax_definition", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("tax_type", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("base_type", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("effective_from", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("create table if not exists pay_payroll_tax_line", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("taxable_base", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("liability_account_id", sql, StringComparison.OrdinalIgnoreCase);
    }

    private static string ReadEmbedded(string suffix)
    {
        var assembly = typeof(PayrollTaxSchemaContractTests).Assembly;
        var name = assembly.GetManifestResourceNames().Single(x => x.EndsWith(suffix));
        using var stream = assembly.GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
