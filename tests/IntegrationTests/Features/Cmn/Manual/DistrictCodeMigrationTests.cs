using System.Data.Common;
using IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.Features.Cmn.Manual;

[Collection(PostgreSqlIntegrationFixture.CollectionName)]
public sealed class DistrictCodeMigrationTests(PostgreSqlIntegrationFixture fixture)
{
    [Fact]
    public async Task DuplicateDistrictMigrationKeepsOneRowAndRepointsForeignKeys()
    {
        await using var context = fixture.CreateDbContext();
        await context.Database.OpenConnectionAsync();
        var connection = context.Database.GetDbConnection();
        var schema = $"district_code_{Guid.NewGuid():N}";

        try
        {
            await ExecuteAsync(
                connection,
                $"""
                create schema "{schema}";
                set search_path to "{schema}";

                create table cmn_district
                (
                    id int primary key,
                    code varchar(10),
                    short_name varchar(250) not null,
                    full_name varchar(250) not null,
                    region_id int not null
                );

                create table org_organization (id int primary key, district_id int references cmn_district(id));
                create table org_branch (id int primary key, district_id int references cmn_district(id));
                create table counterparty_card (id int primary key, district_id int references cmn_district(id));
                create table cmn_bank_branch (id int primary key, district_id int references cmn_district(id));

                insert into cmn_district (id, code, short_name, full_name, region_id) values
                    (1, '11', 'Bektemir', 'Bektemir', 1),
                    (28, '11', 'Bektemir', 'Bektemir', 1),
                    (12, null, 'Bo‘zatov', 'Bo‘zatov', 6),
                    (111, null, 'Bo‘zatov', 'Bo‘zatov', 6),
                    (29, '2', 'Mirzo Ulug‘bek', 'Mirzo Ulug‘bek', 1);

                insert into org_organization values (1, 28);
                insert into org_branch values (1, 111);
                insert into counterparty_card values (1, 29);
                insert into cmn_bank_branch values (1, 28);
                """);

            var migrationSql = await File.ReadAllTextAsync(MigrationPath());
            await ExecuteAsync(connection, migrationSql);

            Assert.Equal(3L, await ScalarAsync<long>(connection, "select count(*) from cmn_district"));
            Assert.Equal(1, await ScalarAsync<int>(connection, "select district_id from org_organization where id = 1"));
            Assert.Equal(12, await ScalarAsync<int>(connection, "select district_id from org_branch where id = 1"));
            Assert.Equal(29, await ScalarAsync<int>(connection, "select district_id from counterparty_card where id = 1"));
            Assert.Equal(1, await ScalarAsync<int>(connection, "select district_id from cmn_bank_branch where id = 1"));
            Assert.Equal(0L, await ScalarAsync<long>(
                connection,
                """
                select count(*)
                from
                (
                    select region_id, code
                    from cmn_district
                    where code is not null
                    group by region_id, code
                    having count(*) > 1
                ) duplicate_codes
                """));
        }
        finally
        {
            await ExecuteAsync(connection, $"rollback; set search_path to public; drop schema if exists \"{schema}\" cascade;");
        }
    }

    private static string MigrationPath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Accounting.slnx")))
            directory = directory.Parent;

        if (directory is null)
            throw new DirectoryNotFoundException("Accounting solution root was not found.");

        return Path.Combine(
            directory.FullName,
            "src",
            "Infrastructure",
            "Persistence",
            "Scripts",
            "01_cmn",
            "0174_remove_duplicate_districts.sql");
    }

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
