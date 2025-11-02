namespace Wordfolio.Api.Tests.Utils

open System
open System.Threading
open System.Threading.Tasks

open Xunit.Sdk

open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type WordfolioTestFixture(messageSink: IMessageSink) =
    inherit BaseDatabaseTestFixture(messageSink)

    let mutable state: {| Seeder: WordfolioSeeder |} option =
        None

    override this.RunMigrations() =
        MigrationRunner.run this.ConnectionString

    member private this.EnsureInitialized() =
        match state with
        | None ->
            let seeder =
                Seeder.create this.Connection

            state <- Some {| Seeder = seeder |}
        | Some _ -> ()

    override this.InitializeAsync() : ValueTask =
        do base.InitializeAsync().GetAwaiter().GetResult()
        this.EnsureInitialized()
        ValueTask.CompletedTask

    member this.Seeder: WordfolioSeeder =
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
