using SharedKernel.Results;

namespace Application.Features.Register.PostingEngines;

public interface IPostingContextValidator<in TDocument>
{
    Task<Result> ValidateAsync(TDocument document, CancellationToken ct = default);
}
