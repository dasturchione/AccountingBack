namespace Application.Features.PurchaseServices;

public class PurchaseServiceBaseDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int ServiceTypeId { get; set; }
}
