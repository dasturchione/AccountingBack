using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.PaymentAcceptancePoints;

public class PaymentAcceptancePointService : IPaymentAcceptancePointService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<PaymentAcceptancePoint> _query;
    private readonly IQueryRepository<PaymentAcceptancePointType> _typeQuery;
    private readonly IQueryRepository<BankAccount> _bankAccountQuery;
    private readonly ICommandRepository<PaymentAcceptancePoint> _command;

    public PaymentAcceptancePointService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<PaymentAcceptancePoint> query,
        IQueryRepository<PaymentAcceptancePointType> typeQuery,
        IQueryRepository<BankAccount> bankAccountQuery,
        ICommandRepository<PaymentAcceptancePoint> command)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _query = query;
        _typeQuery = typeQuery;
        _bankAccountQuery = bankAccountQuery;
        _command = command;
    }

    public async Task<Result<int>> CreateAsync(PaymentAcceptancePointCreateDto dto, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is not int organizationId)
            return Result.Failure<int>(PaymentAcceptancePointErrors.OrganizationRequired(_userContext.LanguageId));

        var validationResult = await ValidateReferencesAndUniquenessAsync(dto, organizationId, null, ct);
        if (!validationResult.IsSuccess)
            return Result.Failure<int>(validationResult.Error);

        var entity = new PaymentAcceptancePoint
        {
            OrganizationId = organizationId,
            TypeId = dto.TypeId,
            BankAccountId = dto.BankAccountId,
            Code = $"PAP-{Guid.NewGuid():N}".ToUpperInvariant(),
            Name = dto.Name,
            MerchantId = NormalizeOptional(dto.MerchantId),
            ExternalId = NormalizeOptional(dto.ExternalId),
            SerialNumber = NormalizeOptional(dto.SerialNumber),
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };

        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await GetEntityAsync(id, ct);
        if (entity is null)
            return Result.Failure(PaymentAcceptancePointErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;
        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<PaymentAcceptancePointListDto>>> GetAllAsync(PaymentAcceptancePointListFilter filter, CancellationToken ct = default)
    {
        var page = Math.Max(filter.Page, 1);
        var pageSize = filter.PageSize.GetValueOrDefault(50);
        var search = string.IsNullOrWhiteSpace(filter.Search) ? null : filter.Search.Trim().ToLower();

        var query = _queryBuilder.For<PaymentAcceptancePoint>()
            .Where(x =>
                (!filter.TypeId.HasValue || x.TypeId == filter.TypeId.Value) &&
                (!filter.BankAccountId.HasValue || x.BankAccountId == filter.BankAccountId.Value) &&
                (!filter.StateId.HasValue || x.StateId == filter.StateId.Value))
            .As(ToListDto())
            .Where(x => search == null ||
                x.Code.ToLower().Contains(search) ||
                x.Name.ToLower().Contains(search) ||
                x.TypeName.ToLower().Contains(search) ||
                (x.MerchantId != null && x.MerchantId.ToLower().Contains(search)) ||
                (x.ExternalId != null && x.ExternalId.ToLower().Contains(search)) ||
                (x.SerialNumber != null && x.SerialNumber.ToLower().Contains(search)) ||
                (x.BankAccountNumber != null && x.BankAccountNumber.ToLower().Contains(search)))
            .OrderBy(x => x.OrderBy(t => t.Name).ThenBy(t => t.Id))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .BuildPaged();

        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, page, pageSize);
    }

    public async Task<Result<PaymentAcceptancePointDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<PaymentAcceptancePoint>()
            .Where(x => x.Id == id)
            .As(ToDto())
            .Build();

        var point = await _query.GetAsync(query, ct);
        return point is null
            ? Result.Failure<PaymentAcceptancePointDto>(PaymentAcceptancePointErrors.NotFound(id, _userContext.LanguageId))
            : point;
    }

    public async Task<Result> UpdateAsync(int id, PaymentAcceptancePointUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await GetEntityAsync(id, ct);
        if (entity is null)
            return Result.Failure(PaymentAcceptancePointErrors.NotFound(id, _userContext.LanguageId));

        var validationResult = await ValidateReferencesAndUniquenessAsync(dto, entity.OrganizationId, id, ct);
        if (!validationResult.IsSuccess)
            return validationResult;

        entity.TypeId = dto.TypeId;
        entity.BankAccountId = dto.BankAccountId;
        entity.Name = dto.Name;
        entity.MerchantId = NormalizeOptional(dto.MerchantId);
        entity.ExternalId = NormalizeOptional(dto.ExternalId);
        entity.SerialNumber = NormalizeOptional(dto.SerialNumber);
        entity.StateId = dto.StateId;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    private async Task<PaymentAcceptancePoint?> GetEntityAsync(int id, CancellationToken ct)
    {
        var query = _queryBuilder.For<PaymentAcceptancePoint>()
            .Where(x => x.Id == id)
            .Build();

        return await _query.GetAsync(query, ct);
    }

    private async Task<Result> ValidateReferencesAndUniquenessAsync(
        PaymentAcceptancePointBaseDto dto,
        int organizationId,
        int? excludedId,
        CancellationToken ct)
    {
        if (!await _typeQuery.AnyAsync(
                x => x.Id == dto.TypeId && x.StateId == StateIdConst.ACTIVE,
                ct))
        {
            return Result.Failure(PaymentAcceptancePointErrors.TypeNotFound(dto.TypeId, _userContext.LanguageId));
        }

        if (dto.BankAccountId.HasValue && !await _bankAccountQuery.AnyAsync(
                x => x.Id == dto.BankAccountId.Value &&
                     x.OrganizationId == organizationId &&
                     x.StateId == StateIdConst.ACTIVE,
                ct))
        {
            return Result.Failure(PaymentAcceptancePointErrors.BankAccountNotFound(dto.BankAccountId.Value, _userContext.LanguageId));
        }

        var externalId = NormalizeOptional(dto.ExternalId);
        if (externalId is not null && await _query.AnyAsync(
                x => x.OrganizationId == organizationId &&
                     x.ExternalId == externalId &&
                     (!excludedId.HasValue || x.Id != excludedId.Value),
                ct))
        {
            return Result.Failure(PaymentAcceptancePointErrors.ExternalIdConflict(externalId, _userContext.LanguageId));
        }

        var serialNumber = NormalizeOptional(dto.SerialNumber);
        if (serialNumber is not null && await _query.AnyAsync(
                x => x.OrganizationId == organizationId &&
                     x.SerialNumber == serialNumber &&
                     (!excludedId.HasValue || x.Id != excludedId.Value),
                ct))
        {
            return Result.Failure(PaymentAcceptancePointErrors.SerialNumberConflict(serialNumber, _userContext.LanguageId));
        }

        return Result.Success();
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static System.Linq.Expressions.Expression<Func<PaymentAcceptancePoint, PaymentAcceptancePointDto>> ToDto() =>
        x => new PaymentAcceptancePointDto
        {
            Id = x.Id,
            Code = x.Code,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            TypeId = x.TypeId,
            TypeCode = x.Type.Code,
            TypeName = x.Type.Name,
            BankAccountId = x.BankAccountId,
            BankAccountNumber = x.BankAccount != null ? x.BankAccount.AccountNumber : null,
            Name = x.Name,
            MerchantId = x.MerchantId,
            ExternalId = x.ExternalId,
            SerialNumber = x.SerialNumber,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };

    private static System.Linq.Expressions.Expression<Func<PaymentAcceptancePoint, PaymentAcceptancePointListDto>> ToListDto() =>
        x => new PaymentAcceptancePointListDto
        {
            Id = x.Id,
            Code = x.Code,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            TypeId = x.TypeId,
            TypeCode = x.Type.Code,
            TypeName = x.Type.Name,
            BankAccountId = x.BankAccountId,
            BankAccountNumber = x.BankAccount != null ? x.BankAccount.AccountNumber : null,
            Name = x.Name,
            MerchantId = x.MerchantId,
            ExternalId = x.ExternalId,
            SerialNumber = x.SerialNumber,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
