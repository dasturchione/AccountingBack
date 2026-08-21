namespace Application.Features.SaleDocs
{
    public class SaleDocAvailableProductDto
    {
        public long SaleDocProductId { get; set; }

        public int ProductId { get; set; }

        public List<SaleDocAvailableProductBatchDto> Batches { get; set; } = new();
    }

    public class SaleDocAvailableProductBatchDto
    {
        public long BatchId { get; set; }
        public string? BatchNumber { get; set; }
        public DateTime BatchDate { get; set; }
        public bool IsRequired { get; set; }
        public List<SaleDocAvailableProductTableDto> ProductTables { get; set; } = new();
    }

    public class SaleDocAvailableProductTableDto
    {
        public int ProductTableId { get; set; }
        public bool HasMarking { get; set; }
        public int MarkingCount { get; set; }
        public string AvailabilityStatus { get; set; } = "AVAILABLE";
    }
}
