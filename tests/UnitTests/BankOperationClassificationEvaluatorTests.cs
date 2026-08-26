using Application.Features.BankParsers;
using Domain.Entities;
using SharedKernel.Constants;

namespace UnitTests;

public sealed class BankOperationClassificationEvaluatorTests
{
    [Fact]
    public void Evaluate_UsesFirstMatchingRuleByPriorityAndDynamicOrganizationInn()
    {
        var transaction = new TransactionDto
        {
            CounterpartyInn = "311 444 422",
            CounterpartyName = "Начисленные проценты",
            Purpose = "Комиссия от суммы перевода",
            Credit = 100
        };
        var fallback = Rule(999, "COUNTERPARTY", "COUNTERPARTY", isFallback: true);
        var commission = Rule(10, "BANK_COMMISSION", "BANK_COMMISSION", MovementDirectionIdConst.OUT, conditions:
        [
            Condition(1, "COUNTERPARTY_INN", "EQUALS", valueSource: "ORGANIZATION_INN", normalization: "NORMALIZE_KEY"),
            Condition(2, "PURPOSE", "CONTAINS", "от суммы")
        ]);

        var result = BankOperationClassificationEvaluator.Evaluate(
            transaction,
            "311444422",
            [fallback, commission]);

        Assert.NotNull(result);
        Assert.Equal("BANK_COMMISSION", result.CategoryCode);
        Assert.Equal("BANK_COMMISSION", result.RuleCode);
        Assert.False(result.RequiresReview);
    }

    [Fact]
    public void Evaluate_RespectsDirectionAndFallsBack()
    {
        var transaction = new TransactionDto { Debit = 500, Purpose = "Комиссия от суммы" };
        var commission = Rule(10, "BANK_COMMISSION", "BANK_COMMISSION", MovementDirectionIdConst.OUT, conditions:
            Condition(1, "PURPOSE", "CONTAINS", "от суммы"));
        var fallback = Rule(999, "COUNTERPARTY", "COUNTERPARTY", isFallback: true);

        var result = BankOperationClassificationEvaluator.Evaluate(transaction, "311444422", [commission, fallback]);

        Assert.NotNull(result);
        Assert.Equal("COUNTERPARTY", result.CategoryCode);
    }

    [Fact]
    public void Evaluate_SupportsAnyOperatorsAndNormalization()
    {
        var transaction = new TransactionDto { Purpose = "  PUL   KARTASIGA o'tkazildi  ", Debit = 100 };
        var personalCard = Rule(70, "PERSONAL_CARD", "PERSONAL_CARD", conditions:
            Condition(1, "PURPOSE", "CONTAINS_ANY", normalization: "LOWER", values: ["kartasiga", "картасига"]));

        var result = BankOperationClassificationEvaluator.Evaluate(transaction, "311444422", [personalCard]);

        Assert.NotNull(result);
        Assert.Equal("PERSONAL_CARD", result.CategoryCode);
    }

    [Fact]
    public void Evaluate_UsesRequestedCategoryTranslation()
    {
        var rule = Rule(999, "COUNTERPARTY", "Counterparty", isFallback: true);
        rule.Category.Translations.Add(new BankOperationCategoryTranslation
        {
            LanguageId = 2,
            Name = "Контрагент"
        });

        var result = BankOperationClassificationEvaluator.Evaluate(new TransactionDto(), "311444422", [rule], 2);

        Assert.NotNull(result);
        Assert.Equal("Контрагент", result.CategoryName);
    }

    [Fact]
    public void Evaluate_ThrowsForAnyConditionWithoutValues()
    {
        var invalid = Rule(10, "BANK_SERVICE", "BANK_SERVICE", conditions:
            Condition(1, "PURPOSE", "CONTAINS_ANY"));

        var exception = Assert.Throws<BankOperationClassificationConfigurationException>(() =>
            BankOperationClassificationEvaluator.Evaluate(new TransactionDto(), "311444422", [invalid]));

        Assert.Contains("BANK_SERVICE", exception.Message);
    }

    [Fact]
    public void Evaluate_ThrowsForFallbackWithConditions()
    {
        var invalid = Rule(999, "COUNTERPARTY", "COUNTERPARTY", isFallback: true, conditions:
            Condition(1, "PURPOSE", "CONTAINS", "test"));

        Assert.Throws<BankOperationClassificationConfigurationException>(() =>
            BankOperationClassificationEvaluator.Evaluate(new TransactionDto(), "311444422", [invalid]));
    }

    [Fact]
    public void Evaluate_IdentifiesRuleForUnsupportedConfiguration()
    {
        var invalid = Rule(10, "BROKEN_RULE", "REVIEW_REQUIRED", conditions:
            Condition(1, "PURPOSE", "CONTAINS", "test", normalization: "UNKNOWN"));

        var exception = Assert.Throws<BankOperationClassificationConfigurationException>(() =>
            BankOperationClassificationEvaluator.Evaluate(new TransactionDto(), "311444422", [invalid]));

        Assert.Contains("BROKEN_RULE", exception.Message);
        Assert.Contains("normalization", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static BankOperationClassificationRule Rule(
        short priority,
        string code,
        string categoryCode,
        short? directionId = null,
        bool isFallback = false,
        params BankOperationClassificationCondition[] conditions)
    {
        var rule = new BankOperationClassificationRule
        {
            Id = priority,
            Priority = priority,
            Code = code,
            DirectionId = directionId,
            IsFallback = isFallback,
            StateId = StateIdConst.ACTIVE,
            CategoryId = priority,
            Category = new BankOperationCategory
            {
                Id = priority,
                Code = categoryCode,
                Name = categoryCode,
                StateId = StateIdConst.ACTIVE
            }
        };

        foreach (var condition in conditions)
        {
            condition.Rule = rule;
            rule.Conditions.Add(condition);
        }

        return rule;
    }

    private static BankOperationClassificationCondition Condition(
        short order,
        string field,
        string operation,
        string? value = null,
        string valueSource = "LITERAL",
        string normalization = "NORMALIZE_WHITESPACE",
        string[]? values = null)
    {
        var condition = new BankOperationClassificationCondition
        {
            ConditionOrder = order,
            FieldCode = field,
            OperatorCode = operation,
            ValueSourceCode = valueSource,
            CompareValue = value,
            NormalizationCode = normalization
        };

        if (values is not null)
        {
            short index = 1;
            foreach (var item in values)
            {
                condition.Values.Add(new BankOperationClassificationConditionValue
                {
                    ValueOrder = index++,
                    CompareValue = item,
                    Condition = condition
                });
            }
        }

        return condition;
    }
}
