using Domain.Entities;

namespace Application.Features.Register.AccountingRegisterEntries
{
    public interface IPurchaseSubkontoNamesResolver
    {
        Task<PurchaseSubkontoContext> FillSubkontoContext(PurchaseDoc document);
    }
}
