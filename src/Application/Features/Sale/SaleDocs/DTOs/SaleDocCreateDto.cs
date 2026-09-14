namespace Application.Features.SaleDocs;

public class SaleDocCreateDto
{
    public DateTime? DocDate { get; set; }
    public int CounterpartyId { get; set; }
    public int WarehouseId { get; set; }
    public short CurrencyId { get; set; }
    public decimal ExchangeRate { get; set; } = 1m;
    public long? ContractId { get; set; }
    public int? CustomerAccountId { get; set; }
    public int? VatAccountId { get; set; }
    public string? Comment { get; set; }

    /// <summary>Prices on this document already contain VAT, so it is extracted rather than added on top.</summary>
    public bool PriceIncludesVat { get; set; }

    public long? ShipmentId { get; set; }

    public SaleProcessingMode ProcessingMode { get; set; } = SaleProcessingMode.StepByStep;
    public List<SaleDocCreateProductDto> Lines { get; set; } = new();
}

public class SaleDocCreateProductDto
{
    public long? ShipmentProductId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public short UnitId { get; set; }
    public decimal UnitPrice { get; set; }
    public short? VatRateId { get; set; }
    public int? InventoryAccountId { get; set; }
    public int? IncomeAccountId { get; set; }
    public int? CostAccountId { get; set; }

    public bool Assembled { get; set; } = false;
    public List<SaleDocProductBatchDto> ProductBatches { get; set; } = new();
    public List<SaleDocCreateProductTableDto> Items { get; set; } = new();
}

public class SaleDocCreateProductTableDto
{
    public int ProductTableId { get; set; }
}

public class SaleDocProductBatchDto
{
    public long BatchId { get; set; }
    public decimal Quantity { get; set; }
}

public enum SaleProcessingMode
{
    StepByStep = 1,
    Immediate = 2
}