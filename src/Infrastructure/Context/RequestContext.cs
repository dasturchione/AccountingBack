using Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace Infrastructure.Context
{
    public class RequestContext : IRequestContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public RequestContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public IPAddress? Ip => GetIp();

        public string? UserAgent => GetUserAgent();

        private IPAddress? GetIp()
        {
            var http = _httpContextAccessor.HttpContext;

            var forwarded = http?.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            var realIp = http?.Request.Headers["X-Real-IP"].FirstOrDefault();

            var ipString = !string.IsNullOrWhiteSpace(forwarded)
                ? forwarded.Split(',')[0].Trim()
                : realIp ?? http?.Connection.RemoteIpAddress?.ToString();

            return IPAddress.TryParse(ipString, out var ip)
                ? ip
                : null;
        }

        private string? GetUserAgent()
        {
            var http = _httpContextAccessor.HttpContext;

            return http?.Request.Headers["User-Agent"].ToString() ?? "unknown";
        }
    }
}
