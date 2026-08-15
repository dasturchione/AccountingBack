using Application.Abstractions;
using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;
using System.Globalization;

namespace Application.Features.DocumentNumbers;

public sealed class DocumentNumberService : IDocumentNumberService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IDocumentPostingLock _postingLock;
    private readonly IQueryRepository<Organization> _organizationQuery;
    private readonly IQueryRepository<DocumentType> _documentTypeQuery;
    private readonly IQueryRepository<DocumentNumberSequence> _sequenceQuery;
    private readonly ICommandRepository<DocumentNumberSequence> _sequenceCommand;

    public DocumentNumberService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IDocumentPostingLock postingLock,
        IQueryRepository<Organization> organizationQuery,
        IQueryRepository<DocumentType> documentTypeQuery,
        IQueryRepository<DocumentNumberSequence> sequenceQuery,
        ICommandRepository<DocumentNumberSequence> sequenceCommand)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _postingLock = postingLock;
        _organizationQuery = organizationQuery;
        _documentTypeQuery = documentTypeQuery;
        _sequenceQuery = sequenceQuery;
        _sequenceCommand = sequenceCommand;
    }

    public async Task<Result<DocumentNumberResult>> GetNextAsync(
        int organizationId,
        short documentTypeId,
        DateTime documentDate,
        CancellationToken ct = default) =>
        await GetNextCoreAsync(
            organizationId, documentTypeId, documentDate, historical: false, ct);

    public async Task<Result<DocumentNumberResult>> GetNextHistoricalAsync(
        int organizationId,
        short documentTypeId,
        DateTime documentDate,
        CancellationToken ct = default) =>
        await GetNextCoreAsync(
            organizationId, documentTypeId, documentDate, historical: true, ct);

    private async Task<Result<DocumentNumberResult>> GetNextCoreAsync(
        int organizationId,
        short documentTypeId,
        DateTime documentDate,
        bool historical,
        CancellationToken ct)
    {
        if (organizationId <= 0)
            return Result.Failure<DocumentNumberResult>(DocumentNumberErrors.InvalidOrganization(_userContext.LanguageId));

        if (documentTypeId <= 0)
            return Result.Failure<DocumentNumberResult>(DocumentNumberErrors.DocumentTypeNotFound(documentTypeId, _userContext.LanguageId));

        if (documentDate == default)
            return Result.Failure<DocumentNumberResult>(DocumentNumberErrors.InvalidDocumentDate(_userContext.LanguageId));

        try
        {
            if (!await _organizationQuery.AnyAsync(x => x.Id == organizationId
                    && x.TenantId > 0
                    && x.StateId == StateIdConst.ACTIVE, ct))
                return Result.Failure<DocumentNumberResult>(DocumentNumberErrors.InvalidOrganization(_userContext.LanguageId));

            if (!await _documentTypeQuery.AnyAsync(x => x.Id == documentTypeId, ct))
                return Result.Failure<DocumentNumberResult>(DocumentNumberErrors.DocumentTypeNotFound(documentTypeId, _userContext.LanguageId));

            var normalizedDocumentDate = DateTime.SpecifyKind(documentDate.Date, DateTimeKind.Unspecified);
            var sequenceLockId = (long)organizationId << 32;
            await _postingLock.AcquireAsync(documentTypeId, sequenceLockId, ct);

            var sequencesQuery = _queryBuilder.For<DocumentNumberSequence>()
                .Where(x => x.OrganizationId == organizationId && x.DocumentTypeId == documentTypeId)
                .Build();
            var sequences = await _sequenceQuery.GetAllAsync(sequencesQuery, ct);

            var latestDocumentDate = sequences.Count == 0
                ? (DateTime?)null
                : sequences.Max(x => x.LastDocumentDate);
            if (!historical
                && latestDocumentDate.HasValue
                && normalizedDocumentDate < latestDocumentDate.Value.Date)
            {
                return Result.Failure<DocumentNumberResult>(
                    DocumentNumberErrors.EarlierDocumentDate(latestDocumentDate.Value, _userContext.LanguageId));
            }

            var documentYear = checked((short)normalizedDocumentDate.Year);
            var sequence = sequences.SingleOrDefault(x => x.DocumentYear == documentYear);
            if (sequence is null)
            {
                sequence = new DocumentNumberSequence
                {
                    OrganizationId = organizationId,
                    DocumentTypeId = documentTypeId,
                    DocumentYear = documentYear,
                    LastNumber = 1,
                    LastDocumentDate = normalizedDocumentDate,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                await _sequenceCommand.CreateAsync(sequence, ct);
            }
            else
            {
                if (sequence.LastNumber < 0 || sequence.LastNumber == long.MaxValue)
                {
                    return Result.Failure<DocumentNumberResult>(
                        DocumentNumberErrors.SequenceConfigurationInvalid(_userContext.LanguageId));
                }

                sequence.LastNumber++;
                if (normalizedDocumentDate > sequence.LastDocumentDate)
                    sequence.LastDocumentDate = normalizedDocumentDate;
                sequence.UpdatedAt = DateTime.Now;
                await _sequenceCommand.UpdateAsync(sequence, ct);
            }

            return Result.Success(new DocumentNumberResult(
                sequence.LastNumber,
                historical
                    ? HistoricalDocumentNumberPolicy.Format(
                        sequence.DocumentYear, sequence.LastNumber)
                    : sequence.LastNumber.ToString(CultureInfo.InvariantCulture),
                normalizedDocumentDate));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return Result.Failure<DocumentNumberResult>(DocumentNumberErrors.CannotGenerate(_userContext.LanguageId));
        }
    }
}
