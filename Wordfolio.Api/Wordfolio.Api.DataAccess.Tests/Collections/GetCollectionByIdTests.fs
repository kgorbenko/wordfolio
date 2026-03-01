namespace Wordfolio.Api.DataAccess.Tests

open System

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type GetCollectionByIdTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``getCollectionByIdAsync returns collection when it exists``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let collection =
                Entities.makeCollection user "My Collection" (Some "Test collection") createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Collections.getCollectionByIdAsync collection.Id
                |> fixture.WithConnectionAsync

            let expected: Collections.Collection option =
                Some
                    { Id = collection.Id
                      UserId = user.Id
                      Name = "My Collection"
                      Description = Some "Test collection"
                      CreatedAt = createdAt
                      UpdatedAt = None }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``getCollectionByIdAsync returns None when collection does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! actual =
                Collections.getCollectionByIdAsync 999
                |> fixture.WithConnectionAsync

            Assert.Equal(None, actual)
        }

    [<Fact>]
    member _.``getCollectionByIdAsync returns None when collection is system``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let systemCollection =
                Entities.makeCollection user "Unsorted" None createdAt None true

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ systemCollection ]
                |> Seeder.saveChangesAsync

            let! actual =
                Collections.getCollectionByIdAsync systemCollection.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(None, actual)
        }
