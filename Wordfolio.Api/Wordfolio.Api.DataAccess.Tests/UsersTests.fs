namespace Wordfolio.Api.DataAccess.Tests

open System
open Xunit
open Npgsql
open Dapper.FSharp.PostgreSQL
open Wordfolio.Common
open Wordfolio.Api.DataAccess

type UsersTests(fx: BaseDatabaseTestFixture) =
  interface IClassFixture<BaseDatabaseTestFixture>

  [<Fact>]
  member _.``createUserAsync inserts a row`` () = task {
    let ct = Xunit.TestContext.Current.CancellationToken
    OptionTypes.register()
    use c = fx.CreateConnection() :?> NpgsqlConnection
    do! c.OpenAsync(ct)
    use tx = c.BeginTransaction()
    do! (new NpgsqlCommand("CREATE SCHEMA IF NOT EXISTS wordfolio;", c, tx)).ExecuteNonQueryAsync(ct) |> Task.ignore
    do! (new NpgsqlCommand("CREATE TABLE IF NOT EXISTS wordfolio.\"Users\" (\n  \"Id\" INT PRIMARY KEY\n);", c, tx)).ExecuteNonQueryAsync(ct) |> Wordfolio.Common.Task.ignore
    do! Users.createUserAsync { Users.UserCreationParameters.Id = 123 } c tx ct
    use cmd = new NpgsqlCommand("SELECT COUNT(*) FROM wordfolio.\"Users\" WHERE \"Id\" = 123", c, tx)
    let! nObj = cmd.ExecuteScalarAsync(ct)
    let n = Convert.ToInt32(nObj)
    Assert.Equal(1, n)
  }
