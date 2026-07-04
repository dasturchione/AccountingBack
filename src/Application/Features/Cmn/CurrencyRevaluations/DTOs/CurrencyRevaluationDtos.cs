namespace Application.Features.Cmn.CurrencyRevaluations;

public class CurrencyRevaluationBaseDto
{
    public DateTime RevaluationDate { get; set; }
    public DateTime? ProviderRateDate { get; set; }
    public short? TargetCurrencyId { get; set; }
    public bool RevalueAllForeignCurrencies { get; set; } = true;
}

public sealed class CurrencyRevaluationCreateDto : CurrencyRevaluationBaseDto;
public sealed class CurrencyRevaluationConfirmDto { }
public sealed class CurrencyRevaluationPreviewDto : CurrencyRevaluationBaseDto;
public sealed class CurrencyRevaluationCancelDto { }

public class CurrencyRevaluationLineDto
{
    public short BaseCurrencyId { get; set; }
    public short TargetCurrencyId { get; set; }
    public string TargetCurrencyCode { get; set; } = null!;
    public decimal BalanceAmount { get; set; }
    public decimal OpeningRate { get; set; }
    public decimal CurrentRate { get; set; }
    public decimal DifferenceAmount { get; set; }
}

public class CurrencyRevaluationDto : CurrencyRevaluationBaseDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public short StatusId { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public List<CurrencyRevaluationLineDto> Lines { get; set; } = [];
}

public sealed class CurrencyRevaluationListDto : CurrencyRevaluationDto;
