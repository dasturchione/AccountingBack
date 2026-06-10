using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.Register.AccountingRegisterEntries
{
    public interface IAccountingDispatcher
    {
        Task<Result<List<AccountingRegisterEntry>>> ProcessAsync(object document, CancellationToken ct = default);
    }
}
