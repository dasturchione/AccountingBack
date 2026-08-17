using Application.Features.PurchaseDocs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/purchase-docs")]
[ApiController]
[Authorize]
public class PurchaseDocController : ControllerBase
{
    private readonly IPurchaseDocService _service;

    public PurchaseDocController(IPurchaseDocService service)
    {
        _service = service;
    }

    /// <summary>
    /// Barcha xarid hujjatlarini ro'yxati (sahifalab)
    /// </summary>
    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocView)]
    public async Task<IResult> GetAllAsync([FromQuery] PurchaseDocListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    /// <summary>
    /// Bitta hujjat — barcha qatorlari bilan birga
    /// </summary>
    [HttpGet("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocViewDetail)]
    public async Task<IResult> GetByIdAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    /// <summary>
    /// EDO hujjati asosida xarid draft preview'si. Ushbu endpoint database'ga yozmaydi.
    /// </summary>
    [HttpPost("preview")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocCreate)]
    public async Task<IResult> PreviewAsync(
        [FromBody] PurchaseDocPreviewRequestDto request,
        CancellationToken ct = default)
    {
        var result = await _service.PreviewAsync(request, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    /// <summary>
    /// Tasdiqlangan EDO hujjatidan Draft Purchase yaratadi.
    /// Hujjat oldin import qilingan bo'lsa, mavjud Purchase'ni muvaffaqiyatli qaytaradi.
    /// </summary>
    [HttpPost("from-edo")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocCreate)]
    public async Task<IResult> CreateFromEdoAsync(
        [FromBody] PurchaseDocFromEdoRequestDto request,
        CancellationToken ct = default)
    {
        var result = await _service.CreateFromEdoAsync(request, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    /// <summary>
    /// Yangi hujjat — sarlavha + qatorlar bitta so'rovda yaratiladi
    /// </summary>
    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocCreate)]
    public async Task<IResult> CreateAsync([FromBody] PurchaseDocCreateDto dto, CancellationToken ct = default)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    /// <summary>
    /// Hujjatni yangilash — eski qatorlar o'chirilib, yangilari yoziladi
    /// </summary>
    [HttpPut("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocUpdate)]
    public async Task<IResult> UpdateAsync([FromRoute] long id, [FromBody] PurchaseDocUpdateDto dto, CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    /// <summary>
    /// Hujjatni tasdiqlash va biznes registrlarga o'tkazish
    /// </summary>
    [HttpPut("{id:long}/confirm")]
    [ModuleAuthorize(PermissionCodeConst.ConfirmPurchase)]
    public async Task<IResult> ConfirmAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.ConfirmAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    /// <summary>
    /// Hujjatni bekor qilish va o'tkazmalarni storno qilish
    /// </summary>
    [HttpPut("{id:long}/cancel")]
    [ModuleAuthorize(PermissionCodeConst.CancelPurchase)]
    public async Task<IResult> CancelAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.CancelAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    /// <summary>
    /// Hujjatni o'chirish — qatorlari ham birga o'chiriladi (faqat Draft holati)
    /// </summary>
    [HttpDelete("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocDelete)]
    public async Task<IResult> DeleteAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
