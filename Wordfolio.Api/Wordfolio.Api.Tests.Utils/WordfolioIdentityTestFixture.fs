namespace Wordfolio.Api.Tests.Utils

open System
open System.Threading
open System.Threading.Tasks

open Xunit.Sdk

open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio
open Wordfolio.Api.Tests.Utils.Identity

type WordfolioIdentityTestFixture(messageSink: IMessageSink) =
    inherit BaseDatabaseTestFixture(messageSink)

    let mutable state
        : {| WordfolioSeeder: WordfolioSeeder
             IdentitySeeder: IdentitySeeder |} option =
        None

    override this.RunMigrations() =
        Identity.MigrationRunner.run this.ConnectionString
        Wordfolio.MigrationRunner.run this.ConnectionString

    member private this.EnsureInitialized() =
        match state with
        | None ->
            let connection = this.Connection

            state <-
                Some
                    {| WordfolioSeeder = Wordfolio.Seeder.create connection
                       IdentitySeeder = Identity.Seeder.create connection |}
        | Some _ -> ()

    override this.InitializeAsync() : ValueTask =
        do base.InitializeAsync().GetAwaiter().GetResult()
        this.EnsureInitialized()
        ValueTask.CompletedTask

    member this.WordfolioSeeder: WordfolioSeeder =
        state.Value.WordfolioSeeder

    member this.IdentitySeeder: IdentitySeeder =
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
