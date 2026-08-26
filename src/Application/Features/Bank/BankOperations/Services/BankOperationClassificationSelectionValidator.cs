using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.BankOperations;

public sealed class BankOperationClassificationSelectionValidator
    : IBankOperationClassificationSelectionValidator
{
    private readonly IQueryRepository<BankAccount> _bankAccountQuery;
    private readonly IQueryRepository<BankOperationCategory> _categoryQuery;
    private readonly IQueryRepository<BankOperationClassificationRule> _ruleQuery;
    private readonly IQueryBuilder _queryBuilder;

    public BankOperationClassificationSelectionValidator(
        IQueryRepository<BankAccount> bankAccountQuery,
        IQueryRepository<BankOperationCategory> categoryQuery,
        IQueryRepository<BankOperationClassificationRule> ruleQuery,
        IQueryBuilder queryBuilder)
    {
        _bankAccountQuery = bankAccountQuery;
        _categoryQuery = categoryQuery;
        _ruleQuery = ruleQuery;
        _queryBuilder = queryBuilder;
    }

    public async Task<Result> ValidateAsync(
        int organizationId,
        int bankAccountId,
        short? categoryId,
        int? ruleId,
        CancellationToken ct = default)
    {
        if (!categoryId.HasValue && !ruleId.HasValue)
            return Result.Success();

        if (!categoryId.HasValue)
            return Result.Failure(BankOperationErrors.ClassificationCategoryRequired());

        var categorySpecification = _queryBuilder.For<BankOperationCategory>()
            .Where(category => category.Id == categoryId.Value && category.StateId == StateIdConst.ACTIVE)
            .Build();
        if (await _categoryQuery.GetAsync(categorySpecification, ct) is null)
            return Result.Failure(BankOperationErrors.ClassificationCategoryNotFound(categoryId.Value));

        var accountSpecification = _queryBuilder.For<BankAccount>()
            .Where(account =>
                account.Id == bankAccountId &&
                account.OrganizationId == organizationId &&
                account.StateId == StateIdConst.ACTIVE)
            .Build();
        var bankAccount = await _bankAccountQuery.GetAsync(accountSpecification, ct);
        if (bankAccount is null)
            return Result.Failure(BankOperationErrors.ClassificationBankAccountNotFound(bankAccountId));

        if (!ruleId.HasValue)
            return Result.Success();

        var ruleSpecification = _queryBuilder.For<BankOperationClassificationRule>()
            .Where(rule => rule.Id == ruleId.Value && rule.StateId == StateIdConst.ACTIVE)
            .AddIncludes(includes => includes.Include(rule => rule.RuleSet))
            .Build();
        var rule = await _ruleQuery.GetAsync(ruleSpecification, ct);
        if (rule is null || rule.RuleSet.StateId != StateIdConst.ACTIVE)
            return Result.Failure(BankOperationErrors.ClassificationRuleNotFound(ruleId.Value));

        if (rule.CategoryId != categoryId.Value)
            return Result.Failure(BankOperationErrors.ClassificationCategoryMismatch(ruleId.Value, categoryId.Value));

        if (rule.RuleSet.BankId != bankAccount.BankId)
            return Result.Failure(BankOperationErrors.ClassificationBankMismatch(ruleId.Value, bankAccount.BankId));

        return Result.Success();
    }
}
