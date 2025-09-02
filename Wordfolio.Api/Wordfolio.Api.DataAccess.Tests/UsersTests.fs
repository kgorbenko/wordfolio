namespace Wordfolio.Api.DataAccess.Tests

open Xunit

open Wordfolio.Api.DataAccess

 type UsersTests(fixture: BaseDatabaseTestFixture) =
  interface IClassFixture<BaseDatabaseTestFixture>

  [<Fact>]
  member _.``createUserAsync inserts a row`` () =
      task {
          use connection = fixture.CreateConnection()

          do!
              Users.createUserAsync { Users.UserCreationParameters.Id = 123 }
              |> fixture.WithConnectionAsync

          use seeder = DatabaseSeeder.create connection

          let! actual = seeder |> DatabaseSeeder.getAllUsersAsync

          let expected: UserEntity list =
              [ { Id = 123 } ]

          Assert.Equivalent(expected, actual)
      }