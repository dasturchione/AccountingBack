using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.FaReceipts;

public sealed class FaReceiptListDtoOrderByBuilder : IOrderByBuilder<FaReceiptDoc, FaReceiptListDto>
{
    public Func<IQueryable<FaReceiptListDto>, IOrderedQueryable<FaReceiptListDto>> Build() =>
        query => query.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.Id);
}
