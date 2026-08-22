namespace SharedKernel.Exceptions;

public sealed class EdoOutboxProviderDocumentException : Exception
{
    public EdoOutboxProviderDocumentException(string code, int statusCode)
        : base(code)
    {
        Code = code;
        StatusCode = statusCode;
    }

    public string Code { get; }
    public int StatusCode { get; }
}
