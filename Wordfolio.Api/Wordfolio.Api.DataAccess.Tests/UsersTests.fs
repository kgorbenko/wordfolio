namespace Wordfolio.Api.DataAccess.Tests

open Xunit

open Wordfolio.Api.DataAccess

type UsersTests(fixture: BaseDatabaseTestFixture) =
  interface IClassFixture<BaseDatabaseTestFixture>

  [<Fact>]
  member _.``createUserAsync inserts a row`` () =
      task {
          do!
              Users.createUserAsync { Id = 123 }
              |> fixture.WithConnectionAsync

          let! actual = fixture.Seeder |> DatabaseSeeder.getAllUsersAsync

          let expected: UserEntity list =
              [ { Id = 123 } ]

          Assert.Equivalent(expected, actual)
      }