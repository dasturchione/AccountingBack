namespace Application.Features.Banks;

public class BankListDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? LegalName { get; set; }
    public string? LicenseNumber { get; set; }
    public DateOnly? LicenseDate { get; set; }
    public string? Inn { get; set; }
    public string? Website { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
}
