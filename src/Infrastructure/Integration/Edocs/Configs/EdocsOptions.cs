namespace Integration.Edocs.Configs;

public sealed class EdocsOptions
{
    public const string SectionName = "Edocs";

    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;

    // 6.3-bosqichda doc.edocs.uz frontend bundle'idan (index-H5tLYbOJ.js.download)
    // reverse-engineering orqali aniqlandi: /documents/ ostidagi barcha so'rovlarga
    // bu ikkita sarlavha qo'shiladi. E-DOCS.pdf (rasmiy hujjat) bularni umuman
    // qayd etmaydi — ularsiz so'rov rad etilishi mumkin.
    // ESLATMA: bu qiymatlar rasmiy ravishda bizga berilmagan — ochiq frontend
    // bundle'idan olingan. Edocs bizga alohida partner ID bersa, shu bilan
    // almashtirilishi kerak.
    public string Product { get; set; } = "edocs.uz";
    public string PartnerId { get; set; } = string.Empty;
}
