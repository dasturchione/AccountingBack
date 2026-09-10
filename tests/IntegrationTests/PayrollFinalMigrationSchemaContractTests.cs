namespace IntegrationTests;

public sealed class PayrollFinalMigrationSchemaContractTests
{
    [Fact]
    public void Attendance_snapshot_migration_has_non_negative_breakdown_columns()
    {
        var sql = ReadEmbedded("1631_add_payroll_line_attendance_totals.sql");

        Assert.Contains("paid_leave_days numeric(6,2)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("overtime_hours numeric(8,2)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ck_pay_payroll_line_attendance_totals", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Formula_migration_has_dependency_and_amount_limits()
    {
        var sql = ReadEmbedded("1632_add_pay_component_formula_rules.sql");

        Assert.Contains("depends_on_component_id", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("minimum_amount", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("maximum_amount", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("is_taxable", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Migration_history_migration_is_idempotent()
    {
        var historySql = ReadEmbedded("1633_create_pay_migration_history.sql");

        Assert.Contains("create table if not exists pay_migration_history", historySql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("checksum_sha256", historySql, StringComparison.OrdinalIgnoreCase);
    }

    private static string ReadEmbedded(string suffix)
    {
        var assembly = typeof(PayrollFinalMigrationSchemaContractTests).Assembly;
        var name = assembly.GetManifestResourceNames().Single(x => x.EndsWith(suffix));
        using var stream = assembly.GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
