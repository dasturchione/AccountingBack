using Application.Common.Pagination;
using Application.Features.CashOperations;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.CashDocuments;

public class CashDocumentService : ICashDocumentService
{
    private readonly ICashOperationService _cashOperationService;

    public CashDocumentService(ICashOperationService cashOperationService)
    {
        _cashOperationService = cashOperationService;
    }

    public Task<Result<PagedResponse<CashOperationListDto>>> GetReceiptOrdersAsync(CashDocumentListFilter filter, CancellationToken ct = default) =>
        GetAsync(filter, OperationTypeIdConst.IN, ct);

    public Task<Result<PagedResponse<CashOperationListDto>>> GetPaymentOrdersAsync(CashDocumentListFilter filter, CancellationToken ct = default) =>
        GetAsync(filter, OperationTypeIdConst.OUT, ct);

    public Task<Result<CashOperationDto>> GetReceiptOrderByIdAsync(long id, CancellationToken ct = default) =>
        GetByIdAsync(id, OperationTypeIdConst.IN, ct);

    public Task<Result<CashOperationDto>> GetPaymentOrderByIdAsync(long id, CancellationToken ct = default) =>
        GetByIdAsync(id, OperationTypeIdConst.OUT, ct);

    public Task<Result<long>> CreateReceiptOrderAsync(CashDocumentCreateDto dto, CancellationToken ct = default) =>
        _cashOperationService.CreateAsync(MapCreateDto(dto, OperationTypeIdConst.IN), ct);

    public Task<Result<long>> CreatePaymentOrderAsync(CashDocumentCreateDto dto, CancellationToken ct = default) =>
        _cashOperationService.CreateAsync(MapCreateDto(dto, OperationTypeIdConst.OUT), ct);

    public Task<Result> UpdateReceiptOrderAsync(long id, CashDocumentUpdateDto dto, CancellationToken ct = default) =>
        UpdateAsync(id, dto, OperationTypeIdConst.IN, ct);

    public Task<Result> UpdatePaymentOrderAsync(long id, CashDocumentUpdateDto dto, CancellationToken ct = default) =>
        UpdateAsync(id, dto, OperationTypeIdConst.OUT, ct);

    public Task<Result> ConfirmReceiptOrderAsync(long id, CancellationToken ct = default) =>
        ExecuteForDocumentTypeAsync(id, OperationTypeIdConst.IN, _cashOperationService.ConfirmAsync, ct);

    public Task<Result> ConfirmPaymentOrderAsync(long id, CancellationToken ct = default) =>
        ExecuteForDocumentTypeAsync(id, OperationTypeIdConst.OUT, _cashOperationService.ConfirmAsync, ct);

    public Task<Result> CancelReceiptOrderAsync(long id, CancellationToken ct = default) =>
        ExecuteForDocumentTypeAsync(id, OperationTypeIdConst.IN, _cashOperationService.CancelAsync, ct);

    public Task<Result> CancelPaymentOrderAsync(long id, CancellationToken ct = default) =>
        ExecuteForDocumentTypeAsync(id, OperationTypeIdConst.OUT, _cashOperationService.CancelAsync, ct);

    public Task<Result> DeleteReceiptOrderAsync(long id, CancellationToken ct = default) =>
        ExecuteForDocumentTypeAsync(id, OperationTypeIdConst.IN, _cashOperationService.DeleteAsync, ct);

    public Task<Result> DeletePaymentOrderAsync(long id, CancellationToken ct = default) =>
        ExecuteForDocumentTypeAsync(id, OperationTypeIdConst.OUT, _cashOperationService.DeleteAsync, ct);

    private async Task<Result<PagedResponse<CashOperationListDto>>> GetAsync(
        CashDocumentListFilter filter,
        short operationTypeId,
        CancellationToken ct)
    {
        var result = await _cashOperationService.GetAllAsync(
            new CashOperationListFilter
            {
                CashBoxId = filter.CashBoxId,
                OperationTypeId = operationTypeId,
                DateFrom = filter.DateFrom,
                DateTo = filter.DateTo,
                Search = filter.Search,
                Page = filter.Page,
                PageSize = filter.PageSize
            },
            ct);

        return result;
    }

    private async Task<Result<CashOperationDto>> GetByIdAsync(long id, short operationTypeId, CancellationToken ct)
    {
        var result = await _cashOperationService.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return Result.Failure<CashOperationDto>(result.Error);

        return result.Value.OperationTypeId == operationTypeId
            ? result
            : Result.Failure<CashOperationDto>(CashOperationErrors.NotFound(id));
    }

    private async Task<Result> UpdateAsync(long id, CashDocumentUpdateDto dto, short operationTypeId, CancellationToken ct)
    {
        var existing = await GetByIdAsync(id, operationTypeId, ct);
        if (!existing.IsSuccess)
            return Result.Failure(existing.Error);

        return await _cashOperationService.UpdateAsync(id, MapUpdateDto(dto, operationTypeId), ct);
    }

    private async Task<Result> ExecuteForDocumentTypeAsync(
        long id,
        short operationTypeId,
        Func<long, CancellationToken, Task<Result>> operation,
        CancellationToken ct)
    {
        var existing = await GetByIdAsync(id, operationTypeId, ct);
        if (!existing.IsSuccess)
            return Result.Failure(existing.Error);

        return await operation(id, ct);
    }

    private static CashOperationCreateDto MapCreateDto(CashDocumentCreateDto dto, short operationTypeId) =>
        new()
        {
            CashBoxId = dto.CashBoxId,
            DestinationCashBoxId = null,
            OperationTypeId = operationTypeId,
            PaymentTypeId = dto.PaymentTypeId,            CashChartAccountId = dto.CashChartAccountId,
            OffsetAccountId = dto.OffsetAccountId,
            CounterpartyId = dto.CounterpartyId,
            DocDate = dto.DocDate,
            CurrencyId = dto.CurrencyId,
            Amount = dto.Amount,
            ExchangeRate = dto.ExchangeRate,
            Comment = dto.Comment
        };

    private static CashOperationUpdateDto MapUpdateDto(CashDocumentUpdateDto dto, short operationTypeId) =>
        new()
        {
            CashBoxId = dto.CashBoxId,
            DestinationCashBoxId = null,
            OperationTypeId = operationTypeId,
            PaymentTypeId = dto.PaymentTypeId,            CashChartAccountId = dto.CashChartAccountId,
            OffsetAccountId = dto.OffsetAccountId,
            CounterpartyId = dto.CounterpartyId,
            DocDate = dto.DocDate,
            CurrencyId = dto.CurrencyId,
            Amount = dto.Amount,
            ExchangeRate = dto.ExchangeRate,
            Comment = dto.Comment
        };
}
