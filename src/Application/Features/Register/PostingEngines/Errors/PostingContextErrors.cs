using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Register.PostingEngines
{
    public static class PostingContextErrors
    {
        public static Error UnsupportedDocumentType(short? languageId = null) =>
            Error.Business("PostingContext.UnsupportedDocumentType", languageId switch
            {
                LanguageIdConst.UZ => "Hujjat turi uchun buxgalteriya konteksti qo'llab-quvvatlanmaydi.",
                LanguageIdConst.UZ_CYRL => "Ҳужжат тури учун бухгалтерия контексти қўллаб-қувватланмайди.",
                LanguageIdConst.RU => "Тип документа не поддерживается построителем бухгалтерского контекста.",
                _ => "Unsupported document type."
            });
    }
}
