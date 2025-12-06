namespace Wordfolio.Api.DataAccess.Tests

open System.Threading.Tasks

open Npgsql
open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type UsersTests(fixture: WordfolioTestFixture) =
    let UniqueViolationErrorCode = "23505"

    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``createUserAsync inserts a row``() =
        task {
            do! fixture.ResetDatabaseAsync()

            do!
                Users.createUserAsync { Id = 123 }
                |> fixture.WithConnectionAsync

            let! actual = fixture.Seeder |> Seeder.getAllUsersAsync

            let expected: User list = [ { Id = 123 } ]

            Assert.Equivalent(expected, actual)
        }

    [<Fact>]
    member _.``createUserAsync fails on duplicate Id``() =
        task {
            do! fixture.ResetDatabaseAsync()

            do!
                Users.createUserAsync { Id = 5 }
                |> fixture.WithConnectionAsync

            let! ex =
                Assert.ThrowsAsync<PostgresException>(fun () ->
                    (Users.createUserAsync { Id = 5 }
                     |> fixture.WithConnectionAsync
                    :> Task))

            Assert.Equal(UniqueViolationErrorCode, ex.SqlState)
        }
