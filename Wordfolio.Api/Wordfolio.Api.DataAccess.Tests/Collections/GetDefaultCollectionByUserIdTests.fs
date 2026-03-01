namespace Wordfolio.Api.DataAccess.Tests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type GetDefaultCollectionByUserIdTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``getDefaultCollectionByUserIdAsync returns system collection when it exists``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let systemCollection =
                Entities.makeCollection user "Unsorted" None createdAt None true

            let regularCollection =
                Entities.makeCollection user "Regular" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ systemCollection; regularCollection ]
                |> Seeder.saveChangesAsync

            let! actual =
                Collections.getDefaultCollectionByUserIdAsync user.Id
                |> fixture.WithConnectionAsync

            let expected: Collections.Collection option =
                Some
                    { Id = systemCollection.Id
                      UserId = user.Id
                      Name = "Unsorted"
                      Description = None
                      CreatedAt = createdAt
                      UpdatedAt = None }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``getDefaultCollectionByUserIdAsync returns None when no system collection exists``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let regularCollection =
                Entities.makeCollection user "Regular" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ regularCollection ]
                |> Seeder.saveChangesAsync

            let! actual =
                Collections.getDefaultCollectionByUserIdAsync user.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(None, actual)
        }

    [<Fact>]
    member _.``getDefaultCollectionByUserIdAsync throws when multiple system collections exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let systemCollection1 =
                Entities.makeCollection user "Unsorted 1" None createdAt None true

            let systemCollection2 =
                Entities.makeCollection user "Unsorted 2" None createdAt None true

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ systemCollection1; systemCollection2 ]
                |> Seeder.saveChangesAsync

            let! ex =
                Assert.ThrowsAsync<System.Exception>(fun () ->
                    Collections.getDefaultCollectionByUserIdAsync user.Id
                    |> fixture.WithConnectionAsync
                    :> Task)

            Assert.Equal("Query returned more than one element when at most one was expected", ex.Message)
        }
