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

            do!
                Collections.createCollectionAsync
                    { UserId = 100
                      Name = "My Collection"
                      Description = Some "Test collection"
                      CreatedAt = createdAt }
                |> fixture.WithConnectionAsync

            let! actual =
                fixture.Seeder
                |> Seeder.getAllCollectionsAsync

            Assert.Single(actual) |> ignore

            let collection = actual.[0]
            Assert.Equal(100, collection.UserId)
            Assert.Equal("My Collection", collection.Name)
            Assert.Equal("Test collection", collection.Description)
            Assert.Equal(createdAt, collection.CreatedAt)
            Assert.False(collection.UpdatedAt.HasValue)
        }

    [<Fact>]
    member _.``getCollectionByIdAsync returns collection when it exists``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.FromHours(0.0))

            do!
                fixture.Seeder
                |> Seeder.addUsers [ { Id = 100 } ]
                |> Seeder.addCollections
                    [ { Id = 0
                        UserId = 100
                        Name = "My Collection"
                        Description = "Test collection"
                        CreatedAt = createdAt
                        UpdatedAt = Nullable() } ]
                |> Seeder.saveChangesAsync

            let! allCollections =
                fixture.Seeder
                |> Seeder.getAllCollectionsAsync

            let collectionId = allCollections.[0].Id

            let! actual =
                Collections.getCollectionByIdAsync collectionId
                |> fixture.WithConnectionAsync

            Assert.True(actual.IsSome)

            let collection = actual.Value
            Assert.Equal(collectionId, collection.Id)
            Assert.Equal(100, collection.UserId)
            Assert.Equal("My Collection", collection.Name)
            Assert.Equal(Some "Test collection", collection.Description)
            Assert.Equal(createdAt, collection.CreatedAt)
            Assert.True(collection.UpdatedAt.IsNone)
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
    member _.``getCollectionsByUserIdAsync returns collections for user``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.FromHours(0.0))

            do!
                fixture.Seeder
                |> Seeder.addUsers [ { Id = 100 }; { Id = 101 } ]
                |> Seeder.addCollections
                    [ { Id = 0
                        UserId = 100
                        Name = "Collection 1"
                        Description = null
                        CreatedAt = createdAt
                        UpdatedAt = Nullable() }
                      { Id = 0
                        UserId = 100
                        Name = "Collection 2"
                        Description = null
                        CreatedAt = createdAt
                        UpdatedAt = Nullable() }
                      { Id = 0
                        UserId = 101
                        Name = "Collection 3"
                        Description = null
                        CreatedAt = createdAt
                        UpdatedAt = Nullable() } ]
                |> Seeder.saveChangesAsync

            let! actual =
                Collections.getCollectionsByUserIdAsync 100
                |> fixture.WithConnectionAsync

            Assert.Equal(2, actual.Length)

            Assert.True(
                actual
                |> List.forall(fun c -> c.UserId = 100)
            )
        }

    [<Fact>]
    member _.``getCollectionsByUserIdAsync returns empty list when user has no collections``() =
        task {
            do! fixture.ResetDatabaseAsync()

            do!
                fixture.Seeder
                |> Seeder.addUsers [ { Id = 100 } ]
                |> Seeder.saveChangesAsync

            let! actual =
                Collections.getCollectionsByUserIdAsync 100
                |> fixture.WithConnectionAsync

            Assert.Empty(actual)
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
                |> Seeder.addCollections
                    [ { Id = 0
                        UserId = 101
                        Name = "Original Name"
                        Description = "Original Description"
                        CreatedAt = createdAt
                        UpdatedAt = Nullable() } ]
                |> Seeder.saveChangesAsync

            let! allCollections =
                fixture.Seeder
                |> Seeder.getAllCollectionsAsync

            let collectionId = allCollections.[0].Id

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
                    UpdatedAt = Nullable(updatedAt) } ]

            Assert.Equivalent(expected, actual)
        }

    [<Fact>]
    member _.``updateCollectionAsync returns 0 when collection does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let updatedAt =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.FromHours(0.0))

            let! affectedRows =
                Collections.updateCollectionAsync
                    { Id = 999
                      Name = "Updated Name"
                      Description = Some "Updated Description"
                      UpdatedAt = updatedAt }
                |> fixture.WithConnectionAsync

            Assert.Equal(0, affectedRows)
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
                |> Seeder.addCollections
                    [ { Id = 0
                        UserId = 102
                        Name = "Collection to delete"
                        Description = null
                        CreatedAt = createdAt
                        UpdatedAt = Nullable() } ]
                |> Seeder.saveChangesAsync

            let! allCollections =
                fixture.Seeder
                |> Seeder.getAllCollectionsAsync

            let collectionId = allCollections.[0].Id

            let! affectedRows =
                Collections.deleteCollectionAsync collectionId
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
