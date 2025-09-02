namespace Wordfolio.Api.DataAccess.Tests

open Testcontainers.PostgreSql
open Npgsql
open Xunit

open Dapper.FSharp.PostgreSQL
open Wordfolio.Common

module private Sql =
    [<Literal>]
    let CreateSchema = "CREATE SCHEMA IF NOT EXISTS wordfolio;"

    [<Literal>]
    let CreateUsers = "CREATE TABLE IF NOT EXISTS wordfolio.\"Users\" (\n  \"Id\" INT PRIMARY KEY\n);"

type PostgresFixture() =
    let container =
        PostgreSqlBuilder()
            .WithImage("postgres:17.5")
            .WithDatabase("wordfoliodb-test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build()

    let mutable dataSource : NpgsqlDataSource option = None

    do OptionTypes.register()

    interface IAsyncLifetime with
        member _.InitializeAsync() = task {
            do! container.StartAsync()
            let cs = container.GetConnectionString()
            let builder = NpgsqlDataSourceBuilder(cs)
            // Enable mapping to open connections easily
            dataSource <- Some (builder.Build())

            // bootstrap schema
            use! conn = dataSource.Value.OpenConnectionAsync()
            do! (new NpgsqlCommand(Sql.CreateSchema, conn)).ExecuteNonQueryAsync() |> Task.ignore
            do! (new NpgsqlCommand(Sql.CreateUsers, conn)).ExecuteNonQueryAsync() |> Task.ignore
        }

        member _.DisposeAsync() = task {
            match dataSource with
            | Some ds -> ds.Dispose()
            | None -> ()
            do! container.StopAsync()
            do! container.DisposeAsync().AsTask()
        }

    member _.DataSource = dataSource.Value

[<CollectionDefinition("postgres")>]
type PostgresCollection() =
    interface Xunit.ICollectionFixture<PostgresFixture>
