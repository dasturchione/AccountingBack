namespace Application.Features.Inv.OpeningInventories
{
    public class OpeningInventoryCreateDto : OpeningInventoryBaseDto
    {
        public List<OpeningInventoryProductCreateDto> Lines { get; set; } = new();
    }

    public class OpeningInventoryProductCreateDto : OpeningInventoryProductBaseDto
    {
        public List<OpeningInventoryTableCreateDto> Items { get; set; } = new();
    }

    public class OpeningInventoryTableCreateDto
    {
        public string MarkingNumber { get; set; } = null!;
        public string? SerialNumber { get; set; }
    }
}
