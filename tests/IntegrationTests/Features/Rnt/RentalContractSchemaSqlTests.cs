using System.Data.Common;
using IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.Features.Rnt;

[Collection(PostgreSqlIntegrationFixture.CollectionName)]
public sealed class RentalContractSchemaSqlTests(PostgreSqlIntegrationFixture fixture)
{
    [Fact]
    public async Task ScriptsMigrateLessorsAndCreateRentalDetailsWithDatabaseConstraints()
    {
        await using var context = fixture.CreateDbContext();
        await context.Database.OpenConnectionAsync();
        var connection = context.Database.GetDbConnection();
        var schema = $"rental_contract_{Guid.NewGuid():N}";

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
                insert into cmn_language (id, code)
                values (1, 'uz'), (2, 'uz_Cyrl'), (3, 'ru'), (4, 'en');

                create table org_organization (id int primary key);
                insert into org_organization (id) values (1), (2);

                create table counterparty_card
                (
                    id serial primary key,
                    organization_id int not null references org_organization(id),
                    short_name varchar(250) not null,
                    full_name varchar(500),
                    inn varchar(20),
                    state_id smallint not null references cmn_state(id)
                );
                insert into counterparty_card
                    (id, organization_id, short_name, full_name, inn, state_id)
                values (10, 1, 'Legal lessor', 'Legal lessor LLC', '309999999', 1);

                create table cmn_currency (id smallint primary key);
                insert into cmn_currency (id) values (1);

                create table acc_chart_account (id int primary key);
                insert into acc_chart_account (id) values (1), (2), (3);

                create table cmn_document_status (id smallint primary key);
                insert into cmn_document_status (id) values (1);

                create table rnt_rental_object_type (id smallint primary key);
                insert into rnt_rental_object_type (id) values (1);

                create table rnt_contract
                (
                    id bigserial primary key,
                    organization_id int not null references org_organization(id),
                    lessor_full_name varchar(500) not null,
                    lessor_inn varchar(20),
                    lessor_pinfl varchar(14),
                    contract_number varchar(100) not null,
                    contract_date date not null,
                    start_date date not null,
                    end_date date not null,
                    currency_id smallint not null references cmn_currency(id),
                    lessor_payable_account_id int references acc_chart_account(id),
                    tax_payable_account_id int references acc_chart_account(id),
                    status_id smallint not null references cmn_document_status(id),
                    state_id smallint not null default 1 references cmn_state(id),
                    created_date timestamp without time zone not null default now()
                );

                create table rnt_contract_object
                (
                    id bigserial primary key,
                    contract_id bigint not null references rnt_contract(id),
                    rental_object_type_id smallint not null references rnt_rental_object_type(id),
                    object_name varchar(500) not null,
                    start_date date not null,
                    end_date date not null,
                    period_unit varchar(20) not null,
                    period_value int not null default 1,
                    next_accrual_date date not null,
                    contract_amount numeric(24, 8) not null,
                    tax_base_amount numeric(24, 8) not null,
                    tax_rate numeric(9, 6) not null,
                    state_id smallint not null default 1 references cmn_state(id),
                    created_date timestamp without time zone not null default now(),
                    check (contract_amount > 0),
                    check (tax_base_amount >= contract_amount)
                );

                insert into rnt_contract
                    (organization_id, lessor_full_name, lessor_inn, lessor_pinfl,
                     contract_number, contract_date, start_date, end_date, currency_id,
                     lessor_payable_account_id, tax_payable_account_id, status_id)
                values
                    (1, 'Existing Individual', '301111111', '12345678901234',
                     'R-1', date '2026-01-01', date '2026-01-01', date '2026-12-31',
                     1, 1, 2, 1);

                insert into rnt_contract_object
                    (contract_id, rental_object_type_id, object_name, start_date, end_date,
                     period_unit, next_accrual_date, contract_amount, tax_base_amount, tax_rate)
                values
                    (1, 1, 'Existing object', date '2026-01-01', date '2026-12-31',
                     'MONTH', date '2026-01-01', 5000000, 6000000, 12);
                """);

            foreach (var scriptPath in ScriptPaths())
            {
                Assert.True(File.Exists(scriptPath), $"Expected SQL script was not found: {scriptPath}");
                await ExecuteAsync(connection, await File.ReadAllTextAsync(scriptPath));
            }

            Assert.Equal(4L, await ScalarAsync<long>(connection, "select count(*) from cmn_utility_service"));
            Assert.Equal(16L, await ScalarAsync<long>(connection, "select count(*) from cmn_utility_service_translation"));
            Assert.Equal(
                "Электроэнергия",
                await ScalarAsync<string>(
                    connection,
                    """
                    select translation.name
                    from cmn_utility_service service
                    join cmn_utility_service_translation translation on translation.utility_service_id = service.id
                    join cmn_language language on language.id = translation.language_id
                    where service.code = 'ELECTRICITY' and language.code = 'ru'
                    """));

            Assert.Equal(1L, await ScalarAsync<long>(connection, "select count(*) from rnt_lessor"));
            Assert.Equal(
                "INDIVIDUAL:Existing Individual",
                await ScalarAsync<string>(connection, "select lessor_kind_code || ':' || full_name from rnt_lessor"));
            Assert.Equal(1L, await ScalarAsync<long>(connection, "select count(*) from rnt_contract_lessor"));
            Assert.False(await ColumnExistsAsync(connection, "rnt_contract", "lessor_full_name"));
            Assert.False(await ColumnExistsAsync(connection, "rnt_contract", "lessor_inn"));
            Assert.False(await ColumnExistsAsync(connection, "rnt_contract", "lessor_pinfl"));
            Assert.True(await ColumnExistsAsync(connection, "rnt_contract", "is_free_of_charge"));
            Assert.True(await ColumnExistsAsync(connection, "rnt_contract_object", "total_area"));
            Assert.True(await ColumnExistsAsync(connection, "rnt_contract_object", "rented_area"));
            Assert.False(await ScalarAsync<bool>(connection, "select is_free_of_charge from rnt_contract where id = 1"));

            await ExecuteAsync(
                connection,
                """
                insert into rnt_lessor
                    (organization_id, lessor_kind_code, counterparty_id, full_name, inn,
                     phone_number, registered_address, state_id)
                values
                    (1, 'LEGAL_ENTITY', 10, 'Legal lessor LLC', '309999999',
                     '+998711234567', 'Registered address', 1)
                """);

            await AssertRejectedAsync(
                connection,
                """
                insert into rnt_lessor
                    (organization_id, lessor_kind_code, full_name, inn, state_id)
                values (1, 'LEGAL_ENTITY', 'Missing counterparty', '308888888', 1)
                """);

            await AssertRejectedAsync(
                connection,
                """
                insert into rnt_lessor
                    (organization_id, lessor_kind_code, counterparty_id, full_name, pinfl, state_id)
                values (1, 'INDIVIDUAL', 10, 'Invalid individual', '11111111111111', 1)
                """);

            await ExecuteAsync(
                connection,
                """
                update rnt_contract_object
                set total_area = 48.22,
                    rented_area = 20
                where id = 1;

                insert into rnt_contract_object_utility
                    (contract_object_id, utility_service_id, payer_code)
                values
                    (1, (select id from cmn_utility_service where code = 'NATURAL_GAS'), 'LESSOR'),
                    (1, (select id from cmn_utility_service where code = 'ELECTRICITY'), 'LESSEE');
                """);

            Assert.Equal(2L, await ScalarAsync<long>(connection, "select count(*) from rnt_contract_object_utility"));

            await AssertRejectedAsync(
                connection,
                "update rnt_contract_object set total_area = 10, rented_area = 11 where id = 1");
            await AssertRejectedAsync(
                connection,
                """
                insert into rnt_contract_object_utility
                    (contract_object_id, utility_service_id, payer_code)
                values
                    (1, (select id from cmn_utility_service where code = 'HOT_WATER'), 'UNKNOWN')
                """);

            await ExecuteAsync(
                connection,
                """
                update rnt_contract set is_free_of_charge = true where id = 1;
                update rnt_contract_object
                set contract_amount = 0,
                    tax_base_amount = 0,
                    tax_rate = 0
                where id = 1
                """);
            Assert.Equal(0m, await ScalarAsync<decimal>(connection, "select contract_amount from rnt_contract_object where id = 1"));
            await AssertRejectedAsync(connection, "update rnt_contract_object set contract_amount = -1 where id = 1");
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
            Path.Combine(scripts, "01_cmn", "0182_create_cmn_utility_service.sql"),
            Path.Combine(scripts, "18_rnt", "1806_expand_rental_contract.sql")
        ];
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Accounting.slnx")))
            directory = directory.Parent;

        return directory?.FullName ?? throw new DirectoryNotFoundException("Accounting solution root was not found.");
    }

    private static async Task<bool> ColumnExistsAsync(DbConnection connection, string tableName, string columnName) =>
        await ScalarAsync<bool>(
            connection,
            $"""
            select exists
            (
                select 1
                from information_schema.columns
                where table_schema = current_schema()
                  and table_name = '{tableName}'
                  and column_name = '{columnName}'
            )
            """);

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
