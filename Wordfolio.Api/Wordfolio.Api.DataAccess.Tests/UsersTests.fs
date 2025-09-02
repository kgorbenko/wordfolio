namespace Wordfolio.Api.DataAccess.Tests

open System.Data
open System.Data.Common
open System.Linq
open System.Threading

open Dapper.FSharp.PostgreSQL
open Microsoft.EntityFrameworkCore
open Xunit

open Wordfolio.Api.DataAccess

 type UsersTests(fixture: BaseDatabaseTestFixture) =
  interface IClassFixture<BaseDatabaseTestFixture>

  [<Fact>]
  member _.``createUserAsync inserts a row`` () =
    fixture.WithConnectionAsync(fun (connection: IDbConnection) (transaction: IDbTransaction) (cancellationToken: CancellationToken) -> task {
      OptionTypes.register()
      do! Users.createUserAsync { Users.UserCreationParameters.Id = 123 } connection transaction cancellationToken

      use context: TestDbContext = createContext (connection :?> DbConnection)

      let! count =
        context.Users.Where(fun u -> u.Id = 123).CountAsync(cancellationToken)

      Assert.Equal(1, count)
    })
