namespace Wordfolio.Api.Tests.Utils

open System
open System.Data
open System.Data.Common
open System.Threading
open System.Threading.Tasks

open Dapper.FSharp.MSSQL
open Npgsql
open Testcontainers.PostgreSql
open Testcontainers.Xunit
open Xunit
open Xunit.Sdk

[<AbstractClass>]
type BaseDatabaseTestFixture(messageSink: IMessageSink) =
    inherit DbContainerFixture<PostgreSqlBuilder, PostgreSqlContainer>(messageSink)

    let mutable state
        : {| Connection: DbConnection
             ConnectionString: string |} option =
        None

    abstract member RunMigrations: unit -> unit

    member private this.EnsureInitialized() =
        match state with
        | None ->
            OptionTypes.register()

            let connectionString =
                this.Container.GetConnectionString()

            let connection = this.CreateConnection()
            connection.Open()

            state <-
                Some
                    {| Connection = connection
                       ConnectionString = connectionString |}

            this.RunMigrations()
        | Some _ -> ()

    override this.Configure(builder: PostgreSqlBuilder) : PostgreSqlBuilder =
        base.Configure(builder).WithImage("postgres:17.5")

    override _.DbProviderFactory: DbProviderFactory =
        NpgsqlFactory.Instance

    override this.InitializeAsync() : ValueTask =
        do base.InitializeAsync().GetAwaiter().GetResult()
        this.EnsureInitialized()
        ValueTask.CompletedTask

    member this.Connection: DbConnection =
        state.Value.Connection

    member this.ConnectionString: string =
        state.Value.ConnectionString

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
            | Some state -> (state.Connection :> IDisposable).Dispose()
