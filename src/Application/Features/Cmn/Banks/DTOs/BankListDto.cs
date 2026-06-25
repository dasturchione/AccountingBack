namespace Application.Features.Banks;

public class BankListDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Mfo { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
}
