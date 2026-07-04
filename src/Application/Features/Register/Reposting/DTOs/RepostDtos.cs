namespace Application.Features.Reposting;

public class RepostDto
{
    public int ProcessedCount { get; set; }
    public List<RepostDocumentDto> Documents { get; set; } = [];
}

public class RepostDocumentDto
{
    public short DocumentType { get; set; }
    public long DocumentId { get; set; }
    public DateTime PostingDate { get; set; }
}
