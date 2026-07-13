namespace Application.Features.PurchaseDocs;

public class PurchaseDocCreateDto : PurchaseDocBaseDto
{
    public PurchaseProcessingMode ProcessingMode { get; set; } = PurchaseProcessingMode.StepByStep;
}

public enum PurchaseProcessingMode
{
    StepByStep = 1,
    Immediate = 2
}