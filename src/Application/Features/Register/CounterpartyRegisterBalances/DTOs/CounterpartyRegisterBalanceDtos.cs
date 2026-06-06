namespace Application.Features.CounterpartyRegisterBalances;

public class CounterpartyRegisterBalanceBaseDto
{
    public int OrganizationId { get; set; }
    public short DocumentTypeId { get; set; }
    public long DocumentId { get; set; }
    public int CounterpartyId { get; set; }
    public short OperationTypeId { get; set; }
    public short CurrencyId { get; set; }
    public decimal Amount { get; set; }
    public DateTime DocDate { get; set; }
}

public class CounterpartyRegisterBalanceCreateDto : CounterpartyRegisterBalanceBaseDto { }

public class CounterpartyRegisterBalanceUpdateDto : CounterpartyRegisterBalanceBaseDto { }

public class CounterpartyRegisterBalanceDto : CounterpartyRegisterBalanceBaseDto
{
    public long Id { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CounterpartyRegisterBalanceListDto : CounterpartyRegisterBalanceDto { }
