using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.InventoryRegisterBalances;

public class InventoryDispatcher : IInventoryDispatcher
{
    private readonly IInventoryDocumentHandler<PurchaseDoc> _purchaseHandler;
    private readonly ICommandRepository<InventoryRegisterBalance> _command;

    public InventoryDispatcher(IInventoryDocumentHandler<PurchaseDoc> purchaseHandler,
                               ICommandRepository<InventoryRegisterBalance> command)
    {
        _purchaseHandler = purchaseHandler;
        _command         = command;
    }

    public async Task<Result<List<InventoryRegisterBalance>>> ProcessAsync(object document, CancellationToken ct = default)
    {
        var result = document switch
        {
            PurchaseDoc p => await _purchaseHandler.HandleAsync(p, ct),
            _             => Result.Failure<List<InventoryRegisterBalance>>(InventoryRegisterBalanceErrors.UnsupportedDocumentType())
        };

        if (!result.IsSuccess)
            return result;

        await _command.CreateAsync(result.Value, ct);
        return result;
    }
}
