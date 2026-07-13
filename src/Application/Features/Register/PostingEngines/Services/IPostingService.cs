using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.Register.PostingEngines
{
    public interface IPostingService
    {
        Task<Result<List<AccountingRegisterEntry>>> BuildEntriesAsync(List<PostingContext> contexts);
    }
}
