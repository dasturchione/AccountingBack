namespace Application.Features.BankOperations;

public class BankOperationCreateDto : BankOperationBaseDto
{
    public short PaymentPurposeId { get; set; }
}
