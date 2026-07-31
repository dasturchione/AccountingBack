namespace Integration.Edocs.Http;

public static class EdocsHttpClientNames
{
    public const string Client = "Edocs";

    // Auth handler'siz alohida client — GET /authId/{serialNumber} va POST /login
    // (ЭЦП handshake) uchun. Bu ikkalasi hali tokenga muhtoj emas — aksincha, ular
    // tokenni ANIQLAYDI — shuning uchun asosiy (Client) orqali yuborilsa
    // EdocsAuthorizationHandler token yo'qligi haqida xato tashlab yuborardi.
    public const string AuthClient = "EdocsAuth";
}
