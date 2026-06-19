using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.AccountingRegisterEntries;
using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.Register.AccountingRegisterEntries
{
    public class AccountingDispatcher : IAccountingDispatcher
    {
        private readonly IUserContext _userContext;
        private readonly IAccountingDocumentHandler<SaleDoc> _saleHandler;
        private readonly IAccountingDocumentHandler<PurchaseDoc> _purchaseHandler;
        private readonly ICommandRepository<AccountingRegisterEntry> _accountingRegisterCommand;
        public AccountingDispatcher(IUserContext userContext,
                                    IAccountingDocumentHandler<SaleDoc> saleHandler,
                                    IAccountingDocumentHandler<PurchaseDoc> purchaseHandler,
                                    ICommandRepository<AccountingRegisterEntry> accountingRegisterCommand)
        {
            _saleHandler = saleHandler;
            _userContext = userContext;
            _purchaseHandler = purchaseHandler;
            _accountingRegisterCommand = accountingRegisterCommand;
        }

        public async Task<Result<List<AccountingRegisterEntry>>> ProcessAsync(object document, CancellationToken ct = default)
        {
            var entryResults = document switch
            {
                PurchaseDoc p => await _purchaseHandler.HandleAsync(p, ct),
                SaleDoc s => await _saleHandler.HandleAsync(s, ct),
                _ => Result.Failure<List<AccountingRegisterEntry>>(AccountingRegisterEntryErrors.UnsupportedDocumentType(_userContext.LanguageId))
            };

            await _accountingRegisterCommand.CreateAsync(entryResults.Value, ct);

            return entryResults;
        }
    }
}
