using SharedKernel.Exceptions;

namespace Integration.Didox.Http;

/// <summary>
/// DidoxTokenCache'da tashkilot uchun amaldagi user-key token yo'q. Bu handler
/// avtomatik login QILMAYDI — chaqiruvchi avval DidoxAuthController orqali
/// (GET /auth/challenge → frontend/e-imzo bilan imzolash → POST /auth/complete)
/// yangi token olishi kerak (EdocsAuthenticationRequiredException bilan bir xil naqsh).
/// </summary>
public sealed class DidoxAuthenticationRequiredException : IntegrationHttpException
{
    public DidoxAuthenticationRequiredException(string message) : base(message, 401)
    {
    }
}
