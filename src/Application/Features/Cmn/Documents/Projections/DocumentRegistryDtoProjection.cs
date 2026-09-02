using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Cmn.Documents;

public sealed class DocumentRegistryDtoProjection(IUserContext userContext) : IProjectionBuilder<DocumentRegistry, DocumentRegistryDto>
{
    public Expression<Func<DocumentRegistry, DocumentRegistryDto>> Build()
    {
        var languageId = userContext.LanguageId;

        return document => new DocumentRegistryDto
        {
            Id = document.Id,
            OrganizationId = document.OrganizationId,
            DocumentTypeId = document.DocumentTypeId,
            DocumentTypeCode = document.DocumentType.Code,
            DocumentTypeName = document.DocumentType.DocumentTypeTranslations
                .Where(translation => translation.LanguageId == languageId)
                .Select(translation => translation.Name)
                .FirstOrDefault() ?? document.DocumentType.Name,
            DocumentId = document.DocumentId,
            DocNumber = document.DocNumber,
            DocDate = document.DocDate,
            Amount = document.Amount,
            CurrencyId = document.CurrencyId,
            CurrencyCode = document.Currency == null ? null : document.Currency.Code,
            CurrencyName = document.Currency == null
                ? null
                : document.Currency.CurrencyTranslations
                    .Where(translation => translation.LanguageId == languageId)
                    .Select(translation => translation.Name)
                    .FirstOrDefault() ?? document.Currency.Name,
            StatusId = document.StatusId,
            StatusCode = document.Status == null ? null : document.Status.Code,
            StatusName = document.Status == null
                ? null
                : document.Status.DocumentStatusTranslations
                    .Where(translation => translation.LanguageId == languageId)
                    .Select(translation => translation.Name)
                    .FirstOrDefault() ?? document.Status.Name,
            StateId = document.StateId,
            StateName = document.State.FullName,
            CreatedDate = document.CreatedDate,
            UpdatedDate = document.UpdatedDate
        };
    }
}
