using System.Text.Json.Serialization;

namespace Application.Abstractions.Integration.Models;

public class CompanyBranchDto
{
    [JsonPropertyName("Id")]   public int    Id   { get; set; }
    [JsonPropertyName("Name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("Code")] public string Code { get; set; } = string.Empty;
}
