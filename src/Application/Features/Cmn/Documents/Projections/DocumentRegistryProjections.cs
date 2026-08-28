using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Cmn.Documents;

public sealed class DocumentRegistryDtoProjection : IProjectionBuilder<DocumentRegistry, DocumentRegistryDto>
{
    public Expression<Func<DocumentRegistry, DocumentRegistryDto>> Build() =>
        document => new DocumentRegistryDto
        {
            Id = document.Id,
            OrganizationId = document.OrganizationId,
            DocumentTypeId = document.DocumentTypeId,
            DocumentTypeCode = document.DocumentType.Code,
            DocumentTypeName = document.DocumentType.Name,
            DocumentId = document.DocumentId,
            DocNumber = document.DocNumber,
            DocDate = document.DocDate,
            Amount = document.Amount,
            CurrencyId = document.CurrencyId,
            CurrencyCode = document.Currency == null ? null : document.Currency.Code,
            CurrencyName = document.Currency == null ? null : document.Currency.Name,
            StatusId = document.StatusId,
            StatusCode = document.Status == null ? null : document.Status.Code,
            StatusName = document.Status == null ? null : document.Status.Name,
            StateId = document.StateId,
            StateName = document.State.FullName,
            CreatedDate = document.CreatedDate,
            UpdatedDate = document.UpdatedDate
        };
}
