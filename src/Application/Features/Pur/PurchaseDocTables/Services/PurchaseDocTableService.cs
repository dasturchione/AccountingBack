using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.PurchaseDocTables;

public class PurchaseDocTableService : IPurchaseDocTableService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<PurchaseDocTable> _query;
    private readonly ICommandRepository<PurchaseDocTable> _command;
    private readonly IQueryRepository<PurchaseDoc> _docQuery;
    private readonly ICommandRepository<PurchaseDoc> _docCommand;
    private readonly IQueryRepository<VatRate> _vatRateQuery;

    public PurchaseDocTableService(IUserContext userContext,
                                   IQueryBuilder queryBuilder,
                                   IQueryRepository<PurchaseDocTable> query,
                                   ICommandRepository<PurchaseDocTable> command,
                                   IQueryRepository<PurchaseDoc> docQuery,
                                   ICommandRepository<PurchaseDoc> docCommand,
                                   IQueryRepository<VatRate> vatRateQuery)
    {
        _query        = query;
        _command      = command;
        _userContext  = userContext;
        _queryBuilder = queryBuilder;
        _docQuery     = docQuery;
        _docCommand   = docCommand;
        _vatRateQuery = vatRateQuery;
    }

    public async Task<Result<PagedResponse<PurchaseDocTableListDto>>> GetAllAsync(PurchaseDocTableListFilter filter, CancellationToken ct = default)
    {
        var query     = _queryBuilder.BuildPaged<PurchaseDocTable, PurchaseDocTableListDto, PurchaseDocTableListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<PurchaseDocTableDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var query  = _queryBuilder.For<PurchaseDocTable>().Where(e => e.Id == id).As<PurchaseDocTableDto>().Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null)
            return Result.Failure<PurchaseDocTableDto>(PurchaseDocTableErrors.NotFound(id, _userContext.LanguageId));

        return entity;
    }

    public async Task<Result<long>> CreateAsync(PurchaseDocTableCreateDto dto, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        return Result.Failure<long>(PurchaseDocTableErrors.DirectTableCreateUnsupported(_userContext.LanguageId));
    }

    public async Task<Result> UpdateAsync(long id, PurchaseDocTableUpdateDto dto, CancellationToken ct = default)
    {
        var lineQuery = _queryBuilder.For<PurchaseDocTable>().Where(e => e.Id == id).Build();
        var entity    = await _query.GetAsync(lineQuery, ct);

        if (entity == null)
            return Result.Failure(PurchaseDocTableErrors.NotFound(id, _userContext.LanguageId));

        var docQuery = _queryBuilder.For<PurchaseDoc>().Where(x => x.Id == entity.Owner.OwnerId).Build();
        var doc      = await _docQuery.GetAsync(docQuery, ct);

        if (doc == null)
            return Result.Failure(PurchaseDocTableErrors.OwnerNotFound(entity.Owner.OwnerId, _userContext.LanguageId));

        if (doc.StatusId == DocumentStatusIdConst.POSTED)
            return Result.Failure(PurchaseDocTableErrors.OwnerAlreadyPosted(entity.Owner.OwnerId, _userContext.LanguageId));

        var (newAmount, newVatAmount, newTotalAmount, error) = await CalculateAmountsAsync(1, dto.Price, dto.VatRateId, ct);
        if (error != null)
            return Result.Failure(error);

        // Hujjat totallaridan eski qator summalarini ayirib, yangilarini qo'shamiz
        doc.TotalAmount += newAmount      - entity.Amount;
        doc.VatAmount   += newVatAmount   - entity.VatAmount;
        doc.FinalAmount += newTotalAmount - entity.TotalAmount;

        entity.ProductTableId   = dto.ProductTableId ?? entity.ProductTableId;
        entity.Amount           = newAmount;
        entity.VatRateId        = dto.VatRateId;
        entity.VatAmount        = newVatAmount;
        entity.TotalAmount      = newTotalAmount;

        await _command.UpdateAsync(entity, ct);
        await _docCommand.UpdateAsync(doc, ct);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(long id, CancellationToken ct = default)
    {
        var lineQuery = _queryBuilder.For<PurchaseDocTable>().Where(e => e.Id == id).Build();
        var entity    = await _query.GetAsync(lineQuery, ct);

        if (entity == null)
            return Result.Failure(PurchaseDocTableErrors.NotFound(id, _userContext.LanguageId));

        var docQuery = _queryBuilder.For<PurchaseDoc>().Where(x => x.Id == entity.Owner.OwnerId).Build();
        var doc      = await _docQuery.GetAsync(docQuery, ct);

        if (doc == null)
            return Result.Failure(PurchaseDocTableErrors.OwnerNotFound(entity.Owner.OwnerId, _userContext.LanguageId));

        if (doc.StatusId == DocumentStatusIdConst.POSTED)
            return Result.Failure(PurchaseDocTableErrors.OwnerAlreadyPosted(entity.Owner.OwnerId, _userContext.LanguageId));

        doc.TotalAmount -= entity.Amount;
        doc.VatAmount   -= entity.VatAmount;
        doc.FinalAmount -= entity.TotalAmount;

        await _command.DeleteAsync(entity, ct);
        await _docCommand.UpdateAsync(doc, ct);

        return Result.Success();
    }

    // VatRateId bo'lsa DB dan rate olib hisoblaydi, bo'lmasa 0
    private async Task<(decimal amount, decimal vatAmount, decimal totalAmount, Error? error)> CalculateAmountsAsync(
        decimal quantity, decimal price, short? vatRateId, CancellationToken ct)
    {
        var amount    = quantity * price;
        var vatAmount = 0m;

        if (vatRateId.HasValue)
        {
            var vatRateQuery = _queryBuilder.For<VatRate>().Where(v => v.Id == vatRateId.Value).Build();
            var vatRate      = await _vatRateQuery.GetAsync(vatRateQuery, ct);

            if (vatRate == null)
                return (0, 0, 0, PurchaseDocTableErrors.VatRateNotFound(vatRateId.Value, _userContext.LanguageId));

            vatAmount = Math.Round(amount * vatRate.Rate / 100, 2);
        }

        return (amount, vatAmount, amount + vatAmount, null);
    }
}
