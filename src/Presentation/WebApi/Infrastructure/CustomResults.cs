using SharedKernel.Results;

namespace WebApi.Infrastructure
{
    public static class CustomResults
    {
        public static IResult Problem(Result result)
        {
            if (result.IsSuccess)
            {
                throw new InvalidOperationException();
            }

            return Results.Problem(
                title: result.Error.Code, 
                detail: result.Error.Description, 
                type: GetType(result.Error.Type),
                statusCode: GetStatusCode(result.Error.Type));

            static string GetType(ErrorType errorType)
            {
                var rfcUrl = "https://tools.ietf.org/html/rfc9110";

                return errorType switch
                {
                    ErrorType.Validation => rfcUrl + "#section-15.5.1",
                    ErrorType.Unauthorized => rfcUrl + "#section-15.5.2",
                    ErrorType.Forbidden => rfcUrl + "#section-15.5.4",
                    ErrorType.NotFound => rfcUrl + "#section-15.5.5",
                    ErrorType.Conflict => rfcUrl + "#section-15.5.10",
                    ErrorType.Business => rfcUrl + "#section-15.5.21",
                    ErrorType.Timeout => rfcUrl + "#section-15.6.5",
                    _ => rfcUrl + "#section-15.6.1"
                };
            }

            static int GetStatusCode(ErrorType errorType) =>
                errorType switch
                {
                    ErrorType.Validation => StatusCodes.Status400BadRequest,
                    ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                    ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                    ErrorType.NotFound => StatusCodes.Status404NotFound,
                    ErrorType.Conflict => StatusCodes.Status409Conflict,
                    ErrorType.Business => StatusCodes.Status422UnprocessableEntity,
                    ErrorType.Timeout => StatusCodes.Status504GatewayTimeout,
                    _ => StatusCodes.Status500InternalServerError
                };
        }
    }
}
