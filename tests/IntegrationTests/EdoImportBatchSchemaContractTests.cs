using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

public sealed class EdoImportBatchSchemaContractTests
{
    [Fact]
    public void CreateScriptDefinesEdocsAndOrganizationScopedBatchDocumentIdentity()
    {
        var sql = ReadResource("1529_create_edo_import_batch.sql");

        Assert.Contains("check (provider_code = 'EDOCS')", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("check (direction in ('INBOX', 'OUTBOX'))", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("document_type in ('FACTURA', 'WAYBILL_LOCAL')", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("WAITING_FOR_SIGNATURE", sql, StringComparison.Ordinal);
        Assert.Contains("sent_override_applied", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ALREADY_IMPORTED", sql, StringComparison.Ordinal);
        Assert.Contains("foreign key (batch_id, organization_id, provider_code)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("foreign key (organization_id, edo_document_id)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("foreign key (organization_id, purchase_document_id)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("foreign key (organization_id, sale_document_id)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("unique (organization_id, batch_id, provider_document_id)", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CreateScriptRejectsPurchaseAndSaleLinkConflict()
    {
        var sql = ReadResource("1529_create_edo_import_batch.sql");

        Assert.Contains("num_nonnulls(purchase_document_id, sale_document_id) = 1", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("status not in ('IMPORTED', 'ALREADY_IMPORTED')", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CreateScriptDoesNotPersistRawMarkingOrProviderPayload()
    {
        var sql = ReadResource("1529_create_edo_import_batch.sql");

        Assert.DoesNotContain("marking_code", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("marking_number", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("raw_json", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("provider_json", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void VerificationScriptIsFailClosedAndRollbackRefusesExistingData()
    {
        var verification = ReadResource("1530_verify_edo_import_batch_schema.sql");
        var rollback = ReadResource("1531_rollback_edo_import_batch.sql");

        Assert.Contains("raise exception", verification, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("raise exception", rollback, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("batch documents exist", rollback, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("batches exist", rollback, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("cascade", rollback, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void WaybillSentOverrideUpgradeAndRollbackAreFailClosed()
    {
        var upgrade = ReadResource("1532_extend_edo_import_batch_waybill_sent_override.sql");
        var rollback = ReadResource("1533_rollback_edo_import_batch_waybill_sent_override.sql");

        Assert.Contains("sent_override_applied", upgrade, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("WAYBILL_LOCAL", upgrade, StringComparison.Ordinal);
        Assert.Contains("raise exception", upgrade, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("sent_override_applied = true", rollback, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("WAYBILL_LOCAL", rollback, StringComparison.Ordinal);
        Assert.Contains("raise exception", rollback, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EfModelContainsOrganizationScopedBatchRelationships()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"edo-batch-schema-{Guid.NewGuid():N}")
            .Options;

        using var context = new AppDbContext(options);
        var batch = context.Model.FindEntityType(typeof(Domain.Entities.EdoImportBatch));
        var document = context.Model.FindEntityType(typeof(Domain.Entities.EdoImportBatchDocument));

        Assert.NotNull(batch);
        Assert.NotNull(document);
        Assert.Equal("edo_import_batch", batch!.GetTableName());
        Assert.Equal("edo_import_batch_document", document!.GetTableName());
        Assert.Contains(document.GetForeignKeys(), foreignKey =>
            foreignKey.Properties.Select(property => property.Name)
                .SequenceEqual(["BatchId", "OrganizationId", "ProviderCode"]));
        Assert.Contains(document.GetForeignKeys(), foreignKey =>
            foreignKey.Properties.Select(property => property.Name)
                .SequenceEqual(["OrganizationId", "PurchaseDocumentId"]));
        Assert.Contains(document.GetForeignKeys(), foreignKey =>
            foreignKey.Properties.Select(property => property.Name)
                .SequenceEqual(["OrganizationId", "SaleDocumentId"]));
    }

    private static string ReadResource(string suffix)
    {
        var assembly = typeof(EdoImportBatchSchemaContractTests).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .Single(name => name.EndsWith(suffix, StringComparison.Ordinal));

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
