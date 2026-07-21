namespace Application.Features.Acc.OpeningBalances
{
    public class OpeningBalanceAccountSaveDto
    {
        public long? Id { get; set; }

        public int ChartAccountId { get; set; }

        public List<OpeningBalanceAccountDetailSaveDto> Details { get; set; } = new();
    }

    public class OpeningBalanceAccountDetailSaveDto
    {
        public long? Id { get; set; }

        public decimal DebitAmount { get; set; }

        public decimal CreditAmount { get; set; }

        public decimal? Quantity { get; set; }

        public short CurrencyId { get; set; }

        public decimal? CurrencyAmount { get; set; }

        public decimal? ExchangeRate { get; set; }

        public string? Description { get; set; }

        public List<OpeningBalanceAccountDetailSubkontoSaveDto> Subkontos { get; set; } = new();
    }

    public class OpeningBalanceAccountDetailSubkontoSaveDto
    {
        public short SubkontoTypeId { get; set; }

        public long SubkontoId { get; set; }
    }
}
