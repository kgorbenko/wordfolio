namespace Wordfolio.Api.DataAccess.Tests

open System
open System.Data
open System.Threading

open Dapper.FSharp.PostgreSQL
open Npgsql
open Xunit

open Wordfolio.Api.DataAccess

type UsersTests(fixture: BaseDatabaseTestFixture) =
  interface IClassFixture<BaseDatabaseTestFixture>

  [<Fact>]
  member _.``createUserAsync inserts a row`` () =
    fixture.WithConnectionAsync(fun (connection: IDbConnection) (transaction: IDbTransaction) (cancellationToken: CancellationToken) -> task {
      OptionTypes.register()
      do! Users.createUserAsync { Users.UserCreationParameters.Id = 123 } connection transaction cancellationToken
      use command = new NpgsqlCommand("SELECT COUNT(*) FROM wordfolio.\"Users\" WHERE \"Id\" = 123", connection :?> NpgsqlConnection, transaction :?> NpgsqlTransaction)
      let! countObject = command.ExecuteScalarAsync(cancellationToken)
      let count = Convert.ToInt32(countObject)
      Assert.Equal(1, count)
    })
