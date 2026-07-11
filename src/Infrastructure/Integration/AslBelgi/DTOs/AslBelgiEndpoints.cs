namespace Integration.AslBelgi.DTOs;

public static class AslBelgiEndpoints
{
    public const string CheckApiKey = "/public/api/v1/party/parties/{tin}/api-keys/check";
    public const string RefreshApiKey = "/public/api/v1/party/parties/{tin}/api-keys/refresh";
    public const string Orders = "/api/orders";
    public const string Codes = "/api/codes";
    public const string Documents = "/public/api/v1/doc/storage/docs/{documentId}";
    public const string Status = "/public/api/v1/doc/status/{id}";
}

