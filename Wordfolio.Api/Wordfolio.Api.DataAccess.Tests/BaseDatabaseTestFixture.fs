namespace Wordfolio.Api.DataAccess.Tests

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

  let mutable migrated = false

  override this.Configure (builder: PostgreSqlBuilder): PostgreSqlBuilder =
      base.Configure(builder).WithImage("postgres:17.5")

  override _.DbProviderFactory : DbProviderFactory = NpgsqlFactory.Instance

  override _.InitializeAsync(): ValueTask =
      OptionTypes.register()
      base.InitializeAsync()

  member private this.EnsureMigrated() =
    if not migrated then
      let connectionString = this.Container.GetConnectionString()
      let serviceProvider =
        ServiceCollection()
          .AddFluentMigratorCore()
          .ConfigureRunner(fun builder ->
            builder.AddPostgres()
                   .WithGlobalConnectionString(connectionString)
                   .ScanIn(typeof<CreateUsersTable>.Assembly).For.Migrations() |> ignore)
          .BuildServiceProvider()
      let scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope()
      let runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>()
      runner.MigrateUp()
      migrated <- true

  member this.WithConnectionAsync
    (callback: IDbConnection -> IDbTransaction -> CancellationToken -> Task<'a>)
    : Task<'a> =
    task {
      this.EnsureMigrated()

      let cancellationToken = TestContext.Current.CancellationToken

      use connection = this.CreateConnection()
      connection.Open()

      use transaction = connection.BeginTransaction()

      let! result = callback connection transaction cancellationToken

      do! transaction.CommitAsync()

      return result
    }