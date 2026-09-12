using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Pay.HrOrders;

public interface IPayrollHrOrderService
{
    Task<Result<PagedResponse<PayrollHrOrderListDto>>> GetAllAsync(PayrollHrOrderListFilter filter, CancellationToken ct = default);
    Task<Result<PayrollHrOrderDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(PayrollHrOrderSaveDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, PayrollHrOrderSaveDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);

    /// <summary>Buyruqni tasdiqlash: DRAFT→POSTED, tegishli employment intervalini hosil qiladi/yopadi.</summary>
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);

    /// <summary>Tasdiqni bekor qilish: POSTED→CANCELLED, employment o'zgarishini qaytaradi.</summary>
    Task<Result> CancelAsync(long id, CancellationToken ct = default);

    /// <summary>Bosmaga tayyor buyruq ma'lumoti (eski→yangi).</summary>
    Task<Result<PayrollHrOrderPrintDto>> GetPrintAsync(long id, CancellationToken ct = default);
}
