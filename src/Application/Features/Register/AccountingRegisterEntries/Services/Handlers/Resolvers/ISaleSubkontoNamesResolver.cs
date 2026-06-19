using Domain.Entities;

namespace Application.Features.Register.AccountingRegisterEntries
{
    public interface ISaleSubkontoNamesResolver
    {
        Task<SaleSubkontoContext> FillSubkontoContext(SaleDoc document);
    }
}
