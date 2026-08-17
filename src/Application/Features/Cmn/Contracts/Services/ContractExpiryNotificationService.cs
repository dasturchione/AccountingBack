using Application.Abstractions;
using Application.Features.Notifications;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;

namespace Application.Features.Contracts;

public sealed class ContractExpiryNotificationService : IContractExpiryNotificationService
{
    private const string ContractEntityType = "Contract";
    private readonly IQueryRepository<Contract> _contractQuery;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ContractExpiryNotificationService> _logger;
    private readonly IQueryBuilder _queryBuilder;

    public ContractExpiryNotificationService(
        IQueryRepository<Contract> contractQuery,
        INotificationService notificationService,
        IQueryBuilder queryBuilder,
        ILogger<ContractExpiryNotificationService> logger)
    {
        _contractQuery = contractQuery;
        _notificationService = notificationService;
        _queryBuilder = queryBuilder;
        _logger = logger;
    }

    public async Task<int> NotifyAsync(DateTime today, CancellationToken ct = default)
    {
        var candidates = await _contractQuery.GetAllAsync(_queryBuilder.For<Contract>()
            .Where(x => x.StateId == StateIdConst.ACTIVE
                && x.Organization.StateId == StateIdConst.ACTIVE
                && x.EndDate.HasValue
                && x.EndDate.Value < today.AddDays(31))
            .IgnoreQueryFilters()
            .As(x => new ContractExpiryCandidate
            {
                ContractId = x.Id,
                OrganizationId = x.OrganizationId,
                OrganizationName = x.Organization.FullName,
                OrganizationInn = x.Organization.Inn,
                OrganizationLanguageId = x.Organization.DefaultLanguageId,
                ContractTypeName = x.ContractType.Name,
                ContractNumber = x.ContractNumber,
                EndDate = x.EndDate!.Value
            })
            .Build(), ct);

        var created = 0;
        foreach (var candidate in candidates)
        {
            ct.ThrowIfCancellationRequested();

            var isExpired = candidate.EndDate < today;
            var isExpiringSoon = candidate.EndDate >= today && candidate.EndDate <= today.AddDays(30);
            if (!isExpired && !isExpiringSoon)
                continue;

            var typeCode = isExpired ? "error" : "warning";
            var (title, body) = BuildMessage(candidate, isExpired, today);

            var result = await _notificationService.CreateAsync(new CreateNotificationRequest
            {
                TypeCode = typeCode,
                Title = title,
                Body = body,
                OrganizationId = candidate.OrganizationId,
                UserId = null,
                Link = null,
                EntityType = ContractEntityType,
                EntityId = candidate.ContractId
            }, ct);

            if (result.IsSuccess)
                created++;
            else
                _logger.LogWarning("Contract expiry notification failed for ContractId={ContractId}, OrganizationId={OrganizationId}, ErrorCode={ErrorCode}",
                    candidate.ContractId, candidate.OrganizationId, result.Error.Code);
        }

        return created;
    }

    private static (string Title, string Body) BuildMessage(ContractExpiryCandidate candidate, bool isExpired, DateTime today)
    {
        var languageId = candidate.OrganizationLanguageId ?? LanguageIdConst.EN;
        var contractName = string.IsNullOrWhiteSpace(candidate.ContractTypeName) ? "shartnoma" : candidate.ContractTypeName;
        var contractNumber = string.IsNullOrWhiteSpace(candidate.ContractNumber) ? string.Empty : $" №{candidate.ContractNumber}";
        var daysUntilExpiry = Math.Max(0, (candidate.EndDate.Date - today.Date).Days);
        var expiryPhraseUz = daysUntilExpiry == 0 ? "bugun" : $"{daysUntilExpiry} kundan keyin";
        var expiryPhraseCyr = daysUntilExpiry == 0 ? "бугун" : $"{daysUntilExpiry} кундан кейин";
        var expiryPhraseEn = daysUntilExpiry == 0 ? "today" : $"in {daysUntilExpiry} days";

        return languageId switch
        {
            LanguageIdConst.UZ => isExpired
                ? ("Shartnoma muddati tugagan", $"{candidate.OrganizationName}, INN {candidate.OrganizationInn} bo‘yicha {contractName}{contractNumber} shartnomasi muddati tugagan. Shu shartnoma asosida yangi Purchase qilishdan oldin uni tekshirish tavsiya qilinadi.")
                : ("Shartnoma muddati yaqin", $"{candidate.OrganizationName}, INN {candidate.OrganizationInn} bo‘yicha {contractName}{contractNumber} shartnomasi {expiryPhraseUz} tugaydi. Purchase qilishdan oldin shartnomani yangilash tavsiya qilinadi."),
            LanguageIdConst.UZ_CYRL => isExpired
                ? ("Шартнома муддати тугаган", $"{candidate.OrganizationName}, INN {candidate.OrganizationInn} бўйича {contractName}{contractNumber} шартномаси муддати тугаган. Шу шартнома асосида янги Purchase қилишдан олдин уни текшириш тавсия қилинади.")
                : ("Шартнома муддати яқин", $"{candidate.OrganizationName}, INN {candidate.OrganizationInn} бўйича {contractName}{contractNumber} шартномаси {expiryPhraseCyr} тугайди. Purchase қилишдан олдин шартномани янгилаш тавсия қилинади."),
            _ => isExpired
                ? ("Contract expired", $"{candidate.OrganizationName}, INN {candidate.OrganizationInn}: the {contractName}{contractNumber} contract has expired. Please check it before creating a new Purchase based on this contract.")
                : ("Contract expiring soon", $"{candidate.OrganizationName}, INN {candidate.OrganizationInn}: the {contractName}{contractNumber} contract expires {expiryPhraseEn}. Renewing it before creating a Purchase is recommended.")
        };
    }

    private sealed class ContractExpiryCandidate
    {
        public long ContractId { get; init; }
        public int OrganizationId { get; init; }
        public string OrganizationName { get; init; } = string.Empty;
        public string OrganizationInn { get; init; } = string.Empty;
        public short? OrganizationLanguageId { get; init; }
        public string ContractTypeName { get; init; } = string.Empty;
        public string ContractNumber { get; init; } = string.Empty;
        public DateTime EndDate { get; init; }
    }
}
