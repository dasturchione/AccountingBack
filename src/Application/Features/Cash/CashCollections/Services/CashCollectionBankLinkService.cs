using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.CashCollections;

public sealed record CashCollectionBankLink(CashCollectionDoc Document, short CategoryId);

public interface ICashCollectionBankLinkService
{
    Task<Result<CashCollectionBankLink?>> ResolveDraftAsync(
        long? cashCollectionDocId,
        int organizationId,
        int bankAccountId,
        short directionId,
        short currencyId,
        decimal amount,
        short? classificationCategoryId,
        long? currentBankOperationId,
        CancellationToken ct);

    Task<Result<CashCollectionDoc?>> ValidateForConfirmAsync(BankOperation operation, CancellationToken ct);

    Task<Result<CashCollectionDoc?>> GetLinkedAsync(BankOperation operation, CancellationToken ct);
}

public sealed class CashCollectionBankLinkService : ICashCollectionBankLinkService
{
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<CashCollectionDoc> _cashCollectionQuery;
    private readonly IQueryRepository<BankOperation> _bankOperationQuery;
    private readonly IQueryRepository<BankOperationCategory> _categoryQuery;

    public CashCollectionBankLinkService(
        IQueryBuilder queryBuilder,
        IQueryRepository<CashCollectionDoc> cashCollectionQuery,
        IQueryRepository<BankOperation> bankOperationQuery,
        IQueryRepository<BankOperationCategory> categoryQuery)
    {
        _queryBuilder = queryBuilder;
        _cashCollectionQuery = cashCollectionQuery;
        _bankOperationQuery = bankOperationQuery;
        _categoryQuery = categoryQuery;
    }

    public async Task<Result<CashCollectionBankLink?>> ResolveDraftAsync(
        long? cashCollectionDocId,
        int organizationId,
        int bankAccountId,
        short directionId,
        short currencyId,
        decimal amount,
        short? classificationCategoryId,
        long? currentBankOperationId,
        CancellationToken ct)
    {
        if (!cashCollectionDocId.HasValue)
            return Result.Success<CashCollectionBankLink?>(null);

        var document = await GetDocumentAsync(cashCollectionDocId.Value, ct);
        if (document is null)
            return Result.Failure<CashCollectionBankLink?>(CashCollectionErrors.NotFound(cashCollectionDocId.Value));

        var validation = CashCollectionBankLinkPolicy.Validate(
            document,
            organizationId,
            bankAccountId,
            directionId,
            currencyId,
            amount);
        if (!validation.IsSuccess)
            return Result.Failure<CashCollectionBankLink?>(validation.Error);

        var alreadyLinked = await _bankOperationQuery.AnyAsync(operation =>
            operation.CashCollectionDocId == document.Id &&
            operation.StateId == StateIdConst.ACTIVE &&
            operation.StatusId != DocumentStatusIdConst.CANCELLED &&
            (!currentBankOperationId.HasValue || operation.Id != currentBankOperationId.Value), ct);
        if (alreadyLinked)
            return Result.Failure<CashCollectionBankLink?>(CashCollectionErrors.AlreadyLinked(document.Id));

        var category = await GetCategoryAsync(ct);
        if (category is null)
            return Result.Failure<CashCollectionBankLink?>(CashCollectionErrors.CategoryNotFound());
        if (classificationCategoryId.HasValue && classificationCategoryId.Value != category.Id)
            return Result.Failure<CashCollectionBankLink?>(CashCollectionErrors.CategoryMismatch(classificationCategoryId.Value));

        return Result.Success<CashCollectionBankLink?>(new CashCollectionBankLink(document, category.Id));
    }

    public async Task<Result<CashCollectionDoc?>> ValidateForConfirmAsync(BankOperation operation, CancellationToken ct)
    {
        var linked = await GetLinkedAsync(operation, ct);
        if (!linked.IsSuccess || linked.Value is null)
            return linked;

        var document = linked.Value;
        var validation = CashCollectionBankLinkPolicy.Validate(
            document,
            operation.OrganizationId,
            operation.BankAccountId,
            operation.DirectionId,
            operation.CurrencyId,
            operation.Amount);
        if (!validation.IsSuccess)
            return Result.Failure<CashCollectionDoc?>(validation.Error);

        var category = await GetCategoryAsync(ct);
        if (category is null)
            return Result.Failure<CashCollectionDoc?>(CashCollectionErrors.CategoryNotFound());

        if (operation.ClassificationCategoryId != category.Id ||
            operation.BankChartAccountId != document.BankChartAccountId ||
            operation.OffsetAccountId != document.CashInTransitAccountId ||
            operation.CounterpartyId.HasValue ||
            operation.CounterpartyBankAccountId.HasValue ||
            operation.ContractId.HasValue)
        {
            return Result.Failure<CashCollectionDoc?>(CashCollectionErrors.InvalidConfiguration(
                "Linked bank operation must use cash-collection category and the bank/cash-in-transit accounts without counterparty settlement references."));
        }

        return Result.Success<CashCollectionDoc?>(document);
    }

    public async Task<Result<CashCollectionDoc?>> GetLinkedAsync(BankOperation operation, CancellationToken ct)
    {
        if (!operation.CashCollectionDocId.HasValue)
            return Result.Success<CashCollectionDoc?>(null);

        var document = await GetDocumentAsync(operation.CashCollectionDocId.Value, ct);
        if (document is null)
            return Result.Failure<CashCollectionDoc?>(CashCollectionErrors.NotFound(operation.CashCollectionDocId.Value));
        if (document.OrganizationId != operation.OrganizationId)
            return Result.Failure<CashCollectionDoc?>(CashCollectionErrors.OrganizationMismatch(document.Id));

        return Result.Success<CashCollectionDoc?>(document);
    }

    private async Task<CashCollectionDoc?> GetDocumentAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<CashCollectionDoc>()
            .Where(document => document.Id == id && document.StateId == StateIdConst.ACTIVE)
            .Build();
        return await _cashCollectionQuery.GetAsync(query, ct);
    }

    private async Task<BankOperationCategory?> GetCategoryAsync(CancellationToken ct)
    {
        var query = _queryBuilder.For<BankOperationCategory>()
            .Where(category => category.Code == BankOperationCategoryCodeConst.CASH_COLLECTION &&
                               category.StateId == StateIdConst.ACTIVE)
            .Build();
        return await _categoryQuery.GetAsync(query, ct);
    }
}
