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
            var correlationId = httpContext.TraceIdentifier;

            if (exception is IntegrationHttpException integrationException)
            {
                logger.LogError(
                    "Unhandled integration exception {ExceptionType} with status {StatusCode} for correlation {CorrelationId}",
                    integrationException.GetType().Name,
                    integrationException.StatusCode,
                    correlationId);
            }
            else
            {
                logger.LogError(
                    "Unhandled exception {ExceptionType} for correlation {CorrelationId}",
                    exception.GetType().Name,
                    correlationId);
            }

            var problemDetails = exception switch
            {
                EdoOrganizationScopeRequiredException => new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                    Title = "OrganizationScopeRequired",
                    Detail = "A current organization scope is required for this request."
                },
                EdoActiveProviderNotConfiguredException => new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    Title = "EdoActiveProviderNotConfigured",
                    Detail = "An active EDO provider is not configured for the current organization."
                },
                EdoCredentialNotConfiguredException => new ProblemDetails
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.21",
                    Title = "CredentialNotConfigured",
                    Detail = "Organization-scoped credentials are not configured for the selected EDO provider."
                },
                EdoProviderNotFoundException => new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    Title = "EdoProviderNotFound",
                    Detail = "The requested EDO provider is not registered."
                },
                EdoCapabilityUnavailableException => new ProblemDetails
                {
                    Status = StatusCodes.Status501NotImplemented,
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.6.2",
                    Title = "EdoCapabilityUnavailable",
                    Detail = "The requested EDO capability is not available."
                },
                EdoAuthSigningSessionException => new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                    Title = "EdoAuthSigningSessionInvalid",
                    Detail = "The EDO authentication signing session is missing, expired, scoped differently, or already used."
                },
                OptimisticConcurrencyException concurrencyException => new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                    Title = "OptimisticConcurrencyConflict",
                    Detail = "The request could not be completed because of a concurrency conflict."
                },
                UniqueConstraintViolationException uniqueConstraintException => new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                    Title = "UniqueConstraintViolation",
                    Detail = "The request could not be completed because it conflicts with existing data."
                },
                IntegrationUnauthorizedException unauthorizedException => new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                    Title = "IntegrationUnauthorized",
                    Detail = "The integration provider rejected the credentials."
                },
                IntegrationForbiddenException forbiddenException => new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                    Title = "IntegrationForbidden",
                    Detail = "The integration provider denied the request."
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
                    Detail = "The integration provider could not complete the request."
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
