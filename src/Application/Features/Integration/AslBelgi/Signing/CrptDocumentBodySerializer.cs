using Application.Features.Integration.AslBelgi.DTOs;
using System.Buffers;
using System.Text;
using System.Text.Json;

namespace Application.Features.Integration.AslBelgi.Signing;

public sealed class CrptDocumentBodySerializer : ICrptDocumentBodySerializer
{
    public CrptUnsignedDocumentDto Create(JsonElement document)
    {
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            WriteCanonicalJson(writer, document);
        }

        return new CrptUnsignedDocumentDto
        {
            DocumentBody = Convert.ToBase64String(buffer.WrittenSpan)
        };
    }

    private static void WriteCanonicalJson(Utf8JsonWriter writer, JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in element.EnumerateObject().OrderBy(x => x.Name, StringComparer.Ordinal))
                {
                    writer.WritePropertyName(property.Name);
                    WriteCanonicalJson(writer, property.Value);
                }

                writer.WriteEndObject();
                break;
            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray())
                    WriteCanonicalJson(writer, item);
                writer.WriteEndArray();
                break;
            default:
                element.WriteTo(writer);
                break;
        }
    }
}
