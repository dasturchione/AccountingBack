namespace Application.Features.PurchaseDocs;

public class PurchaseDocUpdateDto : PurchaseDocBaseDto
{
    public List<PurchaseDocLineDto> Lines { get; set; } = new();
    public short StateId { get; set; }
}
