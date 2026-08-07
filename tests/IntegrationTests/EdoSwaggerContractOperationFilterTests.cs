using Application.Abstractions.Integration.Edo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text.Json;
using WebApi.Controllers.Integration;
using WebApi.Infrastructure;

namespace IntegrationTests;

public sealed class EdoSwaggerContractOperationFilterTests
{
    [Fact]
    public void JsonOperationUsesJsonContentAndApprovedResponsesWithoutExamples()
    {
        var operation = CreateOperation("text/plain");

        Apply(operation, "api/edo/inbox");

        Assert.Equal(
            ["200", "400", "401", "403", "404", "422", "500", "501", "502"],
            operation.Responses!.Keys.OrderBy(key => key, StringComparer.Ordinal).ToArray());
        Assert.All(operation.Responses!.Values, response =>
        {
            var mediaType = Assert.Single(response.Content!);
            Assert.Equal("application/json", mediaType.Key);
            Assert.Null(mediaType.Value.Example);
        });
    }

    [Fact]
    public void FileOperationUsesBinarySuccessAndJsonErrors()
    {
        var operation = CreateOperation("text/plain");

        Apply(operation, "api/edo/files/{id}");

        Assert.Contains("application/pdf", operation.Responses!["200"]!.Content!.Keys);
        Assert.Contains("application/octet-stream", operation.Responses!["200"]!.Content!.Keys);
        foreach (var status in operation.Responses!.Keys.Where(status => status != "200"))
            Assert.Contains("application/json", operation.Responses[status]!.Content!.Keys);
    }

    [Fact]
    public void ErrorSchemaContainsOnlyPublicProblemFields()
    {
        var operation = CreateOperation("application/json");

        Apply(operation, "api/edo/documents/{id}");

        Assert.Equal(
            ["correlationId", "detail", "status", "title"],
            operation.Responses!["404"]!.Content!["application/json"].Schema!.Properties!.Keys
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToArray());
    }

    [Fact]
    public void ControllerDeclaresPublicJsonSuccessDtos()
    {
        var expected = new Dictionary<string, Type>(StringComparer.Ordinal)
        {
            ["GetActiveProvider"] = typeof(EdoProviderSelectionDto),
            ["GetCapabilities"] = typeof(EdoCapabilitiesResponseDto),
            ["GetInbox"] = typeof(EdoPagedDocumentResponse),
            ["GetOutbox"] = typeof(EdoPagedDocumentResponse),
            ["GetRemoteOutboxStatus"] = typeof(EdoProviderDocumentStatusResponseDto),
            ["GetAllDocuments"] = typeof(EdoPagedDocumentResponse),
            ["GetDocumentDetails"] = typeof(EdoDocumentDto),
            ["GetInboxSummary"] = typeof(EdoPublicInboxSummaryDto),
            ["GetOutboxStatus"] = typeof(EdoDocumentStatusDto),
            ["GetInboxStatus"] = typeof(EdoDocumentStatusDto)
        };

        foreach (var entry in expected)
        {
            var method = typeof(EdoController).GetMethod(entry.Key)!;
            var attribute = method.GetCustomAttributes<ProducesResponseTypeAttribute>()
                .Single(item => item.StatusCode == StatusCodes.Status200OK);
            Assert.Equal(entry.Value, attribute.Type);
        }
    }

    [Fact]
    public void FileActionDeclaresBinaryMediaTypes()
    {
        var attribute = typeof(EdoController).GetMethod(nameof(EdoController.GetFile))!
            .GetCustomAttributes<ProducesAttribute>()
            .Single();

        Assert.Equal(
            ["application/pdf", "application/octet-stream"],
            attribute.ContentTypes.ToArray());
    }

    [Fact]
    public void PublicPagedAndDocumentResponsesDoNotExposeInternalFields()
    {
        Assert.Equal(
            ["HasNextPage", "HasPreviousPage", "Items", "Page", "PageSize", "TotalCount", "TotalPages"],
            typeof(EdoPagedDocumentResponse).GetProperties()
                .Select(property => property.Name)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray());

        using var raw = JsonDocument.Parse("{\"value\":1}");
        var json = JsonSerializer.Serialize(new EdoDocumentDto
        {
            ProviderFields = new Dictionary<string, JsonElement>
            {
                ["internalRaw"] = raw.RootElement.Clone()
            }
        });

        Assert.DoesNotContain("providerFields", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("internalRaw", json, StringComparison.OrdinalIgnoreCase);
    }

    private static OpenApiOperation CreateOperation(string mediaType)
    {
        var response = new OpenApiResponse { Description = "initial" };
        response.Content = new Dictionary<string, OpenApiMediaType>();
        response.Content![mediaType] = new OpenApiMediaType { Schema = new OpenApiSchema() };
        return new OpenApiOperation
        {
            Responses = new OpenApiResponses { ["200"] = response }
        };
    }

    private static void Apply(OpenApiOperation operation, string path)
    {
        var apiDescription = new ApiDescription
        {
            HttpMethod = "GET",
            RelativePath = path
        };
        var context = new OperationFilterContext(
            apiDescription,
            null!,
            null!,
            new OpenApiDocument(),
            typeof(EdoController).GetMethod(nameof(EdoController.GetInbox))!);
        new EdoSwaggerContractOperationFilter().Apply(operation, context);
    }
}
