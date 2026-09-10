namespace IntegrationTests;

public sealed class PayrollAccountRemovalSchemaContractTests
{
    [Fact]
    public void Migration_DropsUnusedPayrollDocumentAccountColumns()
    {
        var sql = ReadEmbedded("1620_remove_unused_payroll_account_fields.sql");

        Assert.Contains("deduction_payable_account_id", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("employer_tax_expense_account_id", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("employer_tax_payable_account_id", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("advance_receivable_account_id", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("drop column if exists", sql, StringComparison.OrdinalIgnoreCase);
    }

    private static string ReadEmbedded(string suffix)
    {
        var assembly = typeof(PayrollAccountRemovalSchemaContractTests).Assembly;
        var name = assembly.GetManifestResourceNames().Single(x => x.EndsWith(suffix));
        using var stream = assembly.GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
