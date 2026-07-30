using SharedKernel.Results;

namespace Application.Features.InventoryMovements;

public static class InventoryMovementErrors
{
    public static Error UnsupportedDocumentType() =>
        Error.Business("InventoryMovement.UnsupportedDocumentType", "Unsupported inventory document type.");

    public static Error OriginalMovementsNotFound(short documentTypeId, long documentId) =>
        Error.Conflict(
            "InventoryMovement.OriginalMovementsNotFound",
            $"Original warehouse movements for document {documentTypeId}/{documentId} were not found or are incomplete.");
}
