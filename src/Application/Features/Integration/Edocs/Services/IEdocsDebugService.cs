using System.Text.Json;

namespace Application.Features.Integration.Edocs.Services;

// VAQTINCHALIK — faqat qo'lda sinov uchun (6.3-bosqich). Productionga chiqmaydi
// Debug controller olib tashlangan; development raw tekshiruvlari uchun service ichki
// diagnostic sifatida saqlanadi.
// Haqiqiy hujjat tuzilishi o'qib chiqilgach, bu interfeys + amalga oshiruvi +
// controller BUTUNLAY o'chiriladi.
public interface IEdocsDebugService
{
    Task<JsonElement> GetDocumentAsync(string type, string id, CancellationToken ct = default);
}
