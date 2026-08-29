using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.RetailSaleDocs;

public static class RetailSaleDocErrors
{
    public static Error NotFound(long id, short? languageId = null) => N("RetailSaleDoc.NotFound", languageId,
        $"Id-si {id} bo'lgan chakana savdo hujjati topilmadi.", $"Id-си {id} бўлган чакана савдо ҳужжати топилмади.",
        $"Документ розничной продажи с id {id} не найден.", $"Retail sale document with id {id} was not found.");
    public static Error CannotUpdate(long id, short statusId, short? languageId = null) => C("RetailSaleDoc.CannotUpdate", languageId,
        $"{id}-chakana savdo hujjatini {statusId} holatida o'zgartirib bo'lmaydi.", $"{id}-чакана савдо ҳужжатини {statusId} ҳолатида ўзгартириб бўлмайди.",
        $"Документ розничной продажи {id} нельзя изменить в статусе {statusId}.", $"Retail sale document {id} cannot be changed in status {statusId}.");
    public static Error CannotConfirm(long id, short statusId, short? languageId = null) => C("RetailSaleDoc.CannotConfirm", languageId,
        $"{id}-chakana savdo hujjatini {statusId} holatida tasdiqlab bo'lmaydi.", $"{id}-чакана савдо ҳужжатини {statusId} ҳолатида тасдиқлаб бўлмайди.",
        $"Документ розничной продажи {id} нельзя подтвердить в статусе {statusId}.", $"Retail sale document {id} cannot be confirmed in status {statusId}.");
    public static Error CannotCancel(long id, short statusId, short? languageId = null) => C("RetailSaleDoc.CannotCancel", languageId,
        $"{id}-chakana savdo hujjatini {statusId} holatida bekor qilib bo'lmaydi.", $"{id}-чакана савдо ҳужжатини {statusId} ҳолатида бекор қилиб бўлмайди.",
        $"Документ розничной продажи {id} нельзя отменить в статусе {statusId}.", $"Retail sale document {id} cannot be cancelled in status {statusId}.");
    public static Error EmptyLines(short? languageId = null) => B("RetailSaleDoc.EmptyLines", languageId,
        "Chakana savdo hujjatida kamida bitta mahsulot qatori bo'lishi kerak.", "Чакана савдо ҳужжатида камида битта маҳсулот қатори бўлиши керак.",
        "Документ розничной продажи должен содержать хотя бы одну товарную строку.", "Retail sale document must contain at least one product line.");
    public static Error InvalidLine(int productId, short? languageId = null) => B("RetailSaleDoc.InvalidLine", languageId,
        $"{productId}-mahsulotning chakana savdo qatori noto'g'ri.", $"{productId}-маҳсулотнинг чакана савдо қатори нотўғри.",
        $"Строка розничной продажи товара {productId} некорректна.", $"Retail sale line for product {productId} is invalid.");
    public static Error ProductNotFound(int productId, short? languageId = null) => N("RetailSaleDoc.ProductNotFound", languageId,
        $"Id-si {productId} bo'lgan mahsulot topilmadi.", $"Id-си {productId} бўлган маҳсулот топилмади.",
        $"Товар с id {productId} не найден.", $"Product {productId} was not found.");
    public static Error InvalidProductTable(int productTableId, short? languageId = null) => C("RetailSaleDoc.InvalidProductTable", languageId,
        $"{productTableId}-partiya chakana savdo qatori yoki omborga mos kelmaydi.", $"{productTableId}-партия чакана савдо қатори ёки омборга мос келмайди.",
        $"Партия {productTableId} не соответствует строке розничной продажи или складу.", $"Product table {productTableId} does not match the retail sale line or warehouse.");
    public static Error InvalidPayment(short? languageId = null) => B("RetailSaleDoc.InvalidPayment", languageId,
        "Chakana savdo to'lovlari noto'g'ri.", "Чакана савдо тўловлари нотўғри.",
        "Платежи розничной продажи некорректны.", "Retail sale payments are invalid.");
    public static Error PaymentTotalMismatch(decimal expected, decimal actual, short? languageId = null) => B("RetailSaleDoc.PaymentTotalMismatch", languageId,
        $"To'lovlar jami {actual} hujjat jami {expected} ga teng bo'lishi kerak.", $"Тўловлар жами {actual} ҳужжат жами {expected} га тенг бўлиши керак.",
        $"Сумма платежей {actual} должна равняться итогу документа {expected}.", $"Payment total {actual} must equal document total {expected}.");
    public static Error AccountRequired(short? languageId = null) => B("RetailSaleDoc.AccountRequired", languageId,
        "Chakana savdo uchun zarur buxgalteriya hisobvaraqlari ko'rsatilmagan.", "Чакана савдо учун зарур бухгалтерия ҳисобварақлари кўрсатилмаган.",
        "Не указаны обязательные бухгалтерские счета розничной продажи.", "Required accounting accounts are not specified for the retail sale.");
    public static Error MissingPostingBatch(long id, short? languageId = null) => C("RetailSaleDoc.MissingPostingBatch", languageId,
        $"{id}-chakana savdo hujjati uchun o'tkazmalar paketi topilmadi.", $"{id}-чакана савдо ҳужжати учун ўтказмалар пакети топилмади.",
        $"Для документа розничной продажи {id} не найден пакет проводок.", $"Posting batch for retail sale document {id} was not found.");
    public static Error MissingAccountingEntries(long id, short? languageId = null) => C("RetailSaleDoc.MissingAccountingEntries", languageId,
        $"{id}-chakana savdo hujjatining buxgalteriya yozuvlari topilmadi.", $"{id}-чакана савдо ҳужжатининг бухгалтерия ёзувлари топилмади.",
        $"Бухгалтерские проводки документа розничной продажи {id} не найдены.", $"Accounting entries for retail sale document {id} were not found.");
    public static Error BusinessEffectsExist(long id, short? languageId = null) => C("RetailSaleDoc.BusinessEffectsExist", languageId,
        $"{id}-chakana savdo hujjatida moliyaviy harakatlar allaqachon mavjud.", $"{id}-чакана савдо ҳужжатида молиявий ҳаракатлар аллақачон мавжуд.",
        $"Документ розничной продажи {id} уже имеет финансовые движения.", $"Retail sale document {id} already has business effects.");
    public static Error DocumentRegistryNotFound(long id, short? languageId = null) => C("RetailSaleDoc.DocumentRegistryNotFound", languageId,
        $"{id}-chakana savdo uchun umumiy hujjatlar reyestri yozuvi topilmadi.", $"{id}-чакана савдо учун умумий ҳужжатлар реестри ёзуви топилмади.",
        $"Запись общего реестра документов для розничной продажи {id} не найдена.", $"Document registry row for retail sale {id} was not found.");
    public static Error PaymentOperationsAlreadyExist(long id, short? languageId = null) => C("RetailSaleDoc.PaymentOperationsAlreadyExist", languageId,
        $"{id}-chakana savdo uchun to'lov qabul qilish nuqtasi operatsiyalari allaqachon mavjud.", $"{id}-чакана савдо учун тўлов қабул қилиш нуқтаси операциялари аллақачон мавжуд.",
        $"Для розничной продажи {id} уже существуют операции точки приёма платежей.", $"Retail sale {id} already has payment acceptance point operations.");
    public static Error MissingPaymentOperationBatch(long operationId, short? languageId = null) => C("RetailSaleDoc.MissingPaymentOperationBatch", languageId,
        $"{operationId}-to'lov qabul qilish operatsiyasi uchun o'tkazmalar paketi topilmadi.", $"{operationId}-тўлов қабул қилиш операцияси учун ўтказмалар пакети топилмади.",
        $"Для операции точки приёма платежей {operationId} не найден пакет проводок.", $"Posting batch for payment acceptance point operation {operationId} was not found.");
    public static Error InsufficientPaymentPointBalance(decimal currentBalance, decimal balanceAfterReversal, short? languageId = null) => B(
        "RetailSaleDoc.InsufficientPaymentPointBalance", languageId,
        $"To'lov qabul qilish nuqtasi qoldig'i {currentBalance}; bekor qilinganda qoldiq {balanceAfterReversal} bo'ladi, shuning uchun amal mumkin emas.",
        $"Тўлов қабул қилиш нуқтаси қолдиғи {currentBalance}; бекор қилинганда қолдиқ {balanceAfterReversal} бўлади, шунинг учун амал мумкин эмас.",
        $"Остаток точки приёма платежей {currentBalance}; после сторнирования он станет {balanceAfterReversal}, поэтому операция невозможна.",
        $"Payment acceptance point balance {currentBalance} cannot be reversed because the resulting balance would be {balanceAfterReversal}.");

    private static Error B(string code, short? languageId, string uz, string uzCyrl, string ru, string en) => Error.Business(code, M(languageId, uz, uzCyrl, ru, en));
    private static Error C(string code, short? languageId, string uz, string uzCyrl, string ru, string en) => Error.Conflict(code, M(languageId, uz, uzCyrl, ru, en));
    private static Error N(string code, short? languageId, string uz, string uzCyrl, string ru, string en) => Error.NotFound(code, M(languageId, uz, uzCyrl, ru, en));
    private static string M(short? languageId, string uz, string uzCyrl, string ru, string en) => languageId switch
    {
        LanguageIdConst.UZ => uz, LanguageIdConst.UZ_CYRL => uzCyrl, LanguageIdConst.RU => ru, _ => en
    };
}
