using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApi.Infrastructure;

public class CustomHeadersOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= [];

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-OrganizationId",
            In = ParameterLocation.Header,
            Required = false,
            Description = "Tashkilot ID si (ixtiyoriy)",
            Schema = new OpenApiSchema { Type = JsonSchemaType.Integer }
        });

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Language",
            In = ParameterLocation.Header,
            Required = false,
            Description = "Til kodi: uz, ru, en",
            Schema = new OpenApiSchema { Type = JsonSchemaType.String }
        });
    }
}
