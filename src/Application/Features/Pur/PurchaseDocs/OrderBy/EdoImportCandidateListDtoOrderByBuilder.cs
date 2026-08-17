using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.PurchaseDocs;

public sealed class EdoImportCandidateListDtoOrderByBuilder : IOrderByBuilder<EdoImportCandidate, EdoImportCandidateListDto>
{
    public Func<IQueryable<EdoImportCandidateListDto>, IOrderedQueryable<EdoImportCandidateListDto>> Build() =>
        query => query.OrderByDescending(x => x.DocumentDate).ThenByDescending(x => x.Id);
}
