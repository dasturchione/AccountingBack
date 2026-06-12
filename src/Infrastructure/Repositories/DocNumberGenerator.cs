using Application.Abstractions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class DocNumberGenerator(AppDbContext db) : IDocNumberGenerator
{
    // Format: {PREFIX}-{YYYY}-{NNNNNN}  e.g. PUR-2024-000001
    public async Task<string> GenerateAsync(int organizationId, string prefix, DateTime docDate, CancellationToken ct = default)
    {
        var year       = docDate.Year;
        var yearPrefix = $"{prefix}-{year}-";

        var lastNumber = await db.Database
            .SqlQuery<int>($"""
                SELECT COALESCE(MAX(
                    CASE WHEN doc_number LIKE {yearPrefix + "%"}
                    THEN CAST(SUBSTRING(doc_number FROM {yearPrefix.Length + 1}) AS INTEGER)
                    ELSE 0 END
                ), 0) AS "Value"
                FROM (
                    SELECT doc_number FROM pur_doc WHERE organization_id = {organizationId}
                    UNION ALL
                    SELECT doc_number FROM sale_doc WHERE organization_id = {organizationId}
                ) docs
                """)
            .FirstAsync(ct);

        return $"{yearPrefix}{(lastNumber + 1):D6}";
    }
}
