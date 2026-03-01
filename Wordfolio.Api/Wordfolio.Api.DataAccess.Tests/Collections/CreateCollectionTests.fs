namespace Wordfolio.Api.DataAccess.Tests

open System
open System.Threading.Tasks

open Npgsql
open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type CreateCollectionTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``createCollectionAsync inserts a collection``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! createdId =
                Collections.createCollectionAsync
                    { UserId = user.Id
                      Name = "My Collection"
                      Description = Some "Test collection"
                      CreatedAt = createdAt }
                |> fixture.WithConnectionAsync

            let! actualCollection =
                fixture.Seeder
                |> Seeder.getCollectionByIdAsync createdId

            let expected: Collection option =
                Some
                    { Id = createdId
                      UserId = user.Id
                      Name = "My Collection"
                      Description = Some "Test collection"
                      CreatedAt = createdAt
                      UpdatedAt = None
                      IsSystem = false }

            Assert.Equivalent(expected, actualCollection)
        }

    [<Fact>]
    member _.``createCollectionAsync inserts a collection with None description``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 103

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! createdId =
                Collections.createCollectionAsync
                    { UserId = user.Id
                      Name = "My Collection"
                      Description = None
                      CreatedAt = createdAt }
                |> fixture.WithConnectionAsync

            let! actualCollection =
                fixture.Seeder
                |> Seeder.getCollectionByIdAsync createdId

            let expected: Collection option =
                Some
                    { Id = createdId
                      UserId = user.Id
                      Name = "My Collection"
                      Description = None
                      CreatedAt = createdAt
                      UpdatedAt = None
                      IsSystem = false }

            Assert.Equivalent(expected, actualCollection)
        }

    [<Fact>]
    member _.``createCollectionAsync fails with foreign key violation for non-existent user``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let! ex =
                Assert.ThrowsAsync<PostgresException>(fun () ->
                    (Collections.createCollectionAsync
                        { UserId = 999
                          Name = "My Collection"
                          Description = Some "Test collection"
                          CreatedAt = createdAt }
                     |> fixture.WithConnectionAsync
                    :> Task))

            Assert.Equal(SqlErrorCodes.ForeignKeyViolation, ex.SqlState)
        }
