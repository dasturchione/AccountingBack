using Application.Common.Pagination;
using Application.Features.CashDocuments;
using Application.Features.CashOperations;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace UnitTests;

public class CashDocumentServiceTests
{
    [Fact]
    public async Task CreateReceiptOrderAsync_ShouldForceInOperationType()
    {
        var cashOperationService = new FakeCashOperationService();
        var service = new CashDocumentService(cashOperationService);

        var result = await service.CreateReceiptOrderAsync(new CashDocumentCreateDto
        {
            CashBoxId = 4,
            CashChartAccountId = 5010,
            OffsetAccountId = 6010,
            DocDate = new DateTime(2026, 7, 3),
            CurrencyId = 1,
            Amount = 250m
        });

        Assert.True(result.IsSuccess);
        Assert.NotNull(cashOperationService.LastCreateDto);
        Assert.Equal(OperationTypeIdConst.IN, cashOperationService.LastCreateDto!.OperationTypeId);
        Assert.Null(cashOperationService.LastCreateDto.DestinationCashBoxId);
        Assert.Equal(5010, cashOperationService.LastCreateDto.CashChartAccountId);
        Assert.Equal(6010, cashOperationService.LastCreateDto.OffsetAccountId);
    }

    [Fact]
    public async Task CreatePaymentOrderAsync_ShouldForceOutOperationType()
    {
        var cashOperationService = new FakeCashOperationService();
        var service = new CashDocumentService(cashOperationService);

        var result = await service.CreatePaymentOrderAsync(new CashDocumentCreateDto
        {
            CashBoxId = 4,
            CashChartAccountId = 5010,
            OffsetAccountId = 6010,
            DocDate = new DateTime(2026, 7, 3),
            CurrencyId = 1,
            Amount = 250m
        });

        Assert.True(result.IsSuccess);
        Assert.NotNull(cashOperationService.LastCreateDto);
        Assert.Equal(OperationTypeIdConst.OUT, cashOperationService.LastCreateDto!.OperationTypeId);
        Assert.Null(cashOperationService.LastCreateDto.DestinationCashBoxId);
        Assert.Equal(5010, cashOperationService.LastCreateDto.CashChartAccountId);
        Assert.Equal(6010, cashOperationService.LastCreateDto.OffsetAccountId);
    }
}

file sealed class FakeCashOperationService : ICashOperationService
{
    public CashOperationCreateDto? LastCreateDto { get; private set; }

    public Task<Result<PagedResponse<CashOperationListDto>>> GetAllAsync(CashOperationListFilter filter, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new PagedResponse<CashOperationListDto>()));

    public Task<Result<CashOperationDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success(new CashOperationDto { Id = id, OperationTypeId = OperationTypeIdConst.IN }));

    public Task<Result<long>> CreateAsync(CashOperationCreateDto dto, CancellationToken ct = default)
    {
        LastCreateDto = dto;
        return Task.FromResult(Result.Success(100L));
    }

    public Task<Result> UpdateAsync(long id, CashOperationUpdateDto dto, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result> ConfirmAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result> CancelAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());

    public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
        Task.FromResult(Result.Success());
}
