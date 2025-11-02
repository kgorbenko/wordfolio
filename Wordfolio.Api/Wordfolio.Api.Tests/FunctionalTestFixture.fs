namespace Wordfolio.Api.Tests

open System
open System.Threading
open System.Threading.Tasks

open Xunit.Sdk

open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

[<Sealed>]
type FunctionalTestFixture(messageSink: IMessageSink) =
    inherit BasePostgreSqlFixture(messageSink)

    let mutable state
        : {| WordfolioSeeder: WordfolioSeeder
             IdentitySeeder: TestIdentityDatabaseSeeder |} option =
        None

    override this.RunMigrations() =
        SchemaMigrations.runIdentityMigrations this.ConnectionString
        SchemaMigrations.runWordfolioMigrations this.ConnectionString

    member private this.EnsureInitialized() =
        match state with
        | None ->
            let connection = this.Connection

            state <-
                Some
                    {| WordfolioSeeder = Seeder.create connection
                       IdentitySeeder = IdentityDatabaseSeeder.create connection |}
        | Some _ -> ()

    override this.InitializeAsync() : ValueTask =
        do base.InitializeAsync().GetAwaiter().GetResult()
        this.EnsureInitialized()
        ValueTask.CompletedTask

    member this.WordfolioSeeder: WordfolioSeeder =
        state.Value.WordfolioSeeder

    member this.IdentitySeeder: TestIdentityDatabaseSeeder =
        state.Value.IdentitySeeder

    member this.WithConnectionAsync
        (callback: System.Data.IDbConnection -> System.Data.IDbTransaction -> CancellationToken -> Task<'a>)
        : Task<'a> =
        base.WithConnectionAsync callback

    interface IDisposable with
        member this.Dispose() : unit =
            match state with
            | None -> ()
            | Some state ->
                (state.WordfolioSeeder :> IDisposable).Dispose()
                (state.IdentitySeeder :> IDisposable).Dispose()
