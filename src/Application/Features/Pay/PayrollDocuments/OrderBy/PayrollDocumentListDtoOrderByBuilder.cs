using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.Pay.PayrollDocuments;

public sealed class PayrollDocumentListDtoOrderByBuilder : IOrderByBuilder<PayPayrollDoc, PayrollDocumentListDto>
{
    public Func<IQueryable<PayrollDocumentListDto>, IOrderedQueryable<PayrollDocumentListDto>> Build() =>
        query => query.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.Id);
}
