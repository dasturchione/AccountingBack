using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Acc.PostingRules;

public class PostingRuleListDtoProjection : IProjectionBuilder<PostingRule, PostingRuleListDto>
{
    public Expression<Func<PostingRule, PostingRuleListDto>> Build()
    {
        return x => new PostingRuleListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            DocumentTypeId = x.DocumentTypeId,
            OperationTypeId = x.OperationTypeId,
            Code = x.Code,
            Name = x.Name,
            StateId = x.StateId,
            CreatedDate = x.CreatedDate,
            DocumentTypeName = x.DocumentType.Name,
            OperationTypeName = x.OperationType != null ? x.OperationType.Name : null,
            OrganizationName = x.Organization != null ? x.Organization.ShortName : null,
            StateName = x.State.FullName
        };
    }
}
