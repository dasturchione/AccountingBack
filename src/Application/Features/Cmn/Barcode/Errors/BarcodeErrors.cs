using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Barcode;

public static class BarcodeErrors
{
    public static Error EmptyContent(short? languageId = null) => Validation("Barcode.EmptyContent", languageId,
        "Shtrix-kod yoki QR-kod mazmuni bo'sh bo'lishi mumkin emas.", "Штрих-код ёки QR-код мазмуни бўш бўлиши мумкин эмас.",
        "Содержимое штрихкода или QR-кода не может быть пустым.", "Barcode or QR content must not be empty.");

    public static Error InvalidEan13(short? languageId = null) => Validation("Barcode.InvalidEan13", languageId,
        "EAN-13 uchun 12 yoki 13 ta raqam kerak.", "EAN-13 учун 12 ёки 13 та рақам керак.",
        "Для EAN-13 требуется 12 или 13 цифр.", "EAN-13 requires 12 or 13 digits.");

    public static Error UseQrMethod(short? languageId = null) => Validation("Barcode.UseQrMethod", languageId,
        "QR-kod uchun GenerateQr() metodidan foydalaning.", "QR-код учун GenerateQr() методидан фойдаланинг.",
        "Для QR-кода используйте метод GenerateQr().", "Use GenerateQr() for QR codes.");

    public static Error UnsupportedFormat(string format, short? languageId = null) => Validation("Barcode.UnsupportedFormat", languageId,
        $"Qo'llab-quvvatlanmaydigan format: {format}.", $"Қўллаб-қувватланмайдиган формат: {format}.",
        $"Неподдерживаемый формат: {format}.", $"Unsupported format: {format}.");

    public static Error GenerationFailed(short? languageId = null) =>
        Error.Problem("Barcode.GenerationFailed", languageId switch
        {
            LanguageIdConst.UZ => "Shtrix-kod yoki QR-kodni yaratib bo'lmadi.",
            LanguageIdConst.UZ_CYRL => "Штрих-код ёки QR-кодни яратиб бўлмади.",
            LanguageIdConst.RU => "Не удалось сформировать штрихкод или QR-код.",
            _ => "The barcode or QR code could not be generated."
        });

    private static Error Validation(string code, short? languageId, string uz, string uzCyrl, string ru, string en) =>
        Error.Validation(code, languageId switch
        {
            LanguageIdConst.UZ => uz,
            LanguageIdConst.UZ_CYRL => uzCyrl,
            LanguageIdConst.RU => ru,
            _ => en
        });
}
