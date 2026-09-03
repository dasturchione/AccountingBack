using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.WarehouseTransfers;

public static class WarehouseTransferErrors
{
    public static Error NotFound(long id, short? languageId = null) => N("WarehouseTransfer.NotFound", languageId,
        $"Id-si {id} bo'lgan omborlararo ko'chirish topilmadi.", $"Id-си {id} бўлган омборлараро кўчириш топилмади.",
        $"Складское перемещение с id {id} не найдено.", $"Warehouse transfer with id {id} was not found.");
    public static Error AlreadyCancelled(long id, short? languageId = null) => C("WarehouseTransfer.AlreadyCancelled", languageId,
        $"Id-si {id} bo'lgan omborlararo ko'chirish allaqachon bekor qilingan.", $"Id-си {id} бўлган омборлараро кўчириш аллақачон бекор қилинган.",
        $"Складское перемещение с id {id} уже отменено.", $"Warehouse transfer with id {id} is already cancelled.");
    public static Error CannotConfirmInCurrentStatus(long id, short statusId, short? languageId = null) => InvalidStatus("CannotConfirmInCurrentStatus", id, statusId, "tasdiqlash", "тасдиқлаш", "подтвердить", "confirmed", languageId);
    public static Error CannotUpdateInCurrentStatus(long id, short statusId, short? languageId = null) => InvalidStatus("CannotUpdateInCurrentStatus", id, statusId, "o'zgartirish", "ўзгартириш", "изменить", "updated", languageId);
    public static Error CannotCancelInCurrentStatus(long id, short statusId, short? languageId = null) => InvalidStatus("CannotCancelInCurrentStatus", id, statusId, "bekor qilish", "бекор қилиш", "отменить", "cancelled", languageId);
    public static Error CannotDeleteInCurrentStatus(long id, short statusId, short? languageId = null) => InvalidStatus("CannotDeleteInCurrentStatus", id, statusId, "o'chirish", "ўчириш", "удалить", "deleted", languageId);
    public static Error LinesRequired(long id, short? languageId = null) => B("WarehouseTransfer.LinesRequired", languageId,
        $"Id-si {id} bo'lgan omborlararo ko'chirishda kamida bitta qator bo'lishi kerak.", $"Id-си {id} бўлган омборлараро кўчиришда камида битта қатор бўлиши керак.",
        $"Складское перемещение с id {id} должно содержать хотя бы одну строку.", $"Warehouse transfer with id {id} must contain at least one line.");
    public static Error BusinessEffectsAlreadyExist(long id, short? languageId = null) => C("WarehouseTransfer.BusinessEffectsAlreadyExist", languageId,
        $"Id-si {id} bo'lgan ko'chirishda ombor harakatlari allaqachon mavjud.", $"Id-си {id} бўлган кўчиришда омбор ҳаракатлари аллақачон мавжуд.",
        $"Складское перемещение с id {id} уже имеет движения запасов.", $"Warehouse transfer with id {id} already has inventory movements.");
    public static Error MissingPostingBatch(long id, short? languageId = null) => C("WarehouseTransfer.MissingPostingBatch", languageId,
        $"Id-si {id} bo'lgan ko'chirish uchun o'tkazmalar paketi topilmadi.", $"Id-си {id} бўлган кўчириш учун ўтказмалар пакети топилмади.",
        $"Для складского перемещения с id {id} не найден пакет проводок.", $"Posting batch was not found for warehouse transfer with id {id}.");
    public static Error OrganizationNotFound(int id, short? languageId = null) => EntityNotFound("OrganizationNotFound", id, "tashkilot", "ташкилот", "организация", "Organization", languageId);
    public static Error SourceWarehouseNotFound(int id, short? languageId = null) => EntityNotFound("SourceWarehouseNotFound", id, "jo'natuvchi ombor", "жўнатувчи омбор", "склад-отправитель", "Source warehouse", languageId);
    public static Error DestinationWarehouseNotFound(int id, short? languageId = null) => EntityNotFound("DestinationWarehouseNotFound", id, "qabul qiluvchi ombor", "қабул қилувчи омбор", "склад-получатель", "Destination warehouse", languageId);
    public static Error WarehousesMustDiffer(short? languageId = null) => B("WarehouseTransfer.WarehousesMustDiffer", languageId,
        "Jo'natuvchi va qabul qiluvchi omborlar turli bo'lishi kerak.", "Жўнатувчи ва қабул қилувчи омборлар турли бўлиши керак.",
        "Склад-отправитель и склад-получатель должны различаться.", "Source and destination warehouses must be different.");
    public static Error WarehouseOrganizationMismatch(int warehouseId, int organizationId, short? languageId = null) => B("WarehouseTransfer.WarehouseOrganizationMismatch", languageId,
        $"{warehouseId}-ombor {organizationId}-tashkilotga tegishli emas.", $"{warehouseId}-омбор {organizationId}-ташкилотга тегишли эмас.",
        $"Склад {warehouseId} не относится к организации {organizationId}.", $"Warehouse {warehouseId} does not belong to organization {organizationId}.");
    public static Error WarehouseInactive(int warehouseId, short? languageId = null) => B("WarehouseTransfer.WarehouseInactive", languageId,
        $"{warehouseId}-ombor faol emas.", $"{warehouseId}-омбор фаол эмас.", $"Склад {warehouseId} неактивен.", $"Warehouse {warehouseId} is not active.");
    public static Error ProductNotFound(int id, short? languageId = null) => EntityNotFound("ProductNotFound", id, "mahsulot", "маҳсулот", "товар", "Product", languageId);
    public static Error ProductServiceNotAllowed(int productId, short? languageId = null) => B("WarehouseTransfer.ProductServiceNotAllowed", languageId,
        $"{productId}-xizmat mahsulotini omborlararo ko'chirishda ishlatib bo'lmaydi.", $"{productId}-хизмат маҳсулотини омборлараро кўчиришда ишлатиб бўлмайди.",
        $"Товар-услугу {productId} нельзя использовать в складском перемещении.", $"Service product {productId} cannot be used in warehouse transfer.");
    public static Error UnitNotFound(short id, short? languageId = null) => EntityNotFound("UnitNotFound", id, "o'lchov birligi", "ўлчов бирлиги", "единица измерения", "Unit", languageId);
    public static Error InvalidQuantity(int productId, decimal quantity, short? languageId = null) => B("WarehouseTransfer.InvalidQuantity", languageId,
        $"{productId}-mahsulotda {quantity} miqdor noto'g'ri.", $"{productId}-маҳсулотда {quantity} миқдор нотўғри.",
        $"У товара {productId} недопустимое количество {quantity}.", $"Product {productId} has invalid quantity {quantity}.");
    public static Error ItemsRequired(int productId, short? languageId = null) => B("WarehouseTransfer.ItemsRequired", languageId,
        $"{productId}-mahsulotda kamida bitta partiya birligi bo'lishi kerak.", $"{productId}-маҳсулотда камида битта партия бирлиги бўлиши керак.",
        $"Товар {productId} должен содержать хотя бы один экземпляр партии.", $"Product {productId} must contain at least one product table item.");
    public static Error QuantityItemsMismatch(int productId, decimal quantity, int itemCount, short? languageId = null) => B("WarehouseTransfer.QuantityItemsMismatch", languageId,
        $"{productId}-mahsulot miqdori {quantity} birliklar soni {itemCount} ga mos emas.", $"{productId}-маҳсулот миқдори {quantity} бирликлар сони {itemCount} га мос эмас.",
        $"Количество товара {productId} ({quantity}) не совпадает с числом экземпляров ({itemCount}).", $"Product {productId} quantity {quantity} does not match item count {itemCount}.");
    public static Error DuplicateProductTable(int id, short? languageId = null) => C("WarehouseTransfer.DuplicateProductTable", languageId,
        $"{id}-partiya ko'chirishda takrorlangan.", $"{id}-партия кўчиришда такрорланган.", $"Партия {id} повторяется в перемещении.", $"Product table {id} is duplicated in the transfer.");
    public static Error ProductTableNotFound(int id, short? languageId = null) => EntityNotFound("ProductTableNotFound", id, "partiya", "партия", "партия", "Product table", languageId);
    public static Error ProductTableProductMismatch(int tableId, int productId, short? languageId = null) => B("WarehouseTransfer.ProductTableProductMismatch", languageId,
        $"{tableId}-partiya {productId}-mahsulotga tegishli emas.", $"{tableId}-партия {productId}-маҳсулотга тегишли эмас.",
        $"Партия {tableId} не относится к товару {productId}.", $"Product table {tableId} does not belong to product {productId}.");
    public static Error ProductTableInactive(int id, short? languageId = null) => B("WarehouseTransfer.ProductTableInactive", languageId,
        $"{id}-partiya faol emas.", $"{id}-партия фаол эмас.", $"Партия {id} неактивна.", $"Product table {id} is not active.");
    public static Error ProductTableUnavailable(int id, short statusId, short? languageId = null) => B("WarehouseTransfer.ProductTableUnavailable", languageId,
        $"{id}-partiyani {statusId} holatida ko'chirib bo'lmaydi.", $"{id}-партияни {statusId} ҳолатида кўчириб бўлмайди.",
        $"Партия {id} недоступна для перемещения в статусе {statusId}.", $"Product table {id} is not available for transfer in status {statusId}.");
    public static Error ProductTableWarehouseMismatch(int tableId, int warehouseId, short? languageId = null) => B("WarehouseTransfer.ProductTableWarehouseMismatch", languageId,
        $"{tableId}-partiya {warehouseId}-omborda joylashmagan.", $"{tableId}-партия {warehouseId}-омборда жойлашмаган.",
        $"Партия {tableId} не находится на складе {warehouseId}.", $"Product table {tableId} is not located in warehouse {warehouseId}.");

    private static Error InvalidStatus(string suffix, long id, short statusId, string uzAction, string cyAction, string ruAction, string enAction, short? languageId) =>
        B($"WarehouseTransfer.{suffix}", languageId, $"Id-si {id} bo'lgan ko'chirishni {statusId} holatida {uzAction} mumkin emas.",
            $"Id-си {id} бўлган кўчиришни {statusId} ҳолатида {cyAction} мумкин эмас.",
            $"Складское перемещение с id {id} нельзя {ruAction} в статусе {statusId}.",
            $"Warehouse transfer with id {id} cannot be {enAction} in status {statusId}.");
    private static Error EntityNotFound(string suffix, int id, string uzName, string cyName, string ruName, string enName, short? languageId) =>
        N($"WarehouseTransfer.{suffix}", languageId, $"Id-si {id} bo'lgan {uzName} topilmadi.", $"Id-си {id} бўлган {cyName} топилмади.",
            $"{ruName} с id {id} не найден.", $"{enName} with id {id} was not found.");
    private static Error B(string code, short? languageId, string uz, string cy, string ru, string en) => Error.Business(code, M(languageId, uz, cy, ru, en));
    private static Error C(string code, short? languageId, string uz, string cy, string ru, string en) => Error.Conflict(code, M(languageId, uz, cy, ru, en));
    private static Error N(string code, short? languageId, string uz, string cy, string ru, string en) => Error.NotFound(code, M(languageId, uz, cy, ru, en));
    private static string M(short? languageId, string uz, string cy, string ru, string en) => languageId switch
    {
        LanguageIdConst.UZ => uz, LanguageIdConst.UZ_CYRL => cy, LanguageIdConst.RU => ru, _ => en
    };
}
