using Application.Abstractions.Authentication;
using Application.Features.AccountingRegisterEntries;
using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.Register.AccountingRegisterEntries
{
    public class AccountingDispatcher
    {
        private readonly IUserContext _userContext;
        private readonly IAccountingDocumentHandler<SaleDoc> _saleHandler;
        private readonly IAccountingDocumentHandler<PurchaseDoc> _purchaseHandler;
        public AccountingDispatcher(IUserContext userContext,
                                    IAccountingDocumentHandler<PurchaseDoc> purchaseHandler,
                                    IAccountingDocumentHandler<SaleDoc> saleHandler)
        {
            _saleHandler = saleHandler;
            _userContext = userContext;
            _purchaseHandler = purchaseHandler;
        }

        public async Task<Result<List<AccountingRegisterEntry>>> ProcessAsync(object document)
        {
            return document switch
            {
                PurchaseDoc p => await _purchaseHandler.HandleAsync(p),
                SaleDoc s => await _saleHandler.HandleAsync(s),
                _ => Result.Failure<List<AccountingRegisterEntry>>(AccountingRegisterEntryErrors.UnsupportedDocumentType(_userContext.LanguageId))
            };
        }
    }
}
