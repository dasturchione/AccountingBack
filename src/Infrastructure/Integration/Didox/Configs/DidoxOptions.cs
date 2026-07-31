namespace Integration.Didox.Configs;

public sealed class DidoxOptions
{
    public const string SectionName = "Didox";

    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;

    // Hamkor tokeni (Partner-Authorization) — PLATFORMA darajasida, barcha tashkilotlar
    // uchun bitta (INT_DIDOX.md §1.3/§2.1: har so'rovda majburiy, API orqali olinmaydi —
    // faqat Didox akkaunt menejeri orqali qo'lda beriladi). ATAYLAB ValidateOnStart'da
    // tekshirilmaydi (DidoxOptionsValidator) — bo'sh qiymat bilan ham ilova ishga tushishi
    // kerak; yo'qligi RUNTIME'da, so'rov yuborilayotganda aniq xato bilan chiqadi
    // (6.1-bosqichdagi Edocs darsi: yetishmayotgan majburiy config ValidateOnStart'da
    // butun ilovani ishga tushirmay qo'ygan edi — production'da 502 bergan).
    public string PartnerToken { get; set; } = string.Empty;
}
