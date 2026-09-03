using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Documents;

public static class DocumentPdfErrors
{
    public static Error RenderFailed(short? languageId = null) =>
        Error.Problem("DOCUMENT_PDF_RENDER_FAILED", languageId switch
        {
            LanguageIdConst.UZ => "Hujjatning PDF faylini yaratib bo'lmadi.",
            LanguageIdConst.UZ_CYRL => "Ҳужжатнинг PDF файлини яратиб бўлмади.",
            LanguageIdConst.RU => "Не удалось сформировать PDF-файл документа.",
            _ => "The document PDF could not be generated."
        });
}
