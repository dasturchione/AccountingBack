namespace Integration.AslBelgi.Configs;

public sealed class AslBelgiOptions
{
    public const string SectionName = "AslBelgi";

    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Tin { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;

    // Vaqtincha global qiymat (PROCESS5_AUDIT.md, 4-band): tashkilot hozircha faqat bitta CRPT
    // tovar guruhida ishlaydi. Agar kelajakda bir nechta guruh kerak bo'lsa, bu qiymat
    // inv_product/inv_product_group darajasiga ko'chirilishi kerak.
    public string ProductGroup { get; set; } = string.Empty;
}
