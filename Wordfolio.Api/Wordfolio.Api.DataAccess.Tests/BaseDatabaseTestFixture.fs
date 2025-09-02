namespace Wordfolio.Api.DataAccess.Tests

open System
open System.Data
open System.Data.Common
open System.Threading
open System.Threading.Tasks

open Dapper.FSharp.PostgreSQL
open FluentMigrator.Runner
open Microsoft.Extensions.DependencyInjection
open Npgsql
open Testcontainers.PostgreSql
open Testcontainers.Xunit
open Xunit
open Xunit.Sdk

open Wordfolio.Api.Migrations

 type BaseDatabaseTestFixture(messageSink: IMessageSink) =
    inherit DbContainerFixture<PostgreSqlBuilder, PostgreSqlContainer>(messageSink)

    let mutable state: {| Seeder: TestDatabaseSeeder
                          SeederConnection: DbConnection |} option = None

    override this.Configure (builder: PostgreSqlBuilder): PostgreSqlBuilder =
        base.Configure(builder).WithImage("postgres:17.5")

    override _.DbProviderFactory: DbProviderFactory = NpgsqlFactory.Instance

    member private this.CreateSeeder(): TestDatabaseSeeder * DbConnection =
        let connection = this.CreateConnection()
        connection.Open()
        let seeder = DatabaseSeeder.create connection
        seeder, connection

    member private this.EnsureMigrated() =
        let connectionString = this.Container.GetConnectionString()
        let serviceProvider =
            ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(fun builder ->
                    builder
                        .AddPostgres()
                        .WithGlobalConnectionString(connectionString)
                        .ScanIn(typeof<CreateUsersTable>.Assembly)
                        .For.Migrations()
                    |> ignore)
                .BuildServiceProvider()
        let scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope()
        let runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>()
        runner.MigrateUp()

    member private this.EnsureInitialized() =
        match state with
        | None ->
            this.EnsureMigrated()
            OptionTypes.register()
            let seeder, connection = this.CreateSeeder()
            state <- {| Seeder = seeder
                        SeederConnection = connection |}
                      |> Some
        | Some _ -> ()

    member this.Seeder with get(): TestDatabaseSeeder =
        this.EnsureInitialized()
        match state with
        | Some s -> s.Seeder
        | None -> failwith "State is not initialized"

    member this.WithConnectionAsync
        (callback: IDbConnection -> IDbTransaction -> CancellationToken -> Task<'a>)
        : Task<'a> =
        task {
            this.EnsureInitialized()

            let cancellationToken = TestContext.Current.CancellationToken

            use connection = this.CreateConnection()
            connection.Open()

            use transaction = connection.BeginTransaction()

            let! result = callback connection transaction cancellationToken

            do! transaction.CommitAsync()

            return result
        }

    interface IDisposable with
        member this.Dispose (): unit =
            match state with
            | None -> ()
            | Some state ->
                (state.Seeder :> IDisposable).Dispose()
                (state.SeederConnection :> IDisposable).Dispose()