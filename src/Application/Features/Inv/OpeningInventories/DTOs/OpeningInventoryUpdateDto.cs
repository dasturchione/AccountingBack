namespace Application.Features.Inv.OpeningInventories
{
    public class OpeningInventoryUpdateDto : OpeningInventoryBaseDto
    {
        public List<OpeningInventoryProductCreateDto> Lines { get; set; } = new();
    }
}
