using Application.Features.Acc.PostingTemplateViews;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Results;
using System.Data;

namespace Infrastructure.Query;

public class PostingTemplateViewService : IPostingTemplateViewService
{
    private readonly AppDbContext _db;

    public PostingTemplateViewService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<List<PostingTemplateViewListDto>>> GetAllAsync(CancellationToken ct = default)
    {
        const string sql = """
            select
                r.id,
                r.code,
                r.name,
                count(l.id)::int as lines_count
            from acc_posting_rule r
            left join acc_posting_rule_line l on l.template_id = r.id
            group by r.id, r.code, r.name
            order by r.id
            """;

        var rows = await QueryAsync(sql, null, ReadListDto, ct);
        return Result.Success(rows);
    }

    public async Task<Result<PostingTemplateViewDto>> GetByIdAsync(short id, short policyId = 1, CancellationToken ct = default)
    {
        const string headerSql = """
            select
                r.id,
                r.code,
                r.name,
                count(l.id)::int as lines_count
            from acc_posting_rule r
            left join acc_posting_rule_line l on l.template_id = r.id
            where r.id = @id
            group by r.id, r.code, r.name
            """;

        var headers = await QueryAsync(
            headerSql,
            command => AddParameter(command, "id", id),
            ReadListDto,
            ct);

        var header = headers.FirstOrDefault();
        if (header is null)
            return Result.Failure<PostingTemplateViewDto>(
                Error.NotFound("PostingTemplate.NotFound", $"Posting template with id '{id}' was not found."));

        const string lineSql = """
            select id, template_id, order_number, debit_alias, credit_alias, amount_source, is_optional
            from acc_posting_rule_line
            where template_id = @template_id
            order by order_number, id
            """;

        var lines = await QueryAsync(
            lineSql,
            command => AddParameter(command, "template_id", header.Id),
            ReadLineDto,
            ct);

        var aliases = lines
            .SelectMany(x => new[] { x.DebitAlias, x.CreditAlias })
            .Distinct()
            .ToList();

        var resolveRules = aliases.Count == 0
            ? new List<AccountAliasResolveDto>()
            : await GetResolveRulesAsync(aliases, policyId, ct);

        var rulesByAlias = resolveRules
            .GroupBy(x => x.Alias)
            .ToDictionary(x => x.Key, x => x.OrderBy(r => r.Priority).ThenBy(r => r.Id).ToList());

        foreach (var line in lines)
        {
            line.DebitResolveRules = rulesByAlias.GetValueOrDefault(line.DebitAlias, new List<AccountAliasResolveDto>());
            line.CreditResolveRules = rulesByAlias.GetValueOrDefault(line.CreditAlias, new List<AccountAliasResolveDto>());
        }

        var result = new PostingTemplateViewDto
        {
            Id = header.Id,
            Code = header.Code,
            Name = header.Name,
            DocumentTypeId = header.DocumentTypeId,
            DocumentTypeCode = header.DocumentTypeCode,
            DocumentTypeName = header.DocumentTypeName,
            LinesCount = header.LinesCount,
            PolicyId = policyId,
            Lines = lines
        };

        return Result.Success(result);
    }

    private async Task<List<AccountAliasResolveDto>> GetResolveRulesAsync(List<string> aliases, short policyId, CancellationToken ct)
    {
        var placeholders = aliases.Select((_, index) => $"@alias{index}").ToList();
        var sql = $"""
            select
                rr.id,
                rr.policy_id,
                rr.alias,
                rr.dimension_key,
                rr.dimension_value,
                rr.account_id,
                ca.code as account_code,
                ca.name as account_name,
                rr.priority
            from acc_account_resolve_rule rr
            join acc_chart_account ca on ca.id = rr.account_id
            where rr.policy_id = @policy_id
              and rr.alias in ({string.Join(", ", placeholders)})
            order by rr.alias, rr.priority, rr.id
            """;

        return await QueryAsync(
            sql,
            command =>
            {
                AddParameter(command, "policy_id", policyId);
                for (var i = 0; i < aliases.Count; i++)
                    AddParameter(command, $"alias{i}", aliases[i]);
            },
            ReadResolveDto,
            ct);
    }

    private async Task<List<T>> QueryAsync<T>(
        string sql,
        Action<IDbCommand>? configure,
        Func<IDataRecord, T> map,
        CancellationToken ct)
    {
        var connection = _db.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;

        if (shouldClose)
            await connection.OpenAsync(ct);

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            configure?.Invoke(command);

            var result = new List<T>();
            await using var reader = await command.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
                result.Add(map(reader));

            return result;
        }
        finally
        {
            if (shouldClose)
                await connection.CloseAsync();
        }
    }

    private static PostingTemplateViewListDto ReadListDto(IDataRecord row) =>
        new()
        {
            Id = Convert.ToInt16(row["id"]),
            Code = Convert.ToString(row["code"])!,
            Name = Convert.ToString(row["name"])!,
            DocumentTypeId = Convert.ToInt16(row["id"]),
            DocumentTypeCode = Convert.ToString(row["code"])!,
            DocumentTypeName = Convert.ToString(row["name"])!,
            LinesCount = Convert.ToInt32(row["lines_count"])
        };

    private static PostingTemplateLineViewDto ReadLineDto(IDataRecord row) =>
        new()
        {
            Id = Convert.ToInt32(row["id"]),
            OrderNumber = Convert.ToInt16(row["order_number"]),
            DebitAlias = Convert.ToString(row["debit_alias"])!,
            CreditAlias = Convert.ToString(row["credit_alias"])!,
            AmountSource = row["amount_source"] == DBNull.Value ? null : Convert.ToString(row["amount_source"]),
            IsOptional = Convert.ToBoolean(row["is_optional"])
        };

    private static AccountAliasResolveDto ReadResolveDto(IDataRecord row) =>
        new()
        {
            Id = Convert.ToInt32(row["id"]),
            PolicyId = Convert.ToInt16(row["policy_id"]),
            Alias = Convert.ToString(row["alias"])!,
            DimensionKey = Convert.ToString(row["dimension_key"])!,
            DimensionValue = Convert.ToString(row["dimension_value"])!,
            AccountId = Convert.ToInt32(row["account_id"]),
            AccountCode = Convert.ToString(row["account_code"])!,
            AccountName = Convert.ToString(row["account_name"])!,
            Priority = Convert.ToInt32(row["priority"])
        };

    private static void AddParameter(IDbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
