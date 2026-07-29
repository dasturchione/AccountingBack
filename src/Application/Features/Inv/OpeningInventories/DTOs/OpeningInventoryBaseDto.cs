namespace Application.Features.Inv.OpeningInventories
{
    public class OpeningInventoryBaseDto
    {
        //public string DocNumber { get; set; }

        public DateTime DocDate { get; set; }

        public int CounterpartyId { get; set; }

        public long? ContractId { get; set; }

        public int WarehouseId { get; set; }

        public decimal TotalAmount { get; set; }

        public string? Comment { get; set; }
    }
}
