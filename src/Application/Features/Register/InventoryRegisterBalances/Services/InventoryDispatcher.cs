using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.InventoryRegisterBalances;

public class InventoryDispatcher : IInventoryDispatcher
{
    private readonly IInventoryDocumentHandler<PurchaseDoc> _purchaseHandler;
    private readonly IInventoryDocumentHandler<SaleDoc> _saleHandler;
    private readonly ICommandRepository<RegisterBalance> _command;

    public InventoryDispatcher(IInventoryDocumentHandler<PurchaseDoc> purchaseHandler,
                               IInventoryDocumentHandler<SaleDoc> saleHandler,
                               ICommandRepository<RegisterBalance> command)
    {
        _purchaseHandler = purchaseHandler;
        _saleHandler     = saleHandler;
        _command         = command;
    }

    public async Task<Result<List<RegisterBalance>>> ProcessAsync(object document, CancellationToken ct = default)
    {
        var result = document switch
        {
            PurchaseDoc p => await _purchaseHandler.HandleAsync(p, ct),
            SaleDoc s     => await _saleHandler.HandleAsync(s, ct),
            _             => Result.Failure<List<RegisterBalance>>(InventoryRegisterBalanceErrors.UnsupportedDocumentType())
        };

        if (!result.IsSuccess)
            return result;

        await _command.CreateAsync(result.Value, ct);
        return result;
    }
}
