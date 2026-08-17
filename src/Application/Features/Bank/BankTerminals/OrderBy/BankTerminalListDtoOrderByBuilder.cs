using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.BankTerminals;

public sealed class BankTerminalListDtoOrderByBuilder : IOrderByBuilder<BankTerminal, BankTerminalListDto>
{
    public Func<IQueryable<BankTerminalListDto>, IOrderedQueryable<BankTerminalListDto>> Build() =>
        query => query.OrderBy(x => x.Name).ThenBy(x => x.Id);
}
