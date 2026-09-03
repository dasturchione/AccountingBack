using System.Data.Common;
using IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.Features.Cmn.RegulatedObligations;

[Collection(PostgreSqlIntegrationFixture.CollectionName)]
public sealed class RegulatedObligationSqlTests(PostgreSqlIntegrationFixture fixture)
{
    [Fact]
    public async Task ScriptsCreateCataloguesTranslationsAndOrganizationConstraints()
    {
        await using var context = fixture.CreateDbContext();
        await context.Database.OpenConnectionAsync();
        var connection = context.Database.GetDbConnection();
        var schema = $"regulated_obligation_{Guid.NewGuid():N}";

        try
        {
            await ExecuteAsync(
                connection,
                $"""
                create schema "{schema}";
                set search_path to "{schema}";

                create table cmn_state (id smallint primary key);
                insert into cmn_state (id) values (1), (2);

                create table cmn_language
                (
                    id smallint primary key,
                    code varchar(10) not null unique
                );
                insert into cmn_language (id, code) values (1, 'uz'), (2, 'ru'), (3, 'en');

                create table org_organization (id int primary key);
                create table acc_chart_account
                (
                    id int primary key,
                    organization_id int not null references org_organization(id)
                );

                create table cmn_tax_type (id smallint primary key);
                create table org_tax_settings (id int primary key);
                create table cmn_vat_rate (id smallint primary key);

                create table acc_subkonto_type
                (
                    id smallint primary key,
                    code varchar(50) not null,
                    name varchar(150) not null,
                    source_table varchar(100) not null
                );
                insert into acc_subkonto_type (id, code, name, source_table)
                values (29, 'tax_types', 'Soliq turlari', 'cmn_tax_type');
                """);

            foreach (var scriptPath in ScriptPaths())
            {
                Assert.True(File.Exists(scriptPath), $"Expected SQL script was not found: {scriptPath}");
                await ExecuteAsync(connection, await File.ReadAllTextAsync(scriptPath));
            }

            await ExecuteAsync(
                connection,
                """
                insert into org_organization (id) values (1);
                insert into acc_chart_account (id, organization_id) values (10, 1);

                insert into cmn_regulated_obligation (category_id, code, name, state_id)
                values ((select id from cmn_regulated_obligation_category where code = 'TAX'), '000000007', 'QQS', 1);

                insert into cmn_regulated_obligation_translation (regulated_obligation_id, language_id, name)
                select obligation.id, language.id, 'legacy VAT'
                from cmn_regulated_obligation obligation
                cross join cmn_language language
                where obligation.code = '000000007';

                insert into org_regulated_obligation_setting
                    (organization_id, regulated_obligation_id, periodicity_id, classifier_code,
                     rate, chart_account_id, effective_from, effective_to, state_id)
                values
                    (1,
                     (select id from cmn_regulated_obligation where code = '000000007'),
                     (select id from cmn_regulated_obligation_periodicity where code = 'MONTHLY'),
                     '1', 12, 10, date '2026-01-01', null, 1);
                """);

            var seedScript = ScriptPaths().Single(path => Path.GetFileName(path) == "0181_insert_cmn_regulated_obligation_catalogues.sql");
            await ExecuteAsync(connection, await File.ReadAllTextAsync(seedScript));

            Assert.Equal(2L, await ScalarAsync<long>(connection, "select count(*) from cmn_regulated_obligation_category"));
            Assert.Equal(6L, await ScalarAsync<long>(connection, "select count(*) from cmn_regulated_obligation_category_translation"));
            Assert.Equal(26L, await ScalarAsync<long>(connection, "select count(*) from cmn_regulated_obligation"));
            Assert.Equal(78L, await ScalarAsync<long>(connection, "select count(*) from cmn_regulated_obligation_translation"));
            Assert.Equal(6L, await ScalarAsync<long>(connection, "select count(*) from cmn_regulated_obligation_periodicity"));
            Assert.Equal(18L, await ScalarAsync<long>(connection, "select count(*) from cmn_regulated_obligation_periodicity_translation"));
            Assert.Equal(
                "TAX:Налог на добавленную стоимость",
                await ScalarAsync<string>(
                    connection,
                    """
                    select category.code || ':' || translation.name
                    from cmn_regulated_obligation obligation
                    join cmn_regulated_obligation_category category on category.id = obligation.category_id
                    join cmn_regulated_obligation_translation translation on translation.regulated_obligation_id = obligation.id
                    join cmn_language language on language.id = translation.language_id
                    where obligation.code = 'VAT' and language.code = 'ru'
                    """));
            Assert.Equal(
                "CONTRIBUTION:Взносы в профсоюз из заработной платы",
                await ScalarAsync<string>(
                    connection,
                    """
                    select category.code || ':' || translation.name
                    from cmn_regulated_obligation obligation
                    join cmn_regulated_obligation_category category on category.id = obligation.category_id
                    join cmn_regulated_obligation_translation translation on translation.regulated_obligation_id = obligation.id
                    join cmn_language language on language.id = translation.language_id
                    where obligation.code = 'TRADE_UNION_SALARY_CONTRIBUTION' and language.code = 'ru'
                    """));
            Assert.Equal(
                1L,
                await ScalarAsync<long>(
                    connection,
                    """
                    select count(*)
                    from cmn_regulated_obligation obligation
                    join cmn_regulated_obligation_translation translation on translation.regulated_obligation_id = obligation.id
                    join cmn_language language on language.id = translation.language_id
                    where language.code = 'ru'
                      and translation.name = 'Единый социальный платеж'
                    """));
            Assert.False(await ScalarAsync<bool>(connection, "select exists(select 1 from cmn_regulated_obligation where code ~ '^[0-9]+$')"));
            Assert.False(await ScalarAsync<bool>(connection, "select exists(select 1 from cmn_regulated_obligation where code in ('000000001', '000000002'))"));
            Assert.Equal(
                "VAT",
                await ScalarAsync<string>(
                    connection,
                    """
                    select obligation.code
                    from org_regulated_obligation_setting setting
                    join cmn_regulated_obligation obligation on obligation.id = setting.regulated_obligation_id
                    where setting.organization_id = 1
                    """));
            Assert.True(await ScalarAsync<bool>(connection, "select to_regclass('cmn_tax_type') is null"));
            Assert.True(await ScalarAsync<bool>(connection, "select to_regclass('org_tax_settings') is null"));
            Assert.True(await ScalarAsync<bool>(connection, "select to_regclass('cmn_vat_rate') is not null"));
            Assert.Equal(
                "cmn_regulated_obligation",
                await ScalarAsync<string>(connection, "select source_table from acc_subkonto_type where id = 29"));

            Assert.Equal(
                0L,
                await ScalarAsync<long>(
                    connection,
                    """
                    select count(*)
                    from information_schema.columns
                    where table_schema = current_schema()
                      and table_name in ('cmn_regulated_obligation_category', 'cmn_regulated_obligation')
                      and column_name = 'updated_date'
                    """));

            Assert.Equal(
                1L,
                await ScalarAsync<long>(
                    connection,
                    """
                    select count(*)
                    from information_schema.columns
                    where table_schema = current_schema()
                      and table_name = 'org_regulated_obligation_setting'
                      and column_name = 'updated_date'
                    """));

            await AssertRejectedAsync(
                connection,
                """
                insert into org_regulated_obligation_setting
                    (organization_id, regulated_obligation_id, periodicity_id, rate,
                     chart_account_id, effective_from, effective_to, state_id)
                values
                    (1,
                     (select id from cmn_regulated_obligation where code = 'VAT'),
                     (select id from cmn_regulated_obligation_periodicity where code = 'MONTHLY'),
                     10, 10, date '2027-01-01', null, 1)
                """);

            await AssertRejectedAsync(
                connection,
                """
                insert into org_regulated_obligation_setting
                    (organization_id, regulated_obligation_id, periodicity_id, rate,
                     chart_account_id, effective_from, effective_to, state_id)
                values
                    (1,
                     (select id from cmn_regulated_obligation where code = 'VAT'),
                     (select id from cmn_regulated_obligation_periodicity where code = 'MONTHLY'),
                     100.000001, 10, date '2025-01-01', date '2025-12-31', 1)
                """);

            await AssertRejectedAsync(
                connection,
                """
                insert into org_regulated_obligation_setting
                    (organization_id, regulated_obligation_id, periodicity_id, rate,
                     chart_account_id, effective_from, effective_to, state_id)
                values
                    (1,
                     (select id from cmn_regulated_obligation where code = 'VAT'),
                     (select id from cmn_regulated_obligation_periodicity where code = 'MONTHLY'),
                     12, 10, date '2025-12-31', date '2025-01-01', 1)
                """);

            await AssertRejectedAsync(
                connection,
                """
                insert into org_regulated_obligation_setting
                    (organization_id, regulated_obligation_id, periodicity_id, classifier_code,
                     rate, chart_account_id, effective_from, effective_to, state_id)
                values
                    (1,
                     (select id from cmn_regulated_obligation where code = 'VAT'),
                     (select id from cmn_regulated_obligation_periodicity where code = 'MONTHLY'),
                     '   ', 12, 10, date '2025-01-01', date '2025-12-31', 1)
                """);
        }
        finally
        {
            await ExecuteAsync(connection, $"set search_path to public; drop schema if exists \"{schema}\" cascade;");
        }
    }

    private static IReadOnlyList<string> ScriptPaths()
    {
        var root = FindRepositoryRoot();
        var scripts = Path.Combine(root, "src", "Infrastructure", "Persistence", "Scripts");

        return
        [
            Path.Combine(scripts, "01_cmn", "0175_create_cmn_regulated_obligation_category.sql"),
            Path.Combine(scripts, "01_cmn", "0176_create_cmn_regulated_obligation_category_translation.sql"),
            Path.Combine(scripts, "01_cmn", "0177_create_cmn_regulated_obligation.sql"),
            Path.Combine(scripts, "01_cmn", "0178_create_cmn_regulated_obligation_translation.sql"),
            Path.Combine(scripts, "01_cmn", "0179_create_cmn_regulated_obligation_periodicity.sql"),
            Path.Combine(scripts, "01_cmn", "0180_create_cmn_regulated_obligation_periodicity_translation.sql"),
            Path.Combine(scripts, "01_cmn", "0181_insert_cmn_regulated_obligation_catalogues.sql"),
            Path.Combine(scripts, "02_org", "0211_create_org_regulated_obligation_setting.sql"),
            Path.Combine(scripts, "02_org", "0212_drop_legacy_tax_settings.sql")
        ];
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Accounting.slnx")))
            directory = directory.Parent;

        return directory?.FullName ?? throw new DirectoryNotFoundException("Accounting solution root was not found.");
    }

    private static async Task AssertRejectedAsync(DbConnection connection, string sql) =>
        await Assert.ThrowsAsync<DbException>(() => ExecuteAsync(connection, sql));

    private static async Task ExecuteAsync(DbConnection connection, string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<T> ScalarAsync<T>(DbConnection connection, string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        return (T)(await command.ExecuteScalarAsync())!;
    }
}
