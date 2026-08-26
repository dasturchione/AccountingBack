using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace UnitTests;

public sealed class BankOperationClassificationModelTests
{
    [Fact]
    public void Domain_ContainsClassificationEntitiesWithExpectedTablesAndNoIndexAttributes()
    {
        var expectedTables = new Dictionary<string, string>
        {
            ["BankOperationCategory"] = "cmn_bank_operation_category",
            ["BankOperationCategoryTranslation"] = "cmn_bank_operation_category_translation",
            ["BankOperationClassificationRuleSet"] = "cmn_bank_operation_classification_rule_set",
            ["BankOperationClassificationRule"] = "cmn_bank_operation_classification_rule",
            ["BankOperationClassificationCondition"] = "cmn_bank_operation_classification_condition",
            ["BankOperationClassificationConditionValue"] = "cmn_bank_operation_classification_condition_value"
        };

        foreach (var (typeName, tableName) in expectedTables)
        {
            var clrType = typeof(BankOperation).Assembly.GetType($"Domain.Entities.{typeName}");

            Assert.NotNull(clrType);
            Assert.Equal(tableName, clrType.GetCustomAttributes(typeof(TableAttribute), false)
                .Cast<TableAttribute>()
                .Single()
                .Name);
            Assert.DoesNotContain(
                clrType.GetCustomAttributes(inherit: false),
                attribute => attribute is IndexAttribute);
        }
    }

    [Fact]
    public void BankOperation_MapsNullableClassificationForeignKeys()
    {
        AssertMappedNullableProperty(nameof(BankOperation.ClassificationCategoryId), "classification_category_id", typeof(short?));
        AssertMappedNullableProperty(nameof(BankOperation.ClassificationRuleId), "classification_rule_id", typeof(int?));
    }

    private static void AssertMappedNullableProperty(string propertyName, string columnName, Type propertyType)
    {
        var property = typeof(BankOperation).GetProperty(propertyName);

        Assert.NotNull(property);
        Assert.Equal(propertyType, property.PropertyType);
        Assert.Equal(columnName, property.GetCustomAttributes(typeof(ColumnAttribute), false)
            .Cast<ColumnAttribute>()
            .Single()
            .Name);
    }
}
