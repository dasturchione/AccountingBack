using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
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

    public async Task<Result<PagedResponse<DocumentRegistryDto>>> GetAllAsync(
        DocumentRegistryListFilter filter,
        CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure<PagedResponse<DocumentRegistryDto>>(
                CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var query = _queryBuilder.BuildPaged<DocumentRegistry, DocumentRegistryDto, DocumentRegistryListFilter>(filter);
        var page = await _query.GetPagedAsync(query, ct);
        return Result.Success(PagedResponseFactory.Create(page, filter.Page, filter.PageSize));
    }

    public async Task<Result<DocumentRegistryDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is null)
            return Result.Failure<DocumentRegistryDto>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var query = _queryBuilder.For<DocumentRegistry>()
            .Where(document => document.Id == id)
            .As<DocumentRegistryDto>()
            .Build();
        var document = await _query.GetAsync(query, ct);
        return document is null
            ? Result.Failure<DocumentRegistryDto>(DocumentRegistryErrors.NotFound(id))
            : Result.Success(document);
    }
}
