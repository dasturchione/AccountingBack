namespace Application.Features.PurchaseDocs;

public class PurchaseDocCreateDto : PurchaseDocBaseDto
{
    public List<PurchaseDocLineDto> Lines { get; set; } = new();
    public PurchaseProcessingMode ProcessingMode { get; set; } = PurchaseProcessingMode.StepByStep;
}

public enum PurchaseProcessingMode
{
    StepByStep = 1,
    Immediate = 2
}