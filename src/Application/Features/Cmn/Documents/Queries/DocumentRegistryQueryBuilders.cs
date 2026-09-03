using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Cmn.Documents;

public sealed class DocumentRegistryCriteriaBuilder : ICriteriaBuilder<DocumentRegistry, DocumentRegistryListFilter>
{
    public Expression<Func<DocumentRegistry, bool>> Build(DocumentRegistryListFilter filter) =>
        document =>
            (!filter.OrganizationId.HasValue || document.OrganizationId == filter.OrganizationId.Value) &&
            (string.IsNullOrWhiteSpace(filter.DocumentTypeCode) || document.DocumentType.Code == filter.DocumentTypeCode) &&
            (!filter.CurrencyId.HasValue || document.CurrencyId == filter.CurrencyId.Value) &&
            (!filter.StatusId.HasValue || document.StatusId == filter.StatusId.Value) &&
            (!filter.StateId.HasValue || document.StateId == filter.StateId.Value) &&
            (!filter.DateFrom.HasValue || document.DocDate >= filter.DateFrom.Value) &&
            (!filter.DateTo.HasValue || document.DocDate <= filter.DateTo.Value);
}

public sealed class DocumentRegistryDtoCriteriaBuilder : ICriteriaBuilder<DocumentRegistryDto, DocumentRegistryListFilter>
{
    public Expression<Func<DocumentRegistryDto, bool>> Build(DocumentRegistryListFilter filter) =>
        document => string.IsNullOrWhiteSpace(filter.Search) ||
                    document.DocNumber.ToLower().Contains(filter.Search.ToLower()) ||
                    document.DocumentTypeCode.ToLower().Contains(filter.Search.ToLower()) ||
                    document.DocumentTypeName.ToLower().Contains(filter.Search.ToLower());
}

public sealed class DocumentRegistryOrderByBuilder : IOrderByBuilder<DocumentRegistry, DocumentRegistryDto>
{
    public Func<IQueryable<DocumentRegistryDto>, IOrderedQueryable<DocumentRegistryDto>> Build() =>
        documents => documents.OrderByDescending(document => document.DocDate)
            .ThenByDescending(document => document.Id);
}
