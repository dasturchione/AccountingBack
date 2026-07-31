namespace Integration.Edocs.Configs;

public sealed class EdocsOptions
{
    public const string SectionName = "Edocs";

    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;

    // 6.3-bosqichda doc.edocs.uz frontend bundle'idan (index-H5tLYbOJ.js.download)
    // reverse-engineering orqali aniqlandi: /documents/ ostidagi barcha so'rovlarga
    // x-product sarlavhasi qo'shiladi. E-DOCS.pdf (rasmiy hujjat) buni umuman
    // qayd etmaydi — ularsiz so'rov rad etilishi mumkin. Mahsulot darajasidagi qiymat
    // (barcha tashkilotlar uchun bir xil) — shuning uchun shu yerda qoladi.
    //
    // PartnerId 6.5.7-bosqichda integration_credential jadvaliga ko'chirildi (tashkilotga
    // tegishli — har bir tashkilot Edocs bilan o'z shartnomasi bo'yicha ishlaydi).
    public string Product { get; set; } = "edocs.uz";
}
