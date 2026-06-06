namespace Application.Features.BankOperations;

public class BankOperationUpdateDto : BankOperationBaseDto
{
    public short StatusId { get; set; }
    public short StateId { get; set; }
}
