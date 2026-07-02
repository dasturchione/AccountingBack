using Application.Abstractions;
using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.InventoryCounts;

public class ActiveInventoryCountGuardService : IActiveInventoryCountGuardService
{
    private readonly IQueryRepository<InventoryCountDoc> _inventoryCountQuery;
    private readonly IUserContext _userContext;

    public ActiveInventoryCountGuardService(IQueryRepository<InventoryCountDoc> inventoryCountQuery, IUserContext userContext)
    {
        _inventoryCountQuery = inventoryCountQuery;
        _userContext = userContext;
    }

    public async Task<Result> EnsureWarehouseIsNotBlockedAsync(int organizationId, int warehouseId, string operationName, long? currentInventoryCountId = null, CancellationToken ct = default)
    {
        var hasActiveCount = await _inventoryCountQuery.AnyAsync(x =>
            x.OrganizationId == organizationId &&
            x.WarehouseId == warehouseId &&
            x.StateId == StateIdConst.ACTIVE &&
            (x.StatusId == DocumentStatusIdConst.DRAFT || x.StatusId == DocumentStatusIdConst.PENDING) &&
            (!currentInventoryCountId.HasValue || x.Id != currentInventoryCountId.Value), ct);

        return hasActiveCount
            ? Result.Failure(CommonErrors.WarehouseBlockedByInventoryCount(warehouseId, operationName, _userContext.LanguageId))
            : Result.Success();
    }
}
