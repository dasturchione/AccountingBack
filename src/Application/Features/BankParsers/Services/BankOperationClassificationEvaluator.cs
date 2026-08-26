using System.Text.RegularExpressions;
using Domain.Entities;
using SharedKernel.Constants;

namespace Application.Features.BankParsers;

internal static class BankOperationClassificationEvaluator
{
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);

    public static BankOperationClassificationMatch? Evaluate(
        TransactionDto transaction,
        string organizationInn,
        IReadOnlyCollection<BankOperationClassificationRule> rules,
        short? languageId = null)
    {
        var activeRules = rules
            .Where(rule => rule.StateId == StateIdConst.ACTIVE)
            .OrderBy(rule => rule.Priority)
            .ThenBy(rule => rule.Id)
            .ToList();

        foreach (var rule in activeRules)
            ValidateRule(rule);

        foreach (var rule in activeRules)
        {
            if (rule.DirectionId.HasValue && rule.DirectionId.Value != transaction.DirectionId)
                continue;

            if (!rule.IsFallback && !rule.Conditions
                    .OrderBy(condition => condition.ConditionOrder)
                    .All(condition => Matches(transaction, organizationInn, condition)))
                continue;

            var categoryName = languageId.HasValue
                ? rule.Category.Translations.FirstOrDefault(x => x.LanguageId == languageId.Value)?.Name
                : null;

            return new BankOperationClassificationMatch(
                rule.CategoryId,
                rule.Category.Code,
                categoryName ?? rule.Category.Name,
                rule.Id,
                rule.Code,
                string.Equals(rule.Category.Code, BankOperationCategoryCode.ReviewRequired, StringComparison.Ordinal));
        }

        return null;
    }

    private static bool Matches(
        TransactionDto transaction,
        string organizationInn,
        BankOperationClassificationCondition condition)
    {
        var actual = Normalize(GetFieldValue(transaction, condition.FieldCode), condition.NormalizationCode);
        var expected = condition.ValueSourceCode switch
        {
            BankOperationClassificationValueSource.Literal => condition.CompareValue,
            BankOperationClassificationValueSource.OrganizationInn => organizationInn,
            _ => throw ConfigurationError(condition.Rule, $"unsupported value source '{condition.ValueSourceCode}'")
        };

        if (condition.OperatorCode is BankOperationClassificationOperator.ContainsAny
            or BankOperationClassificationOperator.NotContainsAny)
        {
            var values = condition.Values
                .OrderBy(value => value.ValueOrder)
                .Select(value => Normalize(value.CompareValue, condition.NormalizationCode))
                .ToList();
            var containsAny = values.Any(value => actual.Contains(value, StringComparison.OrdinalIgnoreCase));
            return condition.OperatorCode == BankOperationClassificationOperator.ContainsAny
                ? containsAny
                : !containsAny;
        }

        var normalizedExpected = Normalize(expected ?? string.Empty, condition.NormalizationCode);
        return condition.OperatorCode switch
        {
            BankOperationClassificationOperator.Equal =>
                actual.Equals(normalizedExpected, StringComparison.OrdinalIgnoreCase),
            BankOperationClassificationOperator.NotEquals =>
                !actual.Equals(normalizedExpected, StringComparison.OrdinalIgnoreCase),
            BankOperationClassificationOperator.StartsWith =>
                actual.StartsWith(normalizedExpected, StringComparison.OrdinalIgnoreCase),
            BankOperationClassificationOperator.NotStartsWith =>
                !actual.StartsWith(normalizedExpected, StringComparison.OrdinalIgnoreCase),
            BankOperationClassificationOperator.Contains =>
                actual.Contains(normalizedExpected, StringComparison.OrdinalIgnoreCase),
            BankOperationClassificationOperator.NotContains =>
                !actual.Contains(normalizedExpected, StringComparison.OrdinalIgnoreCase),
            _ => throw ConfigurationError(condition.Rule, $"unsupported operator '{condition.OperatorCode}'")
        };
    }

    private static string GetFieldValue(TransactionDto transaction, string fieldCode) => fieldCode switch
    {
        BankOperationClassificationField.CounterpartyInn => transaction.CounterpartyInn,
        BankOperationClassificationField.CounterpartyName => transaction.CounterpartyName,
        BankOperationClassificationField.CounterpartyAccount => transaction.CounterpartyAccount,
        BankOperationClassificationField.Purpose => transaction.Purpose,
        BankOperationClassificationField.BankDocumentNumber => transaction.BankDocumentNumber,
        BankOperationClassificationField.OperationCode => transaction.OperationCode,
        _ => throw new BankOperationClassificationConfigurationException(
            $"Bank operation classification uses unsupported field '{fieldCode}'.")
    };

    private static string Normalize(string value, string normalizationCode) => normalizationCode switch
    {
        BankOperationClassificationNormalization.None => value,
        BankOperationClassificationNormalization.Trim => value.Trim(),
        BankOperationClassificationNormalization.NormalizeWhitespace =>
            WhitespaceRegex.Replace(value, " ").Trim(),
        BankOperationClassificationNormalization.NormalizeKey =>
            string.Concat(value.Where(character => !char.IsWhiteSpace(character))),
        BankOperationClassificationNormalization.Lower =>
            WhitespaceRegex.Replace(value, " ").Trim().ToLowerInvariant(),
        _ => throw new BankOperationClassificationConfigurationException(
            $"Bank operation classification uses unsupported normalization '{normalizationCode}'.")
    };

    private static void ValidateRule(BankOperationClassificationRule rule)
    {
        if (rule.DirectionId.HasValue && !MovementDirectionIdConst.IsValid(rule.DirectionId.Value))
            throw ConfigurationError(rule, $"unsupported direction '{rule.DirectionId.Value}'");

        if (rule.IsFallback && (rule.DirectionId.HasValue || rule.Conditions.Count > 0))
            throw ConfigurationError(rule, "fallback rule must not have a direction or conditions");

        if (!rule.IsFallback && rule.Conditions.Count == 0)
            throw ConfigurationError(rule, "non-fallback rule must have at least one condition");

        foreach (var condition in rule.Conditions)
        {
            if (condition.FieldCode is not (
                    BankOperationClassificationField.CounterpartyInn or
                    BankOperationClassificationField.CounterpartyName or
                    BankOperationClassificationField.CounterpartyAccount or
                    BankOperationClassificationField.Purpose or
                    BankOperationClassificationField.BankDocumentNumber or
                    BankOperationClassificationField.OperationCode))
                throw ConfigurationError(rule, $"unsupported field '{condition.FieldCode}'");

            if (condition.OperatorCode is not (
                    BankOperationClassificationOperator.Equal or
                    BankOperationClassificationOperator.NotEquals or
                    BankOperationClassificationOperator.StartsWith or
                    BankOperationClassificationOperator.NotStartsWith or
                    BankOperationClassificationOperator.Contains or
                    BankOperationClassificationOperator.NotContains or
                    BankOperationClassificationOperator.ContainsAny or
                    BankOperationClassificationOperator.NotContainsAny))
                throw ConfigurationError(rule, $"unsupported operator '{condition.OperatorCode}'");

            if (condition.NormalizationCode is not (
                    BankOperationClassificationNormalization.None or
                    BankOperationClassificationNormalization.Trim or
                    BankOperationClassificationNormalization.NormalizeWhitespace or
                    BankOperationClassificationNormalization.NormalizeKey or
                    BankOperationClassificationNormalization.Lower))
                throw ConfigurationError(rule, $"unsupported normalization '{condition.NormalizationCode}'");

            if (condition.ValueSourceCode is not (
                    BankOperationClassificationValueSource.Literal or
                    BankOperationClassificationValueSource.OrganizationInn))
                throw ConfigurationError(rule, $"unsupported value source '{condition.ValueSourceCode}'");

            var isAny = condition.OperatorCode is BankOperationClassificationOperator.ContainsAny
                or BankOperationClassificationOperator.NotContainsAny;

            if (condition.ValueSourceCode == BankOperationClassificationValueSource.OrganizationInn &&
                (condition.FieldCode != BankOperationClassificationField.CounterpartyInn ||
                 condition.OperatorCode is not (BankOperationClassificationOperator.Equal or BankOperationClassificationOperator.NotEquals) ||
                 condition.CompareValue is not null ||
                 condition.Values.Count > 0))
                throw ConfigurationError(rule, $"condition {condition.ConditionOrder} has an invalid ORGANIZATION_INN shape");

            if (isAny && condition.Values.Count == 0)
                throw ConfigurationError(rule, $"condition {condition.ConditionOrder} requires values");

            if (isAny && condition.CompareValue is not null)
                throw ConfigurationError(rule, $"condition {condition.ConditionOrder} must use child values only");

            if (!isAny && condition.ValueSourceCode == BankOperationClassificationValueSource.Literal
                       && string.IsNullOrWhiteSpace(condition.CompareValue))
                throw ConfigurationError(rule, $"condition {condition.ConditionOrder} requires compare_value");

            if (!isAny && condition.Values.Count > 0)
                throw ConfigurationError(rule, $"condition {condition.ConditionOrder} must not have child values");
        }
    }

    private static BankOperationClassificationConfigurationException ConfigurationError(
        BankOperationClassificationRule rule,
        string detail) =>
        new($"Invalid bank operation classification rule '{rule.Code}': {detail}.");
}

internal sealed record BankOperationClassificationMatch(
    short CategoryId,
    string CategoryCode,
    string CategoryName,
    int RuleId,
    string RuleCode,
    bool RequiresReview);

internal sealed class BankOperationClassificationConfigurationException(string message) : Exception(message);

internal static class BankOperationCategoryCode
{
    public const string ReviewRequired = "REVIEW_REQUIRED";
}

internal static class BankOperationClassificationField
{
    public const string CounterpartyInn = "COUNTERPARTY_INN";
    public const string CounterpartyName = "COUNTERPARTY_NAME";
    public const string CounterpartyAccount = "COUNTERPARTY_ACCOUNT";
    public const string Purpose = "PURPOSE";
    public const string BankDocumentNumber = "BANK_DOCUMENT_NUMBER";
    public const string OperationCode = "OPERATION_CODE";
}

internal static class BankOperationClassificationOperator
{
    public const string Equal = "EQUALS";
    public const string NotEquals = "NOT_EQUALS";
    public const string StartsWith = "STARTS_WITH";
    public const string NotStartsWith = "NOT_STARTS_WITH";
    public const string Contains = "CONTAINS";
    public const string NotContains = "NOT_CONTAINS";
    public const string ContainsAny = "CONTAINS_ANY";
    public const string NotContainsAny = "NOT_CONTAINS_ANY";
}

internal static class BankOperationClassificationValueSource
{
    public const string Literal = "LITERAL";
    public const string OrganizationInn = "ORGANIZATION_INN";
}

internal static class BankOperationClassificationNormalization
{
    public const string None = "NONE";
    public const string Trim = "TRIM";
    public const string NormalizeWhitespace = "NORMALIZE_WHITESPACE";
    public const string NormalizeKey = "NORMALIZE_KEY";
    public const string Lower = "LOWER";
}
