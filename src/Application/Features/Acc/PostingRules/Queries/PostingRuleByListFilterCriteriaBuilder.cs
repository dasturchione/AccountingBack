using Application.Abstractions.Authentication;
using Domain.Entities;
using LinqKit;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Acc.PostingRules
{
    public class PostingRuleByListFilterCriteriaBuilder : ICriteriaBuilder<PostingRule, PostingRuleListFilter>
    {
        private readonly IUserContext _userContext;
        public PostingRuleByListFilterCriteriaBuilder(IUserContext userContext)
        {
            _userContext = userContext;
        }

        public Expression<Func<PostingRule, bool>> Build(PostingRuleListFilter options)
        {
            Expression<Func<PostingRule, bool>> predicate = x => true;

            predicate = predicate.And(x => x.OrganizationId == null ||
                                           x.OrganizationId == _userContext.OrganizationId);

            if (options.DocumentTypeId.HasValue)
            {
                predicate = predicate.And(x => x.DocumentTypeId == options.DocumentTypeId.Value);
            }

            return predicate;
        }
    }
}
