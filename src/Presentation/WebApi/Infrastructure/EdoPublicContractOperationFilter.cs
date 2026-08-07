using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApi.Infrastructure;

public sealed class EdoPublicContractOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var relativePath = context.ApiDescription.RelativePath?.TrimEnd('/');
        if (!string.Equals(relativePath, "api/edo/outbox", StringComparison.OrdinalIgnoreCase)
            || !string.Equals(context.ApiDescription.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase)
            || operation.Parameters is null)
            return;

        for (var index = operation.Parameters.Count - 1; index >= 0; index--)
        {
            var parameter = operation.Parameters[index];
            if (parameter.In == ParameterLocation.Query
                && (string.Equals(parameter.Name, "scope", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(parameter.Name, "limit", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(parameter.Name, "providerFilters", StringComparison.OrdinalIgnoreCase)))
            {
                operation.Parameters.RemoveAt(index);
            }
        }
    }
}
