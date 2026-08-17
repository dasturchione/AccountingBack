using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.FiscalCashRegisters;

public sealed class FiscalCashRegisterListDtoOrderByBuilder : IOrderByBuilder<FiscalCashRegister, FiscalCashRegisterListDto>
{
    public Func<IQueryable<FiscalCashRegisterListDto>, IOrderedQueryable<FiscalCashRegisterListDto>> Build() =>
        query => query.OrderBy(x => x.Name).ThenBy(x => x.Id);
}
