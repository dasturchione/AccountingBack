using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApi.Infrastructure;

public sealed class EdoPublicContractOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var relativePath = context.ApiDescription.RelativePath?.TrimEnd('/');
        if (!string.Equals(context.ApiDescription.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase)
            || operation.Parameters is null
            || !IsPublicDocumentListPath(relativePath))
            return;

        for (var index = operation.Parameters.Count - 1; index >= 0; index--)
        {
            var parameter = operation.Parameters[index];
            if (parameter.In == ParameterLocation.Query
                && (string.Equals(parameter.Name, "scope", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(parameter.Name, "limit", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(parameter.Name, "providerFilters", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(parameter.Name, "search", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(parameter.Name, "hasMarks", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(parameter.Name, "dateFrom", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(parameter.Name, "dateTo", StringComparison.OrdinalIgnoreCase)))
            {
                operation.Parameters.RemoveAt(index);
            }
        }
    }

    private static bool IsPublicDocumentListPath(string? path) =>
        string.Equals(path, "api/edo/inbox", StringComparison.OrdinalIgnoreCase)
        || string.Equals(path, "api/edo/outbox", StringComparison.OrdinalIgnoreCase)
        || string.Equals(path, "api/edo/documents/all", StringComparison.OrdinalIgnoreCase);
}
