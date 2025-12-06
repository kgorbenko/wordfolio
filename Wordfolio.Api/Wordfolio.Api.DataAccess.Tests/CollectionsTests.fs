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

            let! _ =
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

            let! actual = fixture.Seeder |> Seeder.getAllCollectionsAsync

            Assert.Single(actual) |> ignore

            let expected: Collection =
                { Id = actual.[0].Id
                  UserId = 100
                  Name = "My Collection"
                  Description = Some "Test collection"
                  CreatedAt = createdAt
                  UpdatedAt = None }

            Assert.Equivalent(expected, actual.[0])
        }

    [<Fact>]
    member _.``getCollectionByIdAsync returns collection when it exists``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.FromHours(0.0))

            let! seeded =
                fixture.Seeder
                |> Seeder.addUsers [ { Id = 100 } ]
                |> Seeder.addCollections
                    [ { UserId = 100
                        Name = "My Collection"
                        Description = Some "Test collection"
                        CreatedAt = createdAt
                        UpdatedAt = None
                        Vocabularies = [] } ]
                |> Seeder.saveChangesAsync

            let collection = seeded.Collections.[0]

            let! actual =
                Collections.getCollectionByIdAsync collection.Id
                |> fixture.WithConnectionAsync

            let expected: Collections.Collection =
                { Id = collection.Id
                  UserId = 100
                  Name = "My Collection"
                  Description = Some "Test collection"
                  CreatedAt = createdAt
                  UpdatedAt = None }

            Assert.Equivalent(Some expected, actual)
        }

    [<Fact>]
    member _.``getCollectionByIdAsync returns None when collection does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! actual =
                Collections.getCollectionByIdAsync 999
                |> fixture.WithConnectionAsync

            Assert.True(actual.IsNone)
        }

    [<Fact>]
    member _.``getCollectionsByUserIdAsync returns collections for user``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.FromHours(0.0))

            let! seeded =
                fixture.Seeder
                |> Seeder.addUsers [ { Id = 100 }; { Id = 101 } ]
                |> Seeder.addCollections
                    [ { UserId = 100
                        Name = "Collection 1"
                        Description = None
                        CreatedAt = createdAt
                        UpdatedAt = None
                        Vocabularies = [] }
                      { UserId = 100
                        Name = "Collection 2"
                        Description = None
                        CreatedAt = createdAt
                        UpdatedAt = None
                        Vocabularies = [] }
                      { UserId = 101
                        Name = "Collection 3"
                        Description = None
                        CreatedAt = createdAt
                        UpdatedAt = None
                        Vocabularies = [] } ]
                |> Seeder.saveChangesAsync

            let! actual =
                Collections.getCollectionsByUserIdAsync 100
                |> fixture.WithConnectionAsync

            let expected: Collections.Collection list =
                [ { Id = seeded.Collections.[0].Id
                    UserId = 100
                    Name = "Collection 1"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None }
                  { Id = seeded.Collections.[1].Id
                    UserId = 100
                    Name = "Collection 2"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None } ]

            Assert.Equivalent(expected, actual)
        }

    [<Fact>]
    member _.``getCollectionsByUserIdAsync returns empty list when user has no collections``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! _ =
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

            let! seeded =
                fixture.Seeder
                |> Seeder.addUsers [ { Id = 101 } ]
                |> Seeder.addCollections
                    [ { UserId = 101
                        Name = "Original Name"
                        Description = Some "Original Description"
                        CreatedAt = createdAt
                        UpdatedAt = None
                        Vocabularies = [] } ]
                |> Seeder.saveChangesAsync

            let collection = seeded.Collections.[0]

            let! affectedRows =
                Collections.updateCollectionAsync
                    { Id = collection.Id
                      Name = "Updated Name"
                      Description = Some "Updated Description"
                      UpdatedAt = updatedAt }
                |> fixture.WithConnectionAsync

            Assert.Equal(1, affectedRows)

            let! actual = fixture.Seeder |> Seeder.getAllCollectionsAsync

            Assert.Single(actual) |> ignore

            let expected: Collection =
                { Id = collection.Id
                  UserId = 101
                  Name = "Updated Name"
                  Description = Some "Updated Description"
                  CreatedAt = createdAt
                  UpdatedAt = Some updatedAt }

            Assert.Equivalent(expected, actual.[0])
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

            let! seeded =
                fixture.Seeder
                |> Seeder.addUsers [ { Id = 102 } ]
                |> Seeder.addCollections
                    [ { UserId = 102
                        Name = "Collection to delete"
                        Description = None
                        CreatedAt = createdAt
                        UpdatedAt = None
                        Vocabularies = [] } ]
                |> Seeder.saveChangesAsync

            let collection = seeded.Collections.[0]

            let! affectedRows =
                Collections.deleteCollectionAsync collection.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(1, affectedRows)

            let! actual = fixture.Seeder |> Seeder.getAllCollectionsAsync

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
