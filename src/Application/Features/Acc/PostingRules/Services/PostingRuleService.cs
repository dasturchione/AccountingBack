using Application.Abstractions;
using Application.Abstractions.Authentication;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Acc.PostingRules;

public class PostingRuleService : BaseService, IPostingRuleService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<PostingRule> _postingRuleQuery;
    private readonly ICommandRepository<PostingRule> _postingRuleCommand;

    public PostingRuleService(IUserContext userContext,
                              IQueryBuilder queryBuilder,
                              IQueryRepository<PostingRule> postingRuleQuery,
                              ICommandRepository<PostingRule> postingRuleCommand,
                              ILogger<PostingRuleService> logger,
                              IUnitOfWork unitOfWork) : base(logger, unitOfWork)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _postingRuleQuery = postingRuleQuery;
        _postingRuleCommand = postingRuleCommand;
    }

    public Task<Result<List<PostingRuleListDto>>> GetAllAsync(PostingRuleListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            var query = _queryBuilder.BuildPaged<PostingRule, PostingRuleListDto, PostingRuleListFilter>(filter);
            var items = await _postingRuleQuery.GetAllAsync(query, ct);

            items = items
                    .GroupBy(x => x.DocumentTypeId)
                    .Select(g => g
                        .OrderByDescending(x => x.OrganizationId == _userContext.OrganizationId)
                        .First())
                    .ToList();

            return Result.Success(items);
        });

    public Task<Result<PostingRuleDto>> GetByIdAsync(int id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            var query = _queryBuilder.For<PostingRule>().Where(x => x.Id == id).As<PostingRuleDto>().Build();
            var entity = await _postingRuleQuery.GetAsync(query, ct);
            if (entity is null)
                return Result.Failure<PostingRuleDto>(PostingRuleErrors.NotFound(id, _userContext.LanguageId));
            return entity;
        });

    public Task<Result<int>> CreateAsync(PostingRuleCreateDto dto, CancellationToken ct = default) =>
        ExecuteAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId == null)
                return Result.Failure<int>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var exists = await _postingRuleQuery
                                        .AnyAsync(x => x.OrganizationId == _userContext.OrganizationId.Value &&
                                                       x.DocumentTypeId == dto.DocumentTypeId,
                                                       ct);
            if (exists)
                return Result.Failure<int>(PostingRuleErrors.Conflict(dto.Code, _userContext.LanguageId));

            var lines = dto.Lines.Select(l => new PostingRuleLine
            {
                SortOrder = l.SortOrder,
                DebitAccountId = l.DebitAccountId,
                CreditAccountId = l.CreditAccountId,
                AmountSource = l.AmountSource,
                QuantitySource = l.QuantitySource,
                ContentTemplate = l.ContentTemplate,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.UtcNow
            });

            var entity = new PostingRule
            {
                OrganizationId = _userContext.OrganizationId,
                DocumentTypeId = dto.DocumentTypeId,
                OperationTypeId = dto.OperationTypeId,
                Code = dto.Code,
                Name = dto.Name,
                PostingRuleLines = lines.ToList(),
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.Now
            };

            await _postingRuleCommand.CreateAsync(entity, ct);

            return entity.Id;
        });

    public Task<Result> UpdateAsync(int id, PostingRuleUpdateDto dto, CancellationToken ct = default) =>
        ExecuteAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId == null)
                return Result.Failure<int>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<PostingRule>().Where(x => x.Id == id).Build();
            var entity = await _postingRuleQuery.GetAsync(query, ct);
            if (entity is null)
                return Result.Failure(PostingRuleErrors.NotFound(id, _userContext.LanguageId));

            if (entity.OrganizationId == null)
                return Result.Failure(PostingRuleErrors.CannotModifyGlobalRule(_userContext.LanguageId));

            entity.OperationTypeId = dto.OperationTypeId;
            entity.Code = dto.Code;
            entity.Name = dto.Name;
            entity.StateId = dto.StateId;

            var existingLines = entity.PostingRuleLines.ToList();

            foreach (var existingLine in existingLines)
            {
                var dtoLine = dto.Lines.FirstOrDefault(x => x.Id == existingLine.Id);

                if (dtoLine == null)
                {
                    existingLine.StateId = StateIdConst.PASSIVE;
                    continue;
                }

                existingLine.SortOrder = dtoLine.SortOrder;
                existingLine.DebitAccountId = dtoLine.DebitAccountId;
                existingLine.CreditAccountId = dtoLine.CreditAccountId;
                existingLine.AmountSource = dtoLine.AmountSource;
                existingLine.QuantitySource = dtoLine.QuantitySource;
                existingLine.ContentTemplate = dtoLine.ContentTemplate;
            }

            var newLines = dto.Lines.Where(x => x.Id == null || x.Id == 0);

            foreach (var line in newLines)
            {
                entity.PostingRuleLines.Add(new PostingRuleLine
                {
                    SortOrder = line.SortOrder,
                    DebitAccountId = line.DebitAccountId,
                    CreditAccountId = line.CreditAccountId,
                    AmountSource = line.AmountSource,
                    QuantitySource = line.QuantitySource,
                    ContentTemplate = line.ContentTemplate,
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.Now
                });
            }

            await _postingRuleCommand.UpdateAsync(entity, ct);

            return Result.Success();
        });

    public Task<Result> DeleteAsync(int id, CancellationToken ct = default) => 
        ExecuteAsync(nameof(DeleteAsync), async () =>
        {
            var query = _queryBuilder.For<PostingRule>().Where(x => x.Id == id).Build();
            var entity = await _postingRuleQuery.GetAsync(query, ct);
            if (entity is null)
                return Result.Failure(PostingRuleErrors.NotFound(id, _userContext.LanguageId));

            entity.StateId = StateIdConst.PASSIVE;
            await _postingRuleCommand.UpdateAsync(entity, ct);
            return Result.Success();
        });
}
