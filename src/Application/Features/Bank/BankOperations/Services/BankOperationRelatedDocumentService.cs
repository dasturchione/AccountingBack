using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.CashCollections;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.BankOperations;

public sealed record BankOperationRelatedDocumentLink(
    DocumentRegistry Registry,
    CashCollectionDoc? CashCollection,
    short? ForcedCategoryId);

public interface IBankOperationRelatedDocumentService
{
    Task<Result<BankOperationRelatedDocumentLink?>> ResolveDraftAsync(
        long? relatedDocumentId,
        int organizationId,
        int bankAccountId,
        short directionId,
        short currencyId,
        decimal amount,
        short? classificationCategoryId,
        long? currentBankOperationId,
        CancellationToken ct);

    Task<Result<CashCollectionDoc?>> ValidateCashCollectionForConfirmAsync(
        BankOperation operation,
        CancellationToken ct);

    Task<Result<CashCollectionDoc?>> GetLinkedCashCollectionAsync(
        BankOperation operation,
        CancellationToken ct);

    Task<bool> HasActiveCashCollectionLinkAsync(
        long cashCollectionDocId,
        long? excludedBankOperationId,
        CancellationToken ct);

    Task<IReadOnlyDictionary<long, long>> GetActiveCashCollectionBankOperationIdsAsync(
        IReadOnlyCollection<long> cashCollectionDocIds,
        CancellationToken ct);

    Task<IReadOnlyDictionary<long, long>> GetCashCollectionRegistryIdsAsync(
        IReadOnlyCollection<long> cashCollectionDocIds,
        CancellationToken ct);
}

public sealed class BankOperationRelatedDocumentService : IBankOperationRelatedDocumentService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<DocumentRegistry> _documentRegistryQuery;
    private readonly IQueryRepository<CashCollectionDoc> _cashCollectionQuery;
    private readonly IQueryRepository<BankOperation> _bankOperationQuery;
    private readonly IQueryRepository<BankOperationCategory> _categoryQuery;

    public BankOperationRelatedDocumentService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<DocumentRegistry> documentRegistryQuery,
        IQueryRepository<CashCollectionDoc> cashCollectionQuery,
        IQueryRepository<BankOperation> bankOperationQuery,
        IQueryRepository<BankOperationCategory> categoryQuery)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _documentRegistryQuery = documentRegistryQuery;
        _cashCollectionQuery = cashCollectionQuery;
        _bankOperationQuery = bankOperationQuery;
        _categoryQuery = categoryQuery;
    }

    public async Task<Result<BankOperationRelatedDocumentLink?>> ResolveDraftAsync(
        long? relatedDocumentId,
        int organizationId,
        int bankAccountId,
        short directionId,
        short currencyId,
        decimal amount,
        short? classificationCategoryId,
        long? currentBankOperationId,
        CancellationToken ct)
    {
        if (!relatedDocumentId.HasValue)
            return Result.Success<BankOperationRelatedDocumentLink?>(null);

        var registry = await GetRegistryAsync(relatedDocumentId.Value, ct);
        if (registry is null)
            return Result.Failure<BankOperationRelatedDocumentLink?>(
                BankOperationErrors.RelatedDocumentNotFound(relatedDocumentId.Value, _userContext.LanguageId));

        var registryValidation = BankOperationRelatedDocumentPolicy.Validate(registry, organizationId, _userContext.LanguageId);
        if (!registryValidation.IsSuccess)
            return Result.Failure<BankOperationRelatedDocumentLink?>(registryValidation.Error);

        if (registry.DocumentTypeId != DocumentTypeIdConst.CASHCOLLECTION)
            return Result.Success<BankOperationRelatedDocumentLink?>(new(registry, null, null));

        var document = await GetCashCollectionAsync(registry.DocumentId, ct);
        if (document is null)
            return Result.Failure<BankOperationRelatedDocumentLink?>(CashCollectionErrors.NotFound(registry.DocumentId, _userContext.LanguageId));

        var validation = CashCollectionBankLinkPolicy.Validate(
            document,
            organizationId,
            bankAccountId,
            directionId,
            currencyId,
            amount,
            _userContext.LanguageId);
        if (!validation.IsSuccess)
            return Result.Failure<BankOperationRelatedDocumentLink?>(validation.Error);

        if (await HasActiveLinkToRegistryAsync(registry.Id, currentBankOperationId, ct))
            return Result.Failure<BankOperationRelatedDocumentLink?>(CashCollectionErrors.AlreadyLinked(document.Id, _userContext.LanguageId));

        var category = await GetCategoryAsync(ct);
        if (category is null)
            return Result.Failure<BankOperationRelatedDocumentLink?>(CashCollectionErrors.CategoryNotFound(_userContext.LanguageId));
        if (classificationCategoryId.HasValue && classificationCategoryId.Value != category.Id)
            return Result.Failure<BankOperationRelatedDocumentLink?>(CashCollectionErrors.CategoryMismatch(classificationCategoryId.Value, _userContext.LanguageId));

        return Result.Success<BankOperationRelatedDocumentLink?>(new(registry, document, category.Id));
    }

    public async Task<Result<CashCollectionDoc?>> ValidateCashCollectionForConfirmAsync(
        BankOperation operation,
        CancellationToken ct)
    {
        var linked = await GetLinkedCashCollectionAsync(operation, ct);
        if (!linked.IsSuccess || linked.Value is null)
            return linked;

        var document = linked.Value;
        var validation = CashCollectionBankLinkPolicy.Validate(
            document,
            operation.OrganizationId,
            operation.BankAccountId,
            operation.DirectionId,
            operation.CurrencyId,
            operation.Amount,
            _userContext.LanguageId);
        if (!validation.IsSuccess)
            return Result.Failure<CashCollectionDoc?>(validation.Error);

        var category = await GetCategoryAsync(ct);
        if (category is null)
            return Result.Failure<CashCollectionDoc?>(CashCollectionErrors.CategoryNotFound(_userContext.LanguageId));

        if (operation.ClassificationCategoryId != category.Id ||
            operation.BankChartAccountId != document.BankChartAccountId ||
            operation.OffsetAccountId != document.CashInTransitAccountId ||
            operation.CounterpartyId.HasValue ||
            operation.CounterpartyBankAccountId.HasValue ||
            operation.ContractId.HasValue)
        {
            return Result.Failure<CashCollectionDoc?>(
                CashCollectionErrors.LinkedBankOperationInvalid(_userContext.LanguageId));
        }

        return Result.Success<CashCollectionDoc?>(document);
    }

    public async Task<Result<CashCollectionDoc?>> GetLinkedCashCollectionAsync(
        BankOperation operation,
        CancellationToken ct)
    {
        if (!operation.RelatedDocumentId.HasValue)
            return Result.Success<CashCollectionDoc?>(null);

        var registry = await GetRegistryAsync(operation.RelatedDocumentId.Value, ct);
        if (registry is null)
            return Result.Failure<CashCollectionDoc?>(
                BankOperationErrors.RelatedDocumentNotFound(operation.RelatedDocumentId.Value, _userContext.LanguageId));

        if (registry.OrganizationId != operation.OrganizationId)
            return Result.Failure<CashCollectionDoc?>(
                BankOperationErrors.RelatedDocumentOrganizationMismatch(registry.Id, _userContext.LanguageId));
        if (registry.DocumentTypeId != DocumentTypeIdConst.CASHCOLLECTION)
            return Result.Success<CashCollectionDoc?>(null);
        if (registry.StateId != StateIdConst.ACTIVE)
            return Result.Failure<CashCollectionDoc?>(BankOperationErrors.RelatedDocumentInactive(registry.Id, _userContext.LanguageId));

        var document = await GetCashCollectionAsync(registry.DocumentId, ct);
        if (document is null)
            return Result.Failure<CashCollectionDoc?>(CashCollectionErrors.NotFound(registry.DocumentId, _userContext.LanguageId));
        if (document.OrganizationId != operation.OrganizationId)
            return Result.Failure<CashCollectionDoc?>(CashCollectionErrors.OrganizationMismatch(document.Id, _userContext.LanguageId));

        return Result.Success<CashCollectionDoc?>(document);
    }

    public async Task<bool> HasActiveCashCollectionLinkAsync(
        long cashCollectionDocId,
        long? excludedBankOperationId,
        CancellationToken ct)
    {
        var registryQuery = _queryBuilder.For<DocumentRegistry>()
            .Where(document =>
                document.DocumentTypeId == DocumentTypeIdConst.CASHCOLLECTION &&
                document.DocumentId == cashCollectionDocId)
            .Build();
        var registry = await _documentRegistryQuery.GetAsync(registryQuery, ct);
        return registry is not null &&
               await HasActiveLinkToRegistryAsync(registry.Id, excludedBankOperationId, ct);
    }

    public async Task<IReadOnlyDictionary<long, long>> GetActiveCashCollectionBankOperationIdsAsync(
        IReadOnlyCollection<long> cashCollectionDocIds,
        CancellationToken ct)
    {
        if (cashCollectionDocIds.Count == 0)
            return new Dictionary<long, long>();

        var query = _queryBuilder.For<BankOperation>()
            .Where(operation =>
                operation.RelatedDocument != null &&
                operation.RelatedDocument.DocumentTypeId == DocumentTypeIdConst.CASHCOLLECTION &&
                cashCollectionDocIds.Contains(operation.RelatedDocument.DocumentId) &&
                operation.StateId == StateIdConst.ACTIVE &&
                operation.StatusId != DocumentStatusIdConst.CANCELLED)
            .OrderBy(items => items.OrderByDescending(operation => operation.Id))
            .Build();
        query.AddIncludes(items => items.Include(operation => operation.RelatedDocument));

        var operations = await _bankOperationQuery.GetAllAsync(query, ct);
        return operations
            .Where(operation => operation.RelatedDocument is not null)
            .GroupBy(operation => operation.RelatedDocument!.DocumentId)
            .ToDictionary(group => group.Key, group => group.First().Id);
    }

    public async Task<IReadOnlyDictionary<long, long>> GetCashCollectionRegistryIdsAsync(
        IReadOnlyCollection<long> cashCollectionDocIds,
        CancellationToken ct)
    {
        if (cashCollectionDocIds.Count == 0)
            return new Dictionary<long, long>();

        var query = _queryBuilder.For<DocumentRegistry>()
            .Where(document =>
                document.DocumentTypeId == DocumentTypeIdConst.CASHCOLLECTION &&
                cashCollectionDocIds.Contains(document.DocumentId))
            .Build();
        var documents = await _documentRegistryQuery.GetAllAsync(query, ct);
        return documents.ToDictionary(document => document.DocumentId, document => document.Id);
    }

    private async Task<DocumentRegistry?> GetRegistryAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<DocumentRegistry>()
            .Where(document => document.Id == id)
            .Build();
        return await _documentRegistryQuery.GetAsync(query, ct);
    }

    private async Task<CashCollectionDoc?> GetCashCollectionAsync(long id, CancellationToken ct)
    {
        var query = _queryBuilder.For<CashCollectionDoc>()
            .Where(document => document.Id == id && document.StateId == StateIdConst.ACTIVE)
            .Build();
        return await _cashCollectionQuery.GetAsync(query, ct);
    }

    private Task<bool> HasActiveLinkToRegistryAsync(
        long registryId,
        long? excludedBankOperationId,
        CancellationToken ct) =>
        _bankOperationQuery.AnyAsync(operation =>
            operation.RelatedDocumentId == registryId &&
            operation.StateId == StateIdConst.ACTIVE &&
            operation.StatusId != DocumentStatusIdConst.CANCELLED &&
            (!excludedBankOperationId.HasValue || operation.Id != excludedBankOperationId.Value), ct);

    private async Task<BankOperationCategory?> GetCategoryAsync(CancellationToken ct)
    {
        var query = _queryBuilder.For<BankOperationCategory>()
            .Where(category => category.Code == BankOperationCategoryCodeConst.CASH_COLLECTION &&
                               category.StateId == StateIdConst.ACTIVE)
            .Build();
        return await _categoryQuery.GetAsync(query, ct);
    }
}
