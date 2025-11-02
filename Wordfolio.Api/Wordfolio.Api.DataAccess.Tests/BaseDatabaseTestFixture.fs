namespace Wordfolio.Api.DataAccess.Tests

open System
open System.Threading
open System.Threading.Tasks

open FluentMigrator.Runner
open Microsoft.Extensions.DependencyInjection
open Xunit
open Xunit.Sdk

open Wordfolio.Api.Migrations
open Wordfolio.Api.Tests.Utils

type BaseDatabaseTestFixture(messageSink: IMessageSink) =
    inherit BasePostgreSqlFixture(messageSink)

    let mutable state: {| Seeder: TestDatabaseSeeder |} option =
        None

    override this.RunMigrations() =
        SchemaMigrations.runWordfolioMigrations this.ConnectionString

    member private this.EnsureInitialized() =
        match state with
        | None ->
            let seeder =
                DatabaseSeeder.create this.Connection

            state <- Some {| Seeder = seeder |}
        | Some _ -> ()

    override this.InitializeAsync() : ValueTask =
        do base.InitializeAsync().GetAwaiter().GetResult()
        this.EnsureInitialized()
        ValueTask.CompletedTask

    member this.Seeder: TestDatabaseSeeder =
        state.Value.Seeder

    member this.WithConnectionAsync
        (callback: System.Data.IDbConnection -> System.Data.IDbTransaction -> CancellationToken -> Task<'a>)
        : Task<'a> =
        base.WithConnectionAsync callback

    interface IDisposable with
        member this.Dispose() : unit =
            match state with
            | None -> ()
            | Some state -> (state.Seeder :> IDisposable).Dispose()
