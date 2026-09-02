namespace Application.Features.Cmn.CurrencyRevaluations;

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
