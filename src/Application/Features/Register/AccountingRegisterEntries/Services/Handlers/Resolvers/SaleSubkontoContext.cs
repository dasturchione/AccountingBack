namespace Application.Features.Register.AccountingRegisterEntries
{
    public class SaleSubkontoContext
    {
        public long Id { get; set; }
        public DateTime DocDate { get; set; }
        public string DocNumber { get; set; } = null!;
        public int OrganizationId { get; set; }
        public short CurrencyId { get; set; }
        public int CounterpartyId { get; set; }

        public int? ClientId { get; set; }
        public string? ClientName { get; set; }

        public int WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public string? CounterpartyName { get; set; }

        public List<SaleProductSubkontoContext> Products { get; set; } = new();

        public List<SaleGroupedSubkontoContext> Tables => Products.GroupBy(g => g.VatRateId).Select(grouped => new SaleGroupedSubkontoContext
        {
            VatRateId = grouped.Key,
            Amount = grouped.Sum(s => s.Amount),
            VatAmount = grouped.Sum(s => s.VatAmount),
            Quantity = grouped.Sum(s => s.Quantity)
        }).ToList();
    }

    public class SaleGroupedSubkontoContext
    {
        public short? VatRateId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
        public decimal VatAmount { get; set; }
    }

    public class SaleProductSubkontoContext
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
        public decimal VatAmount { get; set; }
        public short? VatRateId { get; set; }
        public List<SaleProductPurchaseSubkontoContext> Purchases { get; set; } = new();
    }

    public class SaleProductPurchaseSubkontoContext
    {
        public int WarehouseId { get; set; }
        public string Warehouse { get; set; } = null!;
        public long PurchaseId { get; set; }
        public string PurchaseDocNumber { get; set; } = null!;
        public DateTime PurchaseDate { get; set; } 
        public decimal PurchaseAmount { get; set; }
        public decimal Quantity { get; set; }
    }
}
