using Application.Features.Integration.AslBelgi.DTOs;
using System.Text.Json;

namespace Application.Features.Integration.AslBelgi.Signing;

public interface ICrptDocumentBodySerializer
{
    CrptUnsignedDocumentDto Create(JsonElement document);
}
