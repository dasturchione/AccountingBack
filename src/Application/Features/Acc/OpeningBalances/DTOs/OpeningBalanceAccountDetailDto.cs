namespace Application.Features.Acc.OpeningBalances
{
    public class OpeningBalanceDetailDto
    {
        public long Id { get; set; }

        public int ChartAccountId { get; set; }

        public decimal DebitAmount { get; set; }

        public decimal CreditAmount { get; set; }

        public DateTime CreatedDate { get; set; }

        public string ChartAccountName { get; set; } = null!;

        public string? ChartAccountNumber { get; set; }

        public string? ChartAccountCode { get; set; }

        public List<OpeningBalanceAccountDetailDto> Details { get; set; } = new();
    }

    public class OpeningBalanceAccountDetailDto
    {
        public long Id { get; set; }

        public long OpeningBalanceAccountId { get; set; }

        public decimal DebitAmount { get; set; }

        public decimal CreditAmount { get; set; }

        public decimal? Quantity { get; set; }

        public short CurrencyId { get; set; }

        public decimal? CurrencyAmount { get; set; }

        public decimal? ExchangeRate { get; set; }

        public string? Description { get; set; }

        public int SortOrder { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CurrencyName { get; set; } = null!;

        public string CurrencyCode { get; set; } = null!;

        public virtual List<OpeningBalanceAccountDetailSubkontoDto> Subkontos { get; set; } = new();
    }

    public class OpeningBalanceAccountDetailSubkontoDto
    {

        public short SubkontoTypeId { get; set; }

        public long SubkontoId { get; set; }

        public short SortOrder { get; set; }

        public DateTime CreatedDate { get; set; }

        public string SubkontoTypeName { get; set; } = null!;

        public string SubkontoTypeCode { get; set; } = null!;
    }
}
