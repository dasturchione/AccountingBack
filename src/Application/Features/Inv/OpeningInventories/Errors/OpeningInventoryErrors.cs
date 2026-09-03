using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Inv.OpeningInventories;

public static class OpeningInventoryErrors
{
    public static Error NotFound(long id, short? languageId = null) => N("OpeningInventory.NotFound", languageId,
        $"Id-si {id} bo'lgan boshlang'ich zaxira hujjati topilmadi.", $"Id-си {id} бўлган бошланғич захира ҳужжати топилмади.",
        $"Документ начальных запасов {id} не найден.", $"Opening inventory document '{id}' was not found.");
    public static Error ReferenceNotFound(string reference, long id, short? languageId = null) => N($"OpeningInventory.{reference}NotFound", languageId,
        $"{reference} '{id}' topilmadi.", $"{reference} '{id}' топилмади.", $"{reference} '{id}' не найден.", $"{reference} '{id}' was not found.");
    public static Error ChartAccountNotFound(int id, short? languageId = null) => N("OpeningInventory.ChartAccountNotFound", languageId,
        $"Faol, guruh bo'lmagan '{id}' hisobvaraq topilmadi.", $"Фаол, гуруҳ бўлмаган '{id}' ҳисобварақ топилмади.",
        $"Активный негрупповой счёт '{id}' не найден.", $"Active non-group chart account '{id}' was not found.");
    public static Error OpeningBalanceNotFound(int organizationId, short? languageId = null) => B("OpeningInventory.OpeningBalanceNotFound", languageId,
        $"'{organizationId}' tashkilot uchun faol boshlang'ich qoldiq mavjud bo'lishi kerak.", $"'{organizationId}' ташкилот учун фаол бошланғич қолдиқ мавжуд бўлиши керак.",
        $"Для организации '{organizationId}' должен существовать активный начальный остаток.", $"An active opening balance must exist for organization '{organizationId}'.");
    public static Error BaseCurrencyNotConfigured(int organizationId, short? languageId = null) => B("OpeningInventory.BaseCurrencyNotConfigured", languageId,
        $"'{organizationId}' tashkilot uchun asosiy valyuta sozlanishi kerak.", $"'{organizationId}' ташкилот учун асосий валюта созланиши керак.",
        $"Для организации '{organizationId}' должна быть настроена основная валюта.", $"A valid base currency must be configured for organization '{organizationId}'.");
    public static Error DuplicateProduct(int productId, short? languageId = null) => B("OpeningInventory.DuplicateProduct", languageId,
        $"'{productId}' mahsulot bir martadan ko'p kiritilgan.", $"'{productId}' маҳсулот бир мартадан кўп киритилган.",
        $"Товар '{productId}' указан более одного раза.", $"Product '{productId}' occurs more than once.");
    public static Error LinesRequired(short? languageId = null) => B("OpeningInventory.LinesRequired", languageId,
        "Kamida bitta mahsulot qatori talab qilinadi.", "Камида битта маҳсулот қатори талаб қилинади.",
        "Требуется хотя бы одна товарная строка.", "At least one product line is required.");
    public static Error InvalidQuantity(int productId, decimal quantity, short? languageId = null) => B("OpeningInventory.InvalidQuantity", languageId,
        $"'{productId}' mahsulot miqdori noldan katta bo'lishi kerak; berilgan: {quantity}.", $"'{productId}' маҳсулот миқдори нолдан катта бўлиши керак; берилган: {quantity}.",
        $"Количество товара '{productId}' должно быть больше нуля; передано: {quantity}.", $"Product '{productId}' quantity must be greater than zero; supplied value is {quantity}.");
    public static Error InvalidUnitPrice(int productId, decimal unitPrice, short? languageId = null) => B("OpeningInventory.InvalidUnitPrice", languageId,
        $"'{productId}' mahsulot birligi narxi manfiy bo'lishi mumkin emas; berilgan: {unitPrice}.", $"'{productId}' маҳсулот бирлиги нархи манфий бўлиши мумкин эмас; берилган: {unitPrice}.",
        $"Цена единицы товара '{productId}' не может быть отрицательной; передано: {unitPrice}.", $"Product '{productId}' unit price cannot be negative; supplied value is {unitPrice}.");
    public static Error InvalidAmount(int productId, decimal expected, decimal actual, short? languageId = null) => B("OpeningInventory.InvalidAmount", languageId,
        $"'{productId}' mahsulot summasi miqdor × narxga ({expected}) teng bo'lishi kerak; berilgan: {actual}.", $"'{productId}' маҳсулот суммаси миқдор × нархга ({expected}) тенг бўлиши керак; берилган: {actual}.",
        $"Сумма товара '{productId}' должна равняться количество × цена ({expected}); передано: {actual}.", $"Product '{productId}' amount must equal quantity × unit price ({expected}); supplied value is {actual}.");
    public static Error InvalidTotal(decimal expected, decimal actual, short? languageId = null) => B("OpeningInventory.InvalidTotal", languageId,
        $"Hujjat jami qatorlar yig'indisiga ({expected}) teng bo'lishi kerak; berilgan: {actual}.", $"Ҳужжат жами қаторлар йиғиндисига ({expected}) тенг бўлиши керак; берилган: {actual}.",
        $"Итог документа должен равняться сумме строк ({expected}); передано: {actual}.", $"Document total must equal the sum of line amounts ({expected}); supplied value is {actual}.");
    public static Error ItemsNotAllowed(int productId, short? languageId = null) => B("OpeningInventory.ItemsNotAllowed", languageId,
        $"Donabay kuzatilmaydigan '{productId}' mahsulot uchun alohida birliklar kiritilmaydi.", $"Донабай кузатилмайдиган '{productId}' маҳсулот учун алоҳида бирликлар киритилмайди.",
        $"Для товара '{productId}' без поштучного учёта нельзя указывать отдельные экземпляры.", $"Items are not allowed for non-piece-tracked product '{productId}'.");
    public static Error ItemsRequired(int productId, short? languageId = null) => B("OpeningInventory.ItemsRequired", languageId,
        $"Donabay kuzatiladigan '{productId}' mahsulot uchun birliklar talab qilinadi.", $"Донабай кузатиладиган '{productId}' маҳсулот учун бирликлар талаб қилинади.",
        $"Для товара '{productId}' с поштучным учётом требуются экземпляры.", $"Items are required for piece-tracked product '{productId}'.");
    public static Error ItemsQuantityMismatch(int productId, decimal quantity, int count, short? languageId = null) => B("OpeningInventory.ItemsQuantityMismatch", languageId,
        $"'{productId}' mahsulot miqdori ({quantity}) birliklar soniga ({count}) teng bo'lishi kerak.", $"'{productId}' маҳсулот миқдори ({quantity}) бирликлар сонига ({count}) тенг бўлиши керак.",
        $"Количество товара '{productId}' ({quantity}) должно совпадать с числом экземпляров ({count}).", $"Product '{productId}' quantity ({quantity}) must equal its item count ({count}).");
    public static Error DuplicateMarkingNumber(string number, short? languageId = null) => B("OpeningInventory.DuplicateMarkingNumber", languageId,
        $"'{number}' markirovka raqami bir martadan ko'p kiritilgan.", $"'{number}' маркировка рақами бир мартадан кўп киритилган.",
        $"Номер маркировки '{number}' указан более одного раза.", $"Marking number '{number}' occurs more than once.");
    public static Error MarkingNumberRequired(int productId, short? languageId = null) => B("OpeningInventory.MarkingNumberRequired", languageId,
        $"'{productId}' mahsulotning har bir birligi uchun markirovka raqami talab qilinadi.", $"'{productId}' маҳсулотнинг ҳар бир бирлиги учун маркировка рақами талаб қилинади.",
        $"Для каждого экземпляра товара '{productId}' требуется номер маркировки.", $"A marking number is required for every item of product '{productId}'.");
    public static Error EffectsAlreadyUsed(long id, short? languageId = null) => C("OpeningInventory.EffectsAlreadyUsed", languageId,
        $"'{id}' boshlang'ich zaxira hujjatini o'zgartirib bo'lmaydi: uning zaxirasi yoki birliklari boshqa hujjatda ishlatilgan.", $"'{id}' бошланғич захира ҳужжатини ўзгартириб бўлмайди: унинг захираси ёки бирликлари бошқа ҳужжатда ишлатилган.",
        $"Документ начальных запасов '{id}' нельзя изменить: его остатки или экземпляры уже использованы другим документом.", $"Opening inventory document '{id}' cannot be changed because its stock or tracked items are already used by another document.");
    public static Error SubkontoValueUnavailable(int accountId, short subkontoTypeId, short? languageId = null) => B("OpeningInventory.SubkontoValueUnavailable", languageId,
        $"'{accountId}' hisobvaraqning '{subkontoTypeId}' subkonto turini boshlang'ich zaxira qatoridan aniqlab bo'lmaydi.", $"'{accountId}' ҳисобварақнинг '{subkontoTypeId}' субконто турини бошланғич захира қаторидан аниқлаб бўлмайди.",
        $"Тип субконто '{subkontoTypeId}' счёта '{accountId}' нельзя определить из строки начальных запасов.", $"Configured subkonto type '{subkontoTypeId}' for account '{accountId}' cannot be derived from an opening inventory line.");

    private static Error B(string code, short? languageId, string uz, string cy, string ru, string en) => Error.Business(code, M(languageId, uz, cy, ru, en));
    private static Error C(string code, short? languageId, string uz, string cy, string ru, string en) => Error.Conflict(code, M(languageId, uz, cy, ru, en));
    private static Error N(string code, short? languageId, string uz, string cy, string ru, string en) => Error.NotFound(code, M(languageId, uz, cy, ru, en));
    private static string M(short? languageId, string uz, string cy, string ru, string en) => languageId switch
    {
        LanguageIdConst.UZ => uz, LanguageIdConst.UZ_CYRL => cy, LanguageIdConst.RU => ru, _ => en
    };
}
