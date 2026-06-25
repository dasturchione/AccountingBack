using SharedKernel.Results;

namespace Application.Features.Register.PostingEngines
{
    public interface IPostingContextDispatcher
    {
        Task<Result<List<PostingContext>>> ProcessAsync(object document, CancellationToken ct = default);
    }
}
