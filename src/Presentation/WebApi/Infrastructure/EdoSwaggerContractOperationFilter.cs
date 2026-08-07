using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApi.Infrastructure;

public sealed class EdoSwaggerContractOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!string.Equals(context.ApiDescription.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
            return;

        if (operation.Responses is null)
            return;

        var path = context.ApiDescription.RelativePath?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(path) || (!IsSupportedJsonPath(path) && !IsFilePath(path)))
            return;

        var allowedStatuses = GetAllowedStatuses(path);

        foreach (var status in operation.Responses.Keys.ToArray())
        {
            if (!allowedStatuses.Contains(status))
                operation.Responses.Remove(status);
        }

        foreach (var status in allowedStatuses)
        {
            if (!operation.Responses.ContainsKey(status))
            {
                operation.Responses[status] = new OpenApiResponse
                {
                    Description = GetResponseDescription(status)
                };
            }
        }

        foreach (var responseEntry in operation.Responses)
        {
            var response = responseEntry.Value;
            var schema = response.Content?.Values
                .Select(mediaType => mediaType.Schema)
                .FirstOrDefault(candidate => candidate is not null);
            if (response.Content is null)
            {
                var replacement = new OpenApiResponse
                {
                    Description = response.Description ?? GetResponseDescription(responseEntry.Key)
                };
                replacement.Content = new Dictionary<string, OpenApiMediaType>(StringComparer.OrdinalIgnoreCase);
                response = replacement;
                operation.Responses[responseEntry.Key] = response;
            }

            response.Content!.Clear();
            if (IsFilePath(path) && string.Equals(responseEntry.Key, "200", StringComparison.OrdinalIgnoreCase))
            {
                response.Content["application/pdf"] = new OpenApiMediaType { Schema = schema };
                response.Content["application/octet-stream"] = new OpenApiMediaType { Schema = schema };
                continue;
            }

            if (!string.Equals(responseEntry.Key, "200", StringComparison.OrdinalIgnoreCase))
                schema = CreateProblemDetailsSchema();

            response.Content["application/json"] = new OpenApiMediaType
            {
                Schema = schema
            };
        }
    }

    private static string GetResponseDescription(string status) => status switch
    {
        "200" => "Successful response.",
        "400" => "The request is invalid or the organization scope is missing.",
        "401" => "Authentication is required or the provider credentials were rejected.",
        "403" => "The request is not allowed.",
        "404" => "The requested EDO resource was not found.",
        "422" => "The provider credential or business configuration is invalid.",
        "501" => "The requested EDO capability is unavailable.",
        "502" => "The EDO provider returned an invalid or unsuccessful response.",
        "500" => "An unexpected server error occurred.",
        _ => "Response."
    };

    private static OpenApiSchema CreateProblemDetailsSchema() => new()
    {
        Type = JsonSchemaType.Object,
        Properties = new Dictionary<string, IOpenApiSchema>
        {
            ["title"] = new OpenApiSchema { Type = JsonSchemaType.String },
            ["status"] = new OpenApiSchema { Type = JsonSchemaType.Integer },
            ["detail"] = new OpenApiSchema { Type = JsonSchemaType.String },
            ["correlationId"] = new OpenApiSchema { Type = JsonSchemaType.String }
        }
    };

    private static HashSet<string> GetAllowedStatuses(string path) =>
        path.Equals("api/edo/active-provider", StringComparison.OrdinalIgnoreCase)
        || path.Equals("api/edo/capabilities", StringComparison.OrdinalIgnoreCase)
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "200", "400", "401", "403", "404", "500" }
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "200", "400", "401", "403", "404", "422", "500", "501", "502" };

    private static bool IsSupportedJsonPath(string path) =>
        path.Equals("api/edo/active-provider", StringComparison.OrdinalIgnoreCase)
        || path.Equals("api/edo/capabilities", StringComparison.OrdinalIgnoreCase)
        || path.Equals("api/edo/inbox", StringComparison.OrdinalIgnoreCase)
        || path.Equals("api/edo/outbox", StringComparison.OrdinalIgnoreCase)
        || path.Equals("api/edo/outbox/status", StringComparison.OrdinalIgnoreCase)
        || path.Equals("api/edo/documents/all", StringComparison.OrdinalIgnoreCase)
        || path.Equals("api/edo/documents/{id}", StringComparison.OrdinalIgnoreCase)
        || path.Equals("api/edo/inbox/summary", StringComparison.OrdinalIgnoreCase)
        || path.Equals("api/edo/inbox/{id}/status", StringComparison.OrdinalIgnoreCase)
        || path.Equals("api/edo/outbox/{id}/status", StringComparison.OrdinalIgnoreCase);

    private static bool IsFilePath(string path) =>
        path.Equals("api/edo/files/{id}", StringComparison.OrdinalIgnoreCase);

}
