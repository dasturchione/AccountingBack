using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.Cmn.AslBelgi.Abstractions;
using Application.Features.Cmn.AslBelgi.DTOs;
using Application.Features.Cmn.AslBelgi.Errors;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Cmn.AslBelgi.Services;

public sealed class AslBelgiMarkingService : BaseService, IAslBelgiMarkingService
{
    private readonly IUserContext _userContext;
    private readonly IAslBelgiMarkingRepository _markingRepository;
    private readonly ICommandRepository<ProductTable> _productTableCommand;
    private readonly IAslBelgiService _aslBelgiService;

    public AslBelgiMarkingService(
        IUserContext userContext,
        IAslBelgiMarkingRepository markingRepository,
        ICommandRepository<ProductTable> productTableCommand,
        IAslBelgiService aslBelgiService,
        ILogger<AslBelgiMarkingService> logger,
        IUnitOfWork unitOfWork)
        : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _markingRepository = markingRepository;
        _productTableCommand = productTableCommand;
        _aslBelgiService = aslBelgiService;
    }

    public Task<Result<AslBelgiOrderResponse>> RequestMarkingAsync(AslBelgiMarkingRequestDto request, CancellationToken ct = default)
        => ExecuteAsync("RequestMarking", async () =>
        {
            if (_userContext.OrganizationId is not { } organizationId)
                return Result.Failure<AslBelgiOrderResponse>(AslBelgiErrors.UserHasNoOrganization());

            var product = await _markingRepository.GetProductForMarkingAsync(request.ProductId, organizationId, ct);
            if (product is null)
                return Result.Failure<AslBelgiOrderResponse>(AslBelgiErrors.ProductNotFound(request.ProductId));

            var gtin = string.IsNullOrWhiteSpace(request.Gtin) ? product.Gtin : request.Gtin;
            if (string.IsNullOrWhiteSpace(gtin))
                return Result.Failure<AslBelgiOrderResponse>(AslBelgiErrors.MissingGtinForProduct(request.ProductId));

            var order = new AslBelgiOrderRequest
            {
                ProductGroup = request.ProductGroup,
                BusinessPlaceId = request.BusinessPlaceId,
                ReleaseMethodType = request.ReleaseMethodType,
                PoNumber = request.PoNumber,
                Products =
                [
                    new AslBelgiOrderProduct
                    {
                        Gtin = gtin!,
                        Quantity = request.Quantity,
                        CisType = request.CisType,
                        SerialNumberType = request.SerialNumberType
                    }
                ]
            };

            return await _aslBelgiService.RegisterOrderAsync(order, ct);
        });

    public Task<Result<AslBelgiBindResultDto>> FetchAndBindCodesAsync(AslBelgiBindCodesRequestDto request, CancellationToken ct = default)
        => ExecuteInTransactionAsync("FetchAndBindCodes", async () =>
        {
            if (_userContext.OrganizationId is not { } organizationId)
                return Result.Failure<AslBelgiBindResultDto>(AslBelgiErrors.UserHasNoOrganization());

            if (string.IsNullOrWhiteSpace(request.OrderId))
                return Result.Failure<AslBelgiBindResultDto>(AslBelgiErrors.MissingOrderId());

            var product = await _markingRepository.GetProductForMarkingAsync(request.ProductId, organizationId, ct);
            if (product is null)
                return Result.Failure<AslBelgiBindResultDto>(AslBelgiErrors.ProductNotFound(request.ProductId));

            var gtin = string.IsNullOrWhiteSpace(request.Gtin) ? product.Gtin : request.Gtin;

            var codesResult = await _aslBelgiService.GetCodesAsync(request.OrderId, gtin, request.Quantity, request.LastPackId, ct);
            if (!codesResult.IsSuccess)
                return Result.Failure<AslBelgiBindResultDto>(codesResult.Error);

            var codes = (codesResult.Value.Codes ?? [])
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .ToList();

            var packId = codesResult.Value.PackId;

            if (codes.Count == 0)
                return Result.Success(BuildResult(request.OrderId, packId, total: 0, bound: 0));

            // Idempotency: skip codes already bound (also guarded by the ux_inv_product_table_org_marking unique index).
            var existing = (await _markingRepository.GetExistingMarkingsAsync(organizationId, codes, ct)).ToHashSet();
            var newCodes = codes.Where(c => !existing.Contains(c)).ToList();

            if (newCodes.Count > 0)
            {
                var rows = newCodes.Select(code => new ProductTable
                {
                    ProductId = request.ProductId,
                    OrganizationId = organizationId,
                    MarkingNumber = code,
                    StatusId = ProductTableStatusIdConst.IN_STOCK,
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.Now
                }).ToList();

                await _productTableCommand.CreateAsync(rows, ct);
            }

            return Result.Success(BuildResult(request.OrderId, packId, total: codes.Count, bound: newCodes.Count));
        }, ct);

    private static AslBelgiBindResultDto BuildResult(string orderId, string? packId, int total, int bound) => new()
    {
        OrderId = orderId,
        PackId = packId,
        TotalCodes = total,
        BoundCount = bound,
        SkippedExisting = total - bound
    };
}
