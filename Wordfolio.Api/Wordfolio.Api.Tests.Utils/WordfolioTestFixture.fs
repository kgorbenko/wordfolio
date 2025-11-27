namespace Wordfolio.Api.Tests.Utils

open System
open System.Threading
open System.Threading.Tasks

open Xunit.Sdk

open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type WordfolioTestFixture(messageSink: IMessageSink) as this =
    inherit BaseDatabaseTestFixture(messageSink)

    let mutable seeder: WordfolioSeeder option =
        None

    override _.RunMigrations(connectionString: string) = MigrationRunner.run connectionString

    member private _.EnsureInitialized() =
        match seeder with
        | None -> seeder <- Some(Seeder.create this.Connection)
        | Some _ -> ()

    member private _.RecreateSeeder() =
        match seeder with
        | Some existingSeeder ->
            (existingSeeder :> IDisposable).Dispose()
            seeder <- Some(Seeder.create this.Connection)
        | None -> ()

    override this.InitializeAsync() : ValueTask =
        do base.InitializeAsync().GetAwaiter().GetResult()
        this.EnsureInitialized()
        ValueTask.CompletedTask

    member _.Seeder: WordfolioSeeder = seeder.Value

    member this.ResetDatabaseAsync() : Task =
        let baseReset =
            (this :> BaseDatabaseTestFixture).ResetDatabaseAsync

        task {
            do! baseReset()
            this.RecreateSeeder()
        }

    member this.WithConnectionAsync
        (callback: System.Data.IDbConnection -> System.Data.IDbTransaction -> CancellationToken -> Task<'a>)
        : Task<'a> =
        (this :> BaseDatabaseTestFixture).WithConnectionAsync callback

    interface IDisposable with
        member _.Dispose() : unit =
            match seeder with
            | None -> ()
            | Some s -> (s :> IDisposable).Dispose()
