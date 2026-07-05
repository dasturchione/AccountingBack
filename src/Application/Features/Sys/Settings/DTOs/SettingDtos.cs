namespace Application.Features.Settings;

public sealed class SettingDto
{
    public string Code { get; set; } = null!;
    public string? Value { get; set; }
    public short ValueType { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public bool IsEditable { get; set; }
}

public sealed class SettingUpdateDto
{
    public string? Value { get; set; }
}
