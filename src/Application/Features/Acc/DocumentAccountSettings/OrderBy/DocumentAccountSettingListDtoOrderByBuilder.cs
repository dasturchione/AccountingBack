using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.Acc.DocumentAccountSettings;

public sealed class DocumentAccountSettingListDtoOrderByBuilder : IOrderByBuilder<DocumentAccountType, DocumentAccountSettingListDto>
{
    public Func<IQueryable<DocumentAccountSettingListDto>, IOrderedQueryable<DocumentAccountSettingListDto>> Build() =>
        query => query.OrderBy(x => x.DocumentTypeId);
}
