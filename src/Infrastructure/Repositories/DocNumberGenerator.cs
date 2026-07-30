using Application.Abstractions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class DocNumberGenerator(AppDbContext db) : IDocNumberGenerator
{
    // Bank/Sale/Purchase/etc: {PREFIX}-{YYYY}-{NNNNNN} (e.g. PUR-2024-000001)
    // CashOperation: 9-digit sequence (e.g. 100000001)
    public async Task<string> GenerateAsync(int organizationId, string prefix, DateTime docDate, CancellationToken ct = default)
    {
        if (prefix == "CASH")
        {
            var lastNumber = await db.Database
                .SqlQuery<int>($"""
                    SELECT COALESCE(MAX(
                        CASE WHEN doc_number ~ '^[0-9]+$'
                            THEN CAST(doc_number AS INTEGER)
                            ELSE 0
                        END
                    ), 100000000) AS "Value"
                    FROM cash_operation
                    WHERE organization_id = {organizationId}
                """)
                .FirstAsync(ct);

            return (lastNumber + 1).ToString("D9");
        }

        var year       = docDate.Year;
        var yearPrefix = $"{prefix}-{year}-";

        var lastNumberForPrefix = await db.Database
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
                    UNION ALL
                    SELECT doc_number FROM fa_receipt_doc WHERE organization_id = {organizationId}
                    UNION ALL
                    SELECT doc_number FROM fa_movement_doc WHERE organization_id = {organizationId}
                    UNION ALL
                    SELECT doc_number FROM fa_depreciation_run WHERE organization_id = {organizationId}
                    UNION ALL
                    SELECT doc_number FROM fa_disposal_doc WHERE organization_id = {organizationId}
                    UNION ALL
                    SELECT doc_number FROM fa_revaluation_doc WHERE organization_id = {organizationId}
                    UNION ALL
                    SELECT doc_number FROM pay_timesheet WHERE organization_id = {organizationId}
                    UNION ALL
                    SELECT doc_number FROM pay_payroll_doc WHERE organization_id = {organizationId}
                    UNION ALL
                    SELECT doc_number FROM pay_payment_batch WHERE organization_id = {organizationId}
                    UNION ALL
                    SELECT doc_number FROM inv_opening_inventory WHERE organization_id = {organizationId}
                ) docs
                """)
            .FirstAsync(ct);

        return $"{yearPrefix}{(lastNumberForPrefix + 1):D6}";
    }
}
