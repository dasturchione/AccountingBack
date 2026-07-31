namespace Integration.Didox.Http;

public static class DidoxHttpClientNames
{
    public const string Client = "Didox";

    // Auth handler'siz alohida client — POST /v1/dsvs/timestamp va
    // POST /v1/auth/{taxId}/token/{locale} (ЭЦП handshake) uchun. Bu ikkalasi hali
    // user-key tokeniga muhtoj emas — aksincha, ular tokenni ANIQLAYDI — shuning uchun
    // asosiy (Client) orqali yuborilsa DidoxAuthorizationHandler token yo'qligi haqida
    // xato tashlab yuborardi (EdocsHttpClientNames.AuthClient bilan bir xil naqsh).
    public const string AuthClient = "DidoxAuth";
}
