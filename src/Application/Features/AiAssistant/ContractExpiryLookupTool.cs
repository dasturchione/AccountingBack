using Application.Abstractions;
using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query.Specifications;
using SharedKernel.Time;
using System.Globalization;

namespace Application.Features.AiAssistant;

public sealed class ContractExpiryLookupTool(
    IUserContext userContext,
    IQueryRepository<Contract> contractQuery) :
    IContractExpiryLookupTool,
    IAiReadOnlyToolExecutor
{
    private const int ExpiryWindowDays = 30;

    public AiToolDescriptor Descriptor { get; } = new(
        AccountingToolNames.ContractExpiryLookup,
        IsReadOnly: true,
        AiToolAuthorizationRequirement.AuthenticatedOrganizationMember);

    public async Task<AiToolExecutionResult<ContractExpiryLookupResult>> ExecuteAsync(
        AiToolExecutionRequest<ContractExpiryLookupInput> request,
        CancellationToken ct = default)
    {
        var execution = await ExecuteCoreAsync(request.Input, request.OrganizationContext, request.LanguageContext, ct);
        return execution.Status switch
        {
            AiToolResultStatus.Success => AiToolExecutionResult<ContractExpiryLookupResult>.Success(execution.Result!, execution.Evidence),
            AiToolResultStatus.NoData => AiToolExecutionResult<ContractExpiryLookupResult>.NoData(execution.Evidence),
            _ => AiToolExecutionResult<ContractExpiryLookupResult>.Failure(execution.Error!, execution.Evidence)
        };
    }

    public async Task<AiToolExecutionOutcome> ExecuteAsync(
        AiToolInvocationRequest request,
        CancellationToken ct = default)
    {
        var input = request.Input switch
        {
            null => new ContractExpiryLookupInput(),
            ContractExpiryLookupInput value => value,
            _ => null
        };

        if (input is null)
            return AiToolExecutionOutcome.Failure(InvalidInputError(request.LanguageContext.Language));

        var execution = await ExecuteCoreAsync(input, request.OrganizationContext, request.LanguageContext, ct);
        if (execution.Status == AiToolResultStatus.NoData)
            return AiToolExecutionOutcome.NoData(execution.Evidence);

        if (execution.Status != AiToolResultStatus.Success)
            return AiToolExecutionOutcome.Failure(execution.Error!, execution.Evidence);

        return AiToolExecutionOutcome.Success(
            execution.Evidence,
            BuildAnswer(request.LanguageContext.Language, request.OrganizationContext, execution.Result!.Items));
    }

    private async Task<ContractExpiryExecution> ExecuteCoreAsync(
        ContractExpiryLookupInput input,
        AiOrganizationContext organization,
        AiLanguageContext language,
        CancellationToken ct)
    {
        if (!input.IsValid || input.DaysAhead != ExpiryWindowDays)
            return ContractExpiryExecution.Failure(InvalidInputError(language.Language));

        if (!userContext.AllowedOrganizationIds.Contains(organization.OrganizationId))
            return ContractExpiryExecution.Failure(UnauthorizedError(language.Language));

        var today = TashkentTime.Today;
        var endExclusive = today.AddDays(ExpiryWindowDays + 1);
        var candidates = await contractQuery.GetAllAsync(new QuerySpecification<Contract, ContractExpiryCandidate>
        {
            Criteria = contract =>
                contract.OrganizationId == organization.OrganizationId
                && contract.StateId == StateIdConst.ACTIVE
                && contract.EndDate.HasValue
                && contract.EndDate.Value < endExclusive,
            Selector = contract => new ContractExpiryCandidate(
                contract.ContractNumber,
                contract.Counterparty.FullName ?? contract.Counterparty.ShortName,
                contract.EndDate!.Value),
            OrderBy = query => query.OrderBy(candidate => candidate.EndDate)
        }, ct);

        var items = candidates
            .Select(candidate => ToItem(candidate, today, language.Language))
            .Where(item => item.DaysFromToday < 0 || item.DaysFromToday <= ExpiryWindowDays)
            .ToArray();

        var evidence = new AiToolEvidence(
            GetSource(language.Language),
            GetEvidenceDescription(language.Language),
            ToTashkentOffset(TashkentTime.Now),
            $"{today:yyyy-MM-dd} - {today.AddDays(ExpiryWindowDays):yyyy-MM-dd}");

        return items.Length == 0
            ? ContractExpiryExecution.NoData([evidence])
            : ContractExpiryExecution.Success(new ContractExpiryLookupResult(items), [evidence]);
    }

    private static ContractExpiryLookupItem ToItem(
        ContractExpiryCandidate candidate,
        DateTime today,
        AiLanguage language)
    {
        var expiryDate = DateOnly.FromDateTime(candidate.EndDate);
        var daysFromToday = expiryDate.DayNumber - DateOnly.FromDateTime(today).DayNumber;

        return new ContractExpiryLookupItem(
            candidate.ContractNumber,
            candidate.CounterpartyName,
            expiryDate,
            daysFromToday,
            daysFromToday < 0 ? GetExpiredLabel(language) : GetExpiringSoonLabel(language));
    }

    private static string BuildAnswer(
        AiLanguage language,
        AiOrganizationContext organization,
        IReadOnlyList<ContractExpiryLookupItem> items)
    {
        var heading = language switch
        {
            AiLanguage.UzbekLatin => $"{organization.Name} (INN {organization.Inn}) bo‘yicha muddati tugagan yoki yaqinlashgan shartnomalar:",
            AiLanguage.UzbekCyrillic => $"{organization.Name} (ИНН {organization.Inn}) бўйича муддати тугаган ёки яқинлашган шартномалар:",
            AiLanguage.Russian => $"Договоры с истекшим или ближайшим сроком для {organization.Name} (ИНН {organization.Inn}):",
            _ => $"Expired or soon-to-expire contracts for {organization.Name} (INN {organization.Inn}):"
        };

        var lines = items.Select(item =>
        {
            var counterparty = string.IsNullOrWhiteSpace(item.CounterpartyName)
                ? string.Empty
                : $" — {item.CounterpartyName}";
            var timing = GetTimingText(language, item.DaysFromToday);
            return $"• {item.ContractNumber}{counterparty}: {item.StatusLabel}, {item.ExpiryDate:yyyy-MM-dd} ({timing})";
        });

        return string.Join(Environment.NewLine, [heading, .. lines]);
    }

    private static string GetTimingText(AiLanguage language, int daysFromToday)
    {
        if (daysFromToday < 0)
        {
            var days = Math.Abs(daysFromToday);
            return language switch
            {
                AiLanguage.UzbekLatin => $"{days} kun oldin",
                AiLanguage.UzbekCyrillic => $"{days} кун олдин",
                AiLanguage.Russian => $"{days} дн. назад",
                _ => $"{days} days ago"
            };
        }

        if (daysFromToday == 0)
            return language switch
            {
                AiLanguage.UzbekLatin => "bugun",
                AiLanguage.UzbekCyrillic => "бугун",
                AiLanguage.Russian => "сегодня",
                _ => "today"
            };

        return language switch
        {
            AiLanguage.UzbekLatin => $"{daysFromToday} kundan keyin",
            AiLanguage.UzbekCyrillic => $"{daysFromToday} кундан кейин",
            AiLanguage.Russian => $"через {daysFromToday} дн.",
            _ => $"in {daysFromToday} days"
        };
    }

    private static string GetExpiredLabel(AiLanguage language) => language switch
    {
        AiLanguage.UzbekLatin => "Muddati tugagan",
        AiLanguage.UzbekCyrillic => "Муддати тугаган",
        AiLanguage.Russian => "Срок истёк",
        _ => "Expired"
    };

    private static string GetExpiringSoonLabel(AiLanguage language) => language switch
    {
        AiLanguage.UzbekLatin => "Muddati yaqin",
        AiLanguage.UzbekCyrillic => "Муддати яқин",
        AiLanguage.Russian => "Срок скоро истекает",
        _ => "Expiring soon"
    };

    private static string GetSource(AiLanguage language) => language switch
    {
        AiLanguage.UzbekLatin => "Shartnomalar reyestri",
        AiLanguage.UzbekCyrillic => "Шартномалар реестри",
        AiLanguage.Russian => "Реестр договоров",
        _ => "Contract register"
    };

    private static string GetEvidenceDescription(AiLanguage language) => language switch
    {
        AiLanguage.UzbekLatin => "Faol shartnomalarning tugash sanalari tekshirildi.",
        AiLanguage.UzbekCyrillic => "Фаол шартномаларнинг тугаш саналари текширилди.",
        AiLanguage.Russian => "Проверены даты окончания активных договоров.",
        _ => "End dates of active contracts were checked."
    };

    private static AiToolError InvalidInputError(AiLanguage language) => new(
        AiToolErrorKind.InvalidInput,
        language switch
        {
            AiLanguage.UzbekLatin => "Shartnoma muddati tekshiruvi uchun 30 kunlik davr qo‘llab-quvvatlanadi.",
            AiLanguage.UzbekCyrillic => "Шартнома муддати текшируви учун 30 кунлик давр қўллаб-қувватланади.",
            AiLanguage.Russian => "Для проверки срока договоров поддерживается период в 30 дней.",
            _ => "A 30-day period is supported for contract expiry checks."
        });

    private static AiToolError UnauthorizedError(AiLanguage language) => new(
        AiToolErrorKind.UnauthorizedOrganization,
        language switch
        {
            AiLanguage.UzbekLatin => "Tashkilot ma’lumotlariga kirish ruxsati mavjud emas.",
            AiLanguage.UzbekCyrillic => "Ташкилот маълумотларига кириш учун рухсат мавжуд эмас.",
            AiLanguage.Russian => "Нет доступа к данным организации.",
            _ => "Access to the organization data is not allowed."
        });

    private static DateTimeOffset ToTashkentOffset(DateTime value) =>
        new(value, TashkentTime.Zone.GetUtcOffset(value));

    private sealed record ContractExpiryCandidate(
        string ContractNumber,
        string? CounterpartyName,
        DateTime EndDate);

    private sealed record ContractExpiryExecution(
        AiToolResultStatus Status,
        ContractExpiryLookupResult? Result,
        IReadOnlyList<AiToolEvidence> Evidence,
        AiToolError? Error)
    {
        public static ContractExpiryExecution Success(
            ContractExpiryLookupResult result,
            IReadOnlyList<AiToolEvidence> evidence) =>
            new(AiToolResultStatus.Success, result, evidence, null);

        public static ContractExpiryExecution NoData(IReadOnlyList<AiToolEvidence> evidence) =>
            new(AiToolResultStatus.NoData, null, evidence, null);

        public static ContractExpiryExecution Failure(AiToolError error) =>
            new(AiToolResultStatus.Error, null, [], error);
    }
}
