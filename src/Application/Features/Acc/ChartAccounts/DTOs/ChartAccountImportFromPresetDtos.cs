using System.Text.Json.Serialization;

namespace Application.Features.ChartAccounts;

public class ChartAccountImportFromPresetRequestDto
{
    [JsonPropertyName("preset_account_id")]
    public int PresetAccountId { get; set; }
}

public class ChartAccountImportFromPresetResultDto
{
    public int PresetAccountId { get; set; }
    public int ChartAccountId { get; set; }
    public string Number { get; set; } = null!;
    public bool WasCreated { get; set; }
}
