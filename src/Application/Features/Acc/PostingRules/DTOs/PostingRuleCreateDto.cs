namespace Application.Features.Acc.PostingRules;

public class PostingRuleCreateDto : PostingRuleBaseDto
{
    public short DocumentTypeId { get; set; }
    public List<PostingRuleLineCreateDto> Lines { get; set; } = new();
}

public class PostingRuleLineCreateDto : PostingRuleLineBaseDto
{
}
