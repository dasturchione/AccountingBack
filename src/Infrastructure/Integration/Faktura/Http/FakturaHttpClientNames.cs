namespace Integration.Faktura.Http;

public static class FakturaHttpClientNames
{
    public const string Client = "Faktura";

    // Token endpointi alohida client orqali chaqiriladi: aks holda
    // authorization handler o'zini o'zi chaqirib rekursiyaga tushadi.
    public const string AuthClient = "FakturaAuth";
}
