namespace Application.Features.Acc.PostingRules;

public class PostingRuleUpdateDto : PostingRuleBaseDto
{
    public short StateId { get; set; }
    public List<PostingRuleLineUpdateDto> Lines { get; set; } = new();
}

public class PostingRuleLineUpdateDto : PostingRuleLineBaseDto
{
    public int? Id { get; set; }
    public int? StateId { get; set; }
}
