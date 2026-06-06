namespace Application.Features.MoneyRegisterBalances;

public class MoneyRegisterBalanceBaseDto
{
    public int OrganizationId { get; set; }
    public short DocumentTypeId { get; set; }
    public long DocumentId { get; set; }
    public string SourceType { get; set; } = null!;
    public int SourceId { get; set; }
    public short OperationTypeId { get; set; }
    public short CurrencyId { get; set; }
    public decimal Amount { get; set; }
    public DateTime DocDate { get; set; }
}

public class MoneyRegisterBalanceCreateDto : MoneyRegisterBalanceBaseDto { }

public class MoneyRegisterBalanceUpdateDto : MoneyRegisterBalanceBaseDto { }

public class MoneyRegisterBalanceDto : MoneyRegisterBalanceBaseDto
{
    public long Id { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class MoneyRegisterBalanceListDto : MoneyRegisterBalanceDto { }
