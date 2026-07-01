using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.Register.PostingEngines;

public interface IAccountingPostingValidator
{
    Result Validate(IReadOnlyCollection<AccountingRegisterEntry> entries);
}
