using Application.Abstractions;
using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.BankParsers;

public sealed class BankOperationClassifier : IBankOperationClassifier
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<BankOperationClassificationRuleSet> _ruleSetQuery;
    private readonly IQueryRepository<Organization> _organizationQuery;
    private readonly IQueryRepository<BankOperationCategory> _categoryQuery;
    private readonly IQueryBuilder _queryBuilder;

    public BankOperationClassifier(
        IUserContext userContext,
        IQueryRepository<BankOperationClassificationRuleSet> ruleSetQuery,
        IQueryRepository<Organization> organizationQuery,
        IQueryRepository<BankOperationCategory> categoryQuery,
        IQueryBuilder queryBuilder)
    {
        _userContext = userContext;
        _ruleSetQuery = ruleSetQuery;
        _organizationQuery = organizationQuery;
        _categoryQuery = categoryQuery;
        _queryBuilder = queryBuilder;
    }

    public async Task<Result<BankExportDto>> ClassifyAsync(
        BankExportDto export,
        int bankId,
        CancellationToken ct = default)
    {
        if (!_userContext.OrganizationId.HasValue)
            return Result.Failure<BankExportDto>(BankStatementClassificationErrors.OrganizationRequired(_userContext.LanguageId));

        var organizationSpecification = _queryBuilder.For<Organization>()
            .Where(organization => organization.Id == _userContext.OrganizationId.Value)
            .Build();
        var organization = await _organizationQuery.GetAsync(organizationSpecification, ct);
        if (organization is null)
            return Result.Failure<BankExportDto>(BankStatementClassificationErrors.OrganizationNotFound(_userContext.LanguageId));

        foreach (var account in export.Accounts)
        {
            if (!NormalizeKey(account.CompanyInn).Equals(NormalizeKey(organization.Inn), StringComparison.Ordinal))
                return Result.Failure<BankExportDto>(BankStatementClassificationErrors.OrganizationMismatch(_userContext.LanguageId));
        }

        var ruleSetSpecification = _queryBuilder.For<BankOperationClassificationRuleSet>()
            .Where(ruleSet => ruleSet.BankId == bankId && ruleSet.StateId == StateIdConst.ACTIVE)
            .AddIncludes(includes =>
            {
                includes.Include(ruleSet => ruleSet.Rules)
                    .ThenInclude(rule => rule.Conditions)
                    .ThenInclude(condition => condition.Values);
                includes.Include(ruleSet => ruleSet.Rules)
                    .ThenInclude(rule => rule.Category)
                    .ThenInclude(category => category.Translations);
            })
            .Build();
        var ruleSets = await _ruleSetQuery.GetAllAsync(ruleSetSpecification, ct);
        var ruleSet = ruleSets
            .OrderByDescending(item => item.Version)
            .ThenByDescending(item => item.Id)
            .FirstOrDefault();
        if (ruleSet is null)
            return Result.Failure<BankExportDto>(BankStatementClassificationErrors.RuleSetNotFound(bankId, _userContext.LanguageId));

        try
        {
            foreach (var transaction in export.Accounts.SelectMany(account => account.Transactions))
            {
                var match = BankOperationClassificationEvaluator.Evaluate(
                    transaction,
                    organization.Inn,
                    ruleSet.Rules.ToList(),
                    _userContext.LanguageId);

                if (match is null)
                {
                    var categorySpecification = _queryBuilder.For<BankOperationCategory>()
                        .Where(category =>
                            category.Code == BankOperationCategoryCodeConst.REVIEW_REQUIRED &&
                            category.StateId == StateIdConst.ACTIVE)
                        .AddIncludes(includes => includes.Include(category => category.Translations))
                        .Build();
                    var reviewCategory = await _categoryQuery.GetAsync(categorySpecification, ct);
                    if (reviewCategory is null)
                        return Result.Failure<BankExportDto>(BankStatementClassificationErrors.ReviewCategoryNotFound(_userContext.LanguageId));

                    transaction.ClassificationCategoryId = reviewCategory.Id;
                    transaction.ClassificationCode = reviewCategory.Code;
                    transaction.ClassificationName = GetCategoryName(reviewCategory, _userContext.LanguageId);
                    transaction.RequiresReview = true;
                    continue;
                }

                transaction.ClassificationCategoryId = match.CategoryId;
                transaction.ClassificationCode = match.CategoryCode;
                transaction.ClassificationName = match.CategoryName;
                transaction.ClassificationRuleId = match.RuleId;
                transaction.ClassificationRuleCode = match.RuleCode;
                transaction.RequiresReview = match.RequiresReview;
            }
        }
        catch (BankOperationClassificationConfigurationException)
        {
            return Result.Failure<BankExportDto>(
                BankStatementClassificationErrors.InvalidConfiguration(_userContext.LanguageId));
        }

        return Result.Success(export);
    }

    private static string GetCategoryName(BankOperationCategory category, short? languageId) =>
        languageId.HasValue
            ? category.Translations.FirstOrDefault(x => x.LanguageId == languageId.Value)?.Name ?? category.Name
            : category.Name;

    private static string NormalizeKey(string value) =>
        string.Concat(value.Where(character => !char.IsWhiteSpace(character)));
}
