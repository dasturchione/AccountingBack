namespace Application.Features.Inv.OpeningInventories
{
    public class OpeningInventoryProductBaseDto
    {
        public int ProductId { get; set; }

        public decimal Quantity { get; set; }

        public short UnitId { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Amount { get; set; }

        public int DebitAccountId { get; set; }
    }
}
