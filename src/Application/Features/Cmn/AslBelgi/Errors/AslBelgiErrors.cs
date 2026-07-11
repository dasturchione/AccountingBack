using SharedKernel.Results;

namespace Application.Features.Cmn.AslBelgi.Errors;

public static class AslBelgiErrors
{
    public static Error MissingTin() =>
        new Error("AslBelgi.MissingTin", "ИНН (TIN) kiritilishi shart.", ErrorType.Validation);

    public static Error MissingAccessToken() =>
        new Error("AslBelgi.MissingAccessToken", "Authorization headerida Bearer token bo‘lishi shart.", ErrorType.Validation);

    public static Error MissingOrderPayload() =>
        new Error("AslBelgi.MissingOrderPayload", "Buyurtma ma’lumotlari yuborilishi kerak.", ErrorType.Validation);

    public static Error MissingOrderProducts() =>
        new Error("AslBelgi.MissingOrderProducts", "Zakazda kamida bitta tovar (podzakaz) bo‘lishi shart.", ErrorType.Validation);

    public static Error MissingGtin() =>
        new Error("AslBelgi.MissingGtin", "Har bir tovar uchun GTIN kiritilishi shart.", ErrorType.Validation);

    public static Error MissingOrderId() =>
        new Error("AslBelgi.MissingOrderId", "orderId kiritilishi shart.", ErrorType.Validation);

    public static Error IntegrationReturnedNoOrderId() =>
        new Error("AslBelgi.NoOrderId", "Asl Belgisi zakaz javobida orderId qaytmadi.", ErrorType.Problem);

    public static Error UserHasNoOrganization() =>
        new Error("AslBelgi.UserHasNoOrganization", "Foydalanuvchi tashkiloti aniqlanmadi.", ErrorType.Validation);

    public static Error ProductNotFound(int productId) =>
        Error.NotFound("AslBelgi.ProductNotFound", $"Tovar topilmadi (id={productId}).");

    // ⏳ WAITING POINT: GTIN buxgalter/GTIN registri tomonidan tovarga biriktirilishi kerak.
    public static Error MissingGtinForProduct(int productId) =>
        new Error("AslBelgi.MissingGtinForProduct",
            $"Tovar (id={productId}) uchun GTIN sozlanmagan. GTIN qiymatini tovarga biriktiring.", ErrorType.Validation);

    public static Error MissingRefreshIdentifier() =>
        new Error("AslBelgi.MissingRefreshIdentifier", "apiKey yoki id maydonlaridan kamida bittasi kiritilishi shart.", ErrorType.Validation);

    public static Error CredentialsNotConfigured(string detail) =>
        new Error("AslBelgi.CredentialsNotConfigured", detail, ErrorType.Problem);

    public static Error TokenAcquisitionFailed(string? detail) =>
        new Error(
            "AslBelgi.TokenAcquisitionFailed",
            string.IsNullOrWhiteSpace(detail) ? "Asl Belgisi token olishda xatolik." : detail,
            ErrorType.Problem);

    public static Error IntegrationReturnedError(string? message, string? details = null) =>
        new Error(
            "AslBelgi.IntegrationReturnedError",
            string.IsNullOrWhiteSpace(details)
                ? (message ?? "Asl Belgisi returned an error.")
                : $"{message}: {details}",
            ErrorType.Problem);
}
