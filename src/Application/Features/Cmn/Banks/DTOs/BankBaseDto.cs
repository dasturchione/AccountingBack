namespace Application.Features.Banks;

public class BankBaseDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Inn { get; set; }
    public string? Mfo { get; set; }
}
