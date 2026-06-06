namespace Application.Features.CashOperations;

public class CashOperationUpdateDto : CashOperationBaseDto
{
    public short StatusId { get; set; }
    public short StateId { get; set; }
}
