using Domain.Entities;

namespace Application.Features.Register.PostingEngines
{
    public interface IPostingService
    {
        Task<List<AccountingRegisterEntry>> BuildEntriesAsync(List<PostingContext> contexts);
    }
}
