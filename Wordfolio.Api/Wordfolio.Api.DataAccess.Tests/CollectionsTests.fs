namespace Wordfolio.Api.DataAccess.Tests

open System

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type CollectionsTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``createCollectionAsync inserts a collection``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.FromHours(0.0))

            do!
                fixture.Seeder
                |> Seeder.addUsers [ { Id = 100 } ]
                |> Seeder.saveChangesAsync

            let collectionId = 1

            do!
                Collections.createCollectionAsync
                    { Id = collectionId
                      UserId = 100
                      Name = "My Collection"
                      Description = Some "Test collection"
                      CreatedAt = createdAt }
                |> fixture.WithConnectionAsync

            let! actual =
                fixture.Seeder
                |> Seeder.getAllCollectionsAsync

            let expected: CollectionEntity list =
                [ { Id = collectionId
                    UserId = 100
                    Name = "My Collection"
                    Description = "Test collection"
                    CreatedAt = createdAt
                    UpdatedAt = createdAt } ]

            Assert.Equivalent(expected, actual)
        }

    [<Fact>]
    member _.``getCollectionByIdAsync returns None when collection does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! collection =
                Collections.getCollectionByIdAsync 999
                |> fixture.WithConnectionAsync

            Assert.True(collection.IsNone)
        }

    [<Fact>]
    member _.``updateCollectionAsync updates an existing collection``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.FromHours(0.0))

            let updatedAt =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.FromHours(0.0))

            do!
                fixture.Seeder
                |> Seeder.addUsers [ { Id = 101 } ]
                |> Seeder.saveChangesAsync

            let collectionId = 2

            do!
                Collections.createCollectionAsync
                    { Id = collectionId
                      UserId = 101
                      Name = "Original Name"
                      Description = Some "Original Description"
                      CreatedAt = createdAt }
                |> fixture.WithConnectionAsync

            let! affectedRows =
                Collections.updateCollectionAsync
                    { Id = collectionId
                      Name = "Updated Name"
                      Description = Some "Updated Description"
                      UpdatedAt = updatedAt }
                |> fixture.WithConnectionAsync

            Assert.Equal(1, affectedRows)

            let! actual =
                fixture.Seeder
                |> Seeder.getAllCollectionsAsync

            let expected: CollectionEntity list =
                [ { Id = collectionId
                    UserId = 101
                    Name = "Updated Name"
                    Description = "Updated Description"
                    CreatedAt = createdAt
                    UpdatedAt = updatedAt } ]

            Assert.Equivalent(expected, actual)
        }

    [<Fact>]
    member _.``deleteCollectionAsync deletes an existing collection``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.FromHours(0.0))

            do!
                fixture.Seeder
                |> Seeder.addUsers [ { Id = 102 } ]
                |> Seeder.saveChangesAsync

            let collectionId = 3

            do!
                Collections.createCollectionAsync
                    { Id = collectionId
                      UserId = 102
                      Name = "Collection to delete"
                      Description = None
                      CreatedAt = createdAt }
                |> fixture.WithConnectionAsync

            let! affectedRows =
                Collections.deleteCollectionAsync collectionId
                |> fixture.WithConnectionAsync

            Assert.Equal(1, affectedRows)

            let! actual =
                fixture.Seeder
                |> Seeder.getAllCollectionsAsync

            Assert.Empty(actual)
        }
