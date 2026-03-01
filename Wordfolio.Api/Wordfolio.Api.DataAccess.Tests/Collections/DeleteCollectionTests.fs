namespace Wordfolio.Api.DataAccess.Tests.Collections

open System

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type DeleteCollectionTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``deleteCollectionAsync deletes an existing collection``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 102

            let collection =
                Entities.makeCollection user "Collection to delete" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! affectedRows =
                Collections.deleteCollectionAsync collection.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(1, affectedRows)

            let! actual =
                fixture.Seeder
                |> Seeder.getAllCollectionsAsync

            Assert.Empty(actual)
        }

    [<Fact>]
    member _.``deleteCollectionAsync returns 0 when collection does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! affectedRows =
                Collections.deleteCollectionAsync 999
                |> fixture.WithConnectionAsync

            Assert.Equal(0, affectedRows)
        }

    [<Fact>]
    member _.``deleteCollectionAsync returns 0 when collection is system``() =
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

            let! affectedRows =
                Collections.deleteCollectionAsync systemCollection.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(0, affectedRows)

            let! actual = Seeder.getCollectionByIdAsync systemCollection.Id fixture.Seeder

            Assert.True(actual.IsSome)
        }
