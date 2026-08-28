using System.ComponentModel.DataAnnotations.Schema;
using Application.Features.PaymentAcceptancePointOperations;
using Application.Features.PaymentAcceptancePoints;
using Domain.Entities;
using SharedKernel.Constants;

namespace UnitTests;

public sealed class PaymentAcceptancePointTests
{
    [Fact]
    public void DomainModel_UsesApprovedTablesAndMinimalOperationFields()
    {
        var domainAssembly = typeof(BankOperation).Assembly;
        var pointType = domainAssembly.GetType("Domain.Entities.PaymentAcceptancePoint");
        var operationType = domainAssembly.GetType("Domain.Entities.PaymentAcceptancePointOperation");

        Assert.NotNull(pointType);
        Assert.NotNull(operationType);
        Assert.Equal("org_payment_acceptance_point", pointType!.GetCustomAttributes(typeof(TableAttribute), false).Cast<TableAttribute>().Single().Name);
        Assert.Equal("payment_acceptance_point_operation", operationType!.GetCustomAttributes(typeof(TableAttribute), false).Cast<TableAttribute>().Single().Name);
        Assert.NotNull(operationType.GetProperty("DirectionId"));
        Assert.Null(operationType.GetProperty("OperationTypeId"));
        Assert.NotNull(operationType.GetProperty("RelatedDocumentId"));
        Assert.NotNull(operationType.GetProperty("RelatedDocument"));
        Assert.Null(operationType.GetProperty("CommissionPercent"));
        Assert.Null(operationType.GetProperty("CommissionAmount"));
    }

    [Fact]
    public void OperationSql_HasDedicatedDraftPartialIndex()
    {
        var root = FindRepositoryRoot();
        var scripts = Directory.GetFiles(
            Path.Combine(root, "src", "Infrastructure", "Persistence", "Scripts"),
            "*.sql",
            SearchOption.AllDirectories);
        var operationScript = scripts.SingleOrDefault(path =>
            File.ReadAllText(path).Contains("create table payment_acceptance_point_operation", StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(operationScript);
        var sql = File.ReadAllText(operationScript!);
        Assert.Contains("ix_payment_acceptance_point_operation_draft", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("where status_id = 1", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("commission_percent", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("operation_type_id", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("related_document_id", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RetailPaymentConstraintSql_EnforcesPaymentPointAndOrganizationRules()
    {
        var root = FindRepositoryRoot();
        var migration = File.ReadAllText(Path.Combine(
            root,
            "src",
            "Infrastructure",
            "Persistence",
            "Scripts",
            "05_bank",
            "0510_add_payment_operation_document_link_and_retail_constraints.sql"));

        Assert.Contains("payment_method_code = 'CASH' and new.payment_acceptance_point_id is not null", migration, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("payment_method_code <> 'CASH' and new.payment_acceptance_point_id is null", migration, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("organization_id = new.organization_id", migration, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("state_id = 1", migration, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("id = new.related_document_id", migration, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("update of organization_id, payment_acceptance_point_id, related_document_id", migration, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ApiContracts_GenerateCodesAndDocumentNumbersOnlyOnBackend()
    {
        Assert.Null(typeof(PaymentAcceptancePointCreateDto).GetProperty("Code"));
        Assert.NotNull(typeof(PaymentAcceptancePointDto).GetProperty("Code"));
        Assert.Null(typeof(PaymentAcceptancePointOperationCreateDto).GetProperty("DocNumber"));
        Assert.NotNull(typeof(PaymentAcceptancePointOperationDto).GetProperty("DocNumber"));
        Assert.Equal((short)25, DocumentTypeIdConst.PAYMENT_ACCEPTANCE_POINT_OPERATION);
    }

    [Fact]
    public void Migration_PreservesTerminalIdsAndRetailPaymentLink()
    {
        var root = FindRepositoryRoot();
        var migration = File.ReadAllText(Path.Combine(
            root,
            "src",
            "Infrastructure",
            "Persistence",
            "Scripts",
            "05_bank",
            "0508_migrate_bank_terminal_to_payment_acceptance_point.sql"));

        Assert.Contains("alter table bank_terminal rename to org_payment_acceptance_point", migration, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("rename column bank_terminal_id to payment_acceptance_point_id", migration, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("delete from bank_terminal", migration, StringComparison.OrdinalIgnoreCase);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "src")))
            directory = directory.Parent;

        return directory?.FullName ?? throw new DirectoryNotFoundException("Repository root was not found.");
    }
}
