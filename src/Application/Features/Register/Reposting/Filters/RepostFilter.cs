namespace Application.Features.Reposting;

public class RepostFilter
{
    public int? PeriodId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public short? DocumentType { get; set; }
    public long? DocumentId { get; set; }
}
