using System.Net;

namespace Application.Abstractions.Authentication
{
    public interface IRequestContext
    {
        IPAddress? Ip { get; }
        string? UserAgent { get; }
    }
}
