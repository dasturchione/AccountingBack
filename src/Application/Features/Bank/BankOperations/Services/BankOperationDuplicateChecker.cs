using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;

namespace Application.Features.BankOperations;

public sealed class BankOperationDuplicateChecker : IBankOperationDuplicateChecker
{
    private readonly IQueryRepository<BankOperation> _query;
    private readonly IQueryBuilder _queryBuilder;

    public BankOperationDuplicateChecker(
        IQueryRepository<BankOperation> query,
        IQueryBuilder queryBuilder)
    {
        _query = query;
        _queryBuilder = queryBuilder;
    }

    public async Task<IReadOnlySet<BankOperationIdentity>> FindExistingAsync(
        int organizationId,
        IReadOnlyCollection<BankOperationIdentity> identities,
        long? excludedOperationId = null,
        CancellationToken ct = default)
    {
        if (identities.Count == 0)
            return new HashSet<BankOperationIdentity>();

        var requested = identities.ToHashSet();
        var bankAccountIds = requested.Select(x => x.BankAccountId).Distinct().ToList();
        var documentNumbers = requested.Select(x => x.BankDocumentNumber).Distinct().ToList();
        var from = requested.Min(x => x.DocumentDate).ToDateTime(TimeOnly.MinValue);
        var to = requested.Max(x => x.DocumentDate).AddDays(1).ToDateTime(TimeOnly.MinValue);

        var specification = _queryBuilder.For<BankOperation>()
            .Where(x =>
                x.OrganizationId == organizationId &&
                x.StateId == StateIdConst.ACTIVE &&
                (!excludedOperationId.HasValue || x.Id != excludedOperationId.Value) &&
                bankAccountIds.Contains(x.BankAccountId) &&
                x.BankDocumentNumber != null &&
                documentNumbers.Contains(x.BankDocumentNumber.Trim()) &&
                x.DocDate >= from &&
                x.DocDate < to)
            .As(x => new ExistingBankOperation
            {
                BankAccountId = x.BankAccountId,
                BankDocumentNumber = x.BankDocumentNumber!,
                DocDate = x.DocDate
            })
            .Build();

        var matches = await _query.GetAllAsync(specification, ct);
        return matches
            .Select(x => new BankOperationIdentity(
                x.BankAccountId,
                x.BankDocumentNumber.Trim(),
                DateOnly.FromDateTime(x.DocDate)))
            .Where(requested.Contains)
            .ToHashSet();
    }

    private sealed class ExistingBankOperation
    {
        public int BankAccountId { get; set; }
        public string BankDocumentNumber { get; set; } = string.Empty;
        public DateTime DocDate { get; set; }
    }
}
