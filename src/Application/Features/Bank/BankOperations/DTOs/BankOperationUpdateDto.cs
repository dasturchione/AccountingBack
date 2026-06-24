namespace Application.Features.BankOperations;

public class BankOperationUpdateDto : BankOperationBaseDto
{
    public short? PaymentTypeId { get; set; }
    public string DocNumber { get; set; } = null!;
    public short StatusId { get; set; }
    public short StateId { get; set; }
}
