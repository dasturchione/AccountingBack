using Application.Abstractions;
using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Cmn.Documents;

public sealed class DocumentRegistryService : IDocumentRegistryService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<DocumentRegistry> _query;

    public DocumentRegistryService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<DocumentRegistry> query)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _query = query;
    }

    public async Task<Result<List<DocumentRegistryDto>>> GetAllAsync(
        DocumentRegistryListFilter filter,
        CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure<List<DocumentRegistryDto>>(
                CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        filter.OrganizationId = _userContext.OrganizationId.Value;
        var query = _queryBuilder.Build<DocumentRegistry, DocumentRegistryDto, DocumentRegistryListFilter>(filter);
        var documents = await _query.GetAllAsync(query, ct);
        return Result.Success(documents);
    }

    public async Task<Result<DocumentRegistryDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure<DocumentRegistryDto>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var query = _queryBuilder.For<DocumentRegistry>()
            .Where(document => document.Id == id && document.OrganizationId == _userContext.OrganizationId.Value)
            .As<DocumentRegistryDto>()
            .Build();
        var document = await _query.GetAsync(query, ct);
        return document is null
            ? Result.Failure<DocumentRegistryDto>(DocumentRegistryErrors.NotFound(id, _userContext.LanguageId))
            : Result.Success(document);
    }
}
