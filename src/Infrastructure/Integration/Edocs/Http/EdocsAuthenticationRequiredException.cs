using SharedKernel.Exceptions;

namespace Integration.Edocs.Http;

/// <summary>
/// EdocsTokenCache'da amaldagi token yo'q va handler avtomatik qayta login QILMAYDI
/// (6.2-bosqichdan boshlab). Chaqiruvchi /auth/challenge + /auth/complete orqali
/// (frontend/e-imzo bilan) yangi token olishi kerak.
/// </summary>
public sealed class EdocsAuthenticationRequiredException : IntegrationHttpException
{
    public EdocsAuthenticationRequiredException(string message) : base(message, 401)
    {
    }
}
