namespace Application.Features.Acc.OpeningBalances
{
    public class OpeningBalanceDto
    {
        public long Id { get; set; }

        public int OrganizationId { get; set; }

        public DateOnly BalanceDate { get; set; }

        public string? Description { get; set; }

        public short StateId { get; set; }

        public DateTime CreatedDate { get; set; }

        public string OrganizationName { get; set; } = null!;

        public string StateName { get; set; } = null!;

        public List<OpeningBalanceAccountDto> Accounts { get; set; } = new();
    }

    public class OpeningBalanceAccountDto
    {
        public long Id { get; set; }

        public int ChartAccountId { get; set; }

        public decimal DebitAmount { get; set; }

        public decimal CreditAmount { get; set; }
        
        public DateTime CreatedDate { get; set; }

        public string ChartAccountName { get; set; } = null!;

        public string? ChartAccountNumber { get; set; } 

        public string? ChartAccountCode { get; set; } 
    }
}
