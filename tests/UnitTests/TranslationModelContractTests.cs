using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace UnitTests;

public sealed class TranslationModelContractTests
{
    private static readonly string[] ExpectedDedicatedTranslationTables =
    [
        "acc_account_type_translation",
        "acc_chart_account_preset_account_translation",
        "acc_chart_account_preset_translation",
        "acc_document_account_role_translation",
        "acc_document_account_type_translation",
        "acc_subkonto_type_translation",
        "cmn_bank_operation_category_translation",
        "cmn_contract_type_translation",
        "cmn_costing_method_translation",
        "cmn_currency_translation",
        "cmn_document_status_translation",
        "cmn_document_type_translation",
        "cmn_movement_direction_translation",
        "cmn_operation_type_translation",
        "cmn_payment_acceptance_point_type_translation",
        "cmn_payment_type_translation",
        "fa_disposal_type_translation",
        "fa_receipt_type_translation",
        "fiscal_cash_register_type_translation",
        "inv_product_group_translation",
        "rnt_rental_object_type_translation",
        "rtl_payment_method_translation",
        "sys_user_kind_translation"
    ];

    [Fact]
    public void DedicatedTranslationEntities_HaveCompositeKeyAndBothForeignKeys()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=model_metadata_only")
            .Options;

        using var context = new AppDbContext(options);
        var translations = context.Model.GetEntityTypes()
            .Where(IsDedicatedTranslation)
            .OrderBy(entityType => entityType.GetTableName())
            .ToArray();

        var actualTables = translations.Select(entityType => entityType.GetTableName()!).ToArray();
        Assert.True(
            ExpectedDedicatedTranslationTables.SequenceEqual(actualTables),
            "Dedicated translation model changed. Actual mappings:" + Environment.NewLine +
            string.Join(Environment.NewLine, translations.Select(Describe)));

        foreach (var translation in translations)
        {
            var languageProperty = translation.FindProperty("LanguageId");
            Assert.True(
                languageProperty is not null,
                $"{translation.GetTableName()} must map LanguageId.");

            var primaryKey = translation.FindPrimaryKey();
            Assert.True(
                primaryKey is not null && primaryKey.Properties.Count == 2,
                $"{translation.GetTableName()} must have a two-column primary key.");
            Assert.Contains(primaryKey!.Properties, property => property.Name == "LanguageId");
            Assert.DoesNotContain(
                primaryKey.Properties,
                property => property.IsShadowProperty());

            var foreignKeys = translation.GetForeignKeys().ToArray();
            Assert.Contains(
                foreignKeys,
                foreignKey => foreignKey.Properties.Any(property => property.Name == "LanguageId") &&
                              foreignKey.PrincipalEntityType.GetTableName() == "cmn_language" &&
                              foreignKey.DependentToPrincipal is not null);
            Assert.Contains(
                foreignKeys,
                foreignKey => foreignKey.Properties.All(property => property.Name != "LanguageId") &&
                              foreignKey.DependentToPrincipal is not null);
        }
    }

    private static string Describe(IReadOnlyEntityType entityType)
    {
        var key = string.Join(", ", entityType.FindPrimaryKey()!.Properties.Select(property => property.Name));
        var navigations = string.Join(", ", entityType.GetNavigations().Select(navigation => navigation.Name));
        return $"- {entityType.GetTableName()} => {entityType.ClrType.Name}; key: {key}; navigations: {navigations}";
    }

    private static bool IsDedicatedTranslation(IReadOnlyEntityType entityType)
    {
        var tableName = entityType.GetTableName();
        return tableName is not null &&
               tableName.EndsWith("_translation", StringComparison.Ordinal) &&
               tableName != "cmn_translation";
    }
}
