namespace Wordfolio.Api.DataAccess.Tests

open System.Data.Common

open Npgsql
open Testcontainers.PostgreSql
open Testcontainers.Xunit
open Xunit.Sdk

type BaseDatabaseTestFixture(messageSink: IMessageSink) =
  inherit DbContainerFixture<PostgreSqlBuilder, PostgreSqlContainer>(messageSink)
  override _.DbProviderFactory : DbProviderFactory = NpgsqlFactory.Instance