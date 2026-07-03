namespace Application.Features.Reposting;

public class RepostReadRequest
{
    public int OrganizationId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public short? DocumentType { get; set; }
    public long? DocumentId { get; set; }
}

public class RepostCandidate
{
    public short DocumentType { get; set; }
    public long DocumentId { get; set; }
    public DateTime DocDate { get; set; }
}
