using Application.Abstractions.Integration.Edo;
using Domain.Entities;

namespace Application.Features.Integration.Edo;

public static class EdoSourceMetadataMapper
{
    public static EdoSourceMetadataDto Map(EdoDocument document) => new()
    {
        EdoDocumentId = document.Id,
        ProviderCode = document.Provider,
        ProviderDocumentId = document.ProviderDocumentId,
        DocumentNumber = document.DocumentNumber,
        DocumentDate = document.DocumentDate,
        Direction = document.Direction,
        Status = document.Status
    };
}
