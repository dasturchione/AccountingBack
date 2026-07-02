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
    }
}
