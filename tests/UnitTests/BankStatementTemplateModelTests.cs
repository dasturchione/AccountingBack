using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace UnitTests;

public sealed class BankStatementTemplateModelTests
{
    [Fact]
    public void AppDbContext_MapsBankStatementTemplateAggregateWithoutIndexAttributes()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=model_metadata_only")
            .Options;

        using var context = new AppDbContext(options);
        var expectedTables = new Dictionary<Type, string>
        {
            [typeof(BankStatementTemplate)] = "cmn_bank_statement_template",
            [typeof(BankStatementTemplateHeaderRule)] = "cmn_bank_statement_template_header_rule",
            [typeof(BankStatementTemplateRowRule)] = "cmn_bank_statement_template_row_rule",
            [typeof(BankStatementTemplateField)] = "cmn_bank_statement_template_field"
        };

        foreach (var (clrType, tableName) in expectedTables)
        {
            var entityType = context.Model.FindEntityType(clrType);
            Assert.NotNull(entityType);
            Assert.Equal(tableName, entityType.GetTableName());
            Assert.DoesNotContain(
                clrType.GetCustomAttributes(inherit: false),
                attribute => attribute is IndexAttribute);
        }

        AssertTemplateForeignKey<BankStatementTemplateHeaderRule>(context.Model);
        AssertTemplateForeignKey<BankStatementTemplateRowRule>(context.Model);
        AssertTemplateForeignKey<BankStatementTemplateField>(context.Model);
    }

    private static void AssertTemplateForeignKey<TChild>(IModel model)
    {
        var child = model.FindEntityType(typeof(TChild));
        Assert.NotNull(child);

        var foreignKey = Assert.Single(
            child.GetForeignKeys(),
            key => key.PrincipalEntityType.ClrType == typeof(BankStatementTemplate));

        Assert.Equal(nameof(BankStatementTemplate.Id), Assert.Single(foreignKey.PrincipalKey.Properties).Name);
    }
}
