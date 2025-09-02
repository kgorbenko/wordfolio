namespace Wordfolio.Api.DataAccess.Tests

open System
open System.Threading.Tasks
open Npgsql
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

    [<Fact>]
    member _.``createUserAsync fails on duplicate Id`` () =
        task {
            do!
                Users.createUserAsync { Id = 5 }
                |> fixture.WithConnectionAsync

            let! ex =
                Assert.ThrowsAsync<PostgresException>(fun () ->
                    (Users.createUserAsync { Id = 5 }
                    |> fixture.WithConnectionAsync
                    :> Task)
                )

            Assert.Equal("23505", ex.SqlState)
        }