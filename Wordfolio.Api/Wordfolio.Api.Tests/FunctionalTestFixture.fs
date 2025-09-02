namespace Wordfolio.Api.Tests

open System
open System.Data
open System.Data.Common
open System.Threading
open System.Threading.Tasks

open Dapper.FSharp.PostgreSQL
open FluentMigrator.Runner
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.DependencyInjection
open Npgsql
open Testcontainers.PostgreSql
open Testcontainers.Xunit
open Xunit
open Xunit.Sdk

open Wordfolio.Api.DataAccess.Tests
open Wordfolio.Api.Identity
open Wordfolio.Api.Migrations

type FunctionalTestFixture(messageSink: IMessageSink) =
    inherit DbContainerFixture<PostgreSqlBuilder, PostgreSqlContainer>(messageSink)

    let mutable state
        : {| Seeder: TestDatabaseSeeder
             Connection: DbConnection |} option =
        None

    override this.Configure(builder: PostgreSqlBuilder) : PostgreSqlBuilder =
        base.Configure(builder).WithImage("postgres:17.5")

    override _.DbProviderFactory: DbProviderFactory =
        NpgsqlFactory.Instance

    member private this.MigrateWordfolio() =
        let connectionString =
            this.Container.GetConnectionString()

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

        let scope =
            serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope()

        let runner =
            scope.ServiceProvider.GetRequiredService<IMigrationRunner>()

        runner.MigrateUp()

    member private this.MigrateIdentity() =
        let options =
            DbContextOptionsBuilder<IdentityDbContext>()
                .UseNpgsql(
                    this.ConnectionString,
                    fun o ->
                        o.MigrationsHistoryTable(Constants.MigrationsHistoryTableName, Constants.SchemaName)
                        |> ignore)
                .Options

        use identityContext = new IdentityDbContext(options)
        identityContext.Database.Migrate()

    member private this.EnsureInitialized() =
        match state with
        | None ->
            OptionTypes.register()

            let connection = this.CreateConnection()
            connection.Open()

            state <-
                {| Seeder = DatabaseSeeder.create connection
                   Connection = connection |}
                |> Some

            this.MigrateIdentity()
            this.MigrateWordfolio()
        | Some _ -> ()

    override this.InitializeAsync() : ValueTask =
        // https://github.com/dotnet/fsharp/issues/2307
        do base.InitializeAsync().GetAwaiter().GetResult()

        this.EnsureInitialized()
        ValueTask.CompletedTask

    member this.Seeder: TestDatabaseSeeder =
        state.Value.Seeder

    member this.WithConnectionAsync
        (callback: IDbConnection -> IDbTransaction -> CancellationToken -> Task<'a>)
        : Task<'a> =
        task {
            let cancellationToken =
                TestContext.Current.CancellationToken

            let connection = state.Value.Connection

            use transaction =
                connection.BeginTransaction()

            let! result = callback connection transaction cancellationToken
            do! transaction.CommitAsync()

            return result
        }

    interface IDisposable with
        member this.Dispose() : unit =
            match state with
            | None -> ()
            | Some state ->
                (state.Seeder :> IDisposable).Dispose()
                (state.Connection :> IDisposable).Dispose()