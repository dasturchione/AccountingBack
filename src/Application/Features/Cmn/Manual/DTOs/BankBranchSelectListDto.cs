namespace Application.Features.Manual;

public class BankBranchSelectListDto : SelectListDto
{
    public int BankId { get; set; }
    public string Mfo { get; set; } = null!;
}
