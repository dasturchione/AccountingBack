using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Acc.PostingRules;

public class PostingRuleDtoProjection : IProjectionBuilder<PostingRule, PostingRuleDto>
{
    public Expression<Func<PostingRule, PostingRuleDto>> Build()
    {
        return x => new PostingRuleDto
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
            StateName = x.State.FullName,
            Lines = x.PostingRuleLines.Where(s => s.StateId == StateIdConst.ACTIVE).Select(l => new PostingRuleLineDto
            {
                Id = l.Id,
                SortOrder = l.SortOrder,
                DebitAccountId = l.DebitAccountId,
                CreditAccountId = l.CreditAccountId,
                AmountSource = l.AmountSource,
                QuantitySource = l.QuantitySource,
                ContentTemplate = l.ContentTemplate,
                StateId = l.StateId,
                CreatedDate = l.CreatedDate,
                CreditAccountCode = l.CreditAccount != null ? l.CreditAccount.Code : null,
                CreditAccountName = l.CreditAccount != null ? l.CreditAccount.Name : null,
                DebitAccountCode = l.DebitAccount != null ? l.DebitAccount.Code : null,
                DebitAccountName = l.DebitAccount != null ? l.DebitAccount.Name : null,
                StateName = l.State.FullName
            }).ToList()
        };
    }
}
