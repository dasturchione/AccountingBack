using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.Register.AccountingRegisterEntries
{
    public interface IAccountingDocumentHandler<T>
    {
        Task<Result<List<AccountingRegisterEntry>>> HandleAsync(T document, CancellationToken ct = default);
    }
}
