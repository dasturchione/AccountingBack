namespace Application.Features.Register.AccountingRegisterEntries
{
    public class PurchaseSubkontoContext
    {
        public long Id { get; set; }
        public DateTime DocDate { get; set; }
        public string DocNumber { get; set; } = null!;
        public int OrganizationId { get; set; }
        public short CurrencyId { get; set; }
        public int CounterpartyId { get; set; }
        public long? ContractId { get; set; }

        public int WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public string? CounterpartyName { get; set; }
        public string? ContractNumber { get; set; }
        public DateTime? ContractDate { get; set; }
        public List<ProductPurchaseSubkontoContext> Products { get; set; } = new();
    }

    public class ProductPurchaseSubkontoContext
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
        public decimal VatAmount { get; set; }
        public List<int> ProductTableIds { get; set; } = new();
    }
}
