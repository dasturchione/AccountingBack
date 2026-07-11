using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Exceptions;
using WebApi.Middlewares;

namespace WebApi.Infrastructure
{
    internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError(exception, "Unhandled exception occurred");
            var correlationId = httpContext.TraceIdentifier;

            var problemDetails = exception switch
            {
                OptimisticConcurrencyException concurrencyException => new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                    Title = "OptimisticConcurrencyConflict",
                    Detail = concurrencyException.Message
                },
                UniqueConstraintViolationException uniqueConstraintException => new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                    Title = "UniqueConstraintViolation",
                    Detail = uniqueConstraintException.Message
                },
                IntegrationUnauthorizedException unauthorizedException => new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                    Title = "IntegrationUnauthorized",
                    Detail = unauthorizedException.Message
                },
                IntegrationForbiddenException forbiddenException => new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                    Title = "IntegrationForbidden",
                    Detail = forbiddenException.Message
                },
                IntegrationHttpException integrationHttpException => new ProblemDetails
                {
                    Status = integrationHttpException.StatusCode is >= 100 and <= 599
                        ? integrationHttpException.StatusCode
                        : StatusCodes.Status502BadGateway,
                    Type = GetIntegrationType(integrationHttpException.StatusCode),
                    Title = integrationHttpException.StatusCode switch
                    {
                        StatusCodes.Status400BadRequest => "IntegrationBadRequest",
                        StatusCodes.Status422UnprocessableEntity => "IntegrationBusinessError",
                        >= 500 and <= 599 => "IntegrationServerError",
                        _ => "IntegrationError"
                    },
                    Detail = string.IsNullOrWhiteSpace(integrationHttpException.Message)
                        ? "Integration call failed with an empty error message."
                        : integrationHttpException.Message
                },
                _ => new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                    Title = "Server failure",
                    Detail = "An unexpected error occurred while processing the request."
                }
            };

            problemDetails.Extensions["correlationId"] = correlationId;
            httpContext.Response.Headers[CorrelationIdMiddleware.HeaderName] = correlationId;
            httpContext.Response.StatusCode = problemDetails.Status!.Value;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        private static string GetIntegrationType(int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status401Unauthorized => "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                StatusCodes.Status403Forbidden => "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                StatusCodes.Status422UnprocessableEntity => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                >= 500 and <= 599 => "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                _ => "https://tools.ietf.org/html/rfc9110#section-15.5.1"
            };
        }
    }
}
