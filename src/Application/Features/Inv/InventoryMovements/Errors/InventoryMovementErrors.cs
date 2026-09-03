using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.InventoryMovements;

public static class InventoryMovementErrors
{
    public static Error UnsupportedDocumentType(short? languageId = null) =>
        Error.Business("InventoryMovement.UnsupportedDocumentType", languageId switch
        {
            LanguageIdConst.UZ => "Ombor harakati uchun bu hujjat turi qo'llab-quvvatlanmaydi.",
            LanguageIdConst.UZ_CYRL => "Омбор ҳаракати учун бу ҳужжат тури қўллаб-қувватланмайди.",
            LanguageIdConst.RU => "Тип складского документа не поддерживается.",
            _ => "Unsupported inventory document type."
        });

    public static Error OriginalMovementsNotFound(short documentTypeId, long documentId, short? languageId = null) =>
        Error.Conflict(
            "InventoryMovement.OriginalMovementsNotFound",
            languageId switch
            {
                LanguageIdConst.UZ => $"{documentTypeId}/{documentId} hujjatining dastlabki ombor harakatlari topilmadi yoki to'liq emas.",
                LanguageIdConst.UZ_CYRL => $"{documentTypeId}/{documentId} ҳужжатининг дастлабки омбор ҳаракатлари топилмади ёки тўлиқ эмас.",
                LanguageIdConst.RU => $"Исходные складские движения документа {documentTypeId}/{documentId} не найдены или неполны.",
                _ => $"Original warehouse movements for document {documentTypeId}/{documentId} were not found or are incomplete."
            });
}
