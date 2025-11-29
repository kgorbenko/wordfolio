namespace Wordfolio.Api.DataAccess.Tests

open System

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type CollectionsTests(fixture: WordfolioTestFixture) =
    let assertCollectionEquivalent (expected: CollectionEntity) (actual: CollectionEntity) =
        Assert.Equal(expected.Id, actual.Id)
        Assert.Equal(expected.UserId, actual.UserId)
        Assert.Equal(expected.Name, actual.Name)
        Assert.Equal(expected.Description, actual.Description)
        Assert.Equal(expected.CreatedAt, actual.CreatedAt)
        Assert.Equal(expected.UpdatedAt, actual.UpdatedAt)

    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``createCollectionAsync inserts a collection``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.FromHours(0.0))

            let user: UserEntity =
                { Id = 100
                  Collections = ResizeArray() }

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            do!
                Collections.createCollectionAsync
                    { UserId = user.Id
                      Name = "My Collection"
                      Description = Some "Test collection"
                      CreatedAt = createdAt }
                |> fixture.WithConnectionAsync

            let! actual =
                fixture.Seeder
                |> Seeder.getAllCollectionsAsync

            Assert.Single(actual) |> ignore

            let expected: CollectionEntity =
                { Id = actual.[0].Id
                  UserId = user.Id
                  Name = "My Collection"
                  Description = "Test collection"
                  CreatedAt = createdAt
                  UpdatedAt = Nullable()
                  User = Unchecked.defaultof<UserEntity>
                  Vocabularies = Unchecked.defaultof<ResizeArray<VocabularyEntity>> }

            assertCollectionEquivalent expected actual.[0]
        }

    [<Fact>]
    member _.``getCollectionByIdAsync returns collection when it exists``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.FromHours(0.0))

            let user: UserEntity =
                { Id = 100
                  Collections = ResizeArray() }

            let collection: CollectionEntity =
                { Id = 0
                  UserId = user.Id
                  Name = "My Collection"
                  Description = "Test collection"
                  CreatedAt = createdAt
                  UpdatedAt = Nullable()
                  User = Unchecked.defaultof<UserEntity>
                  Vocabularies = ResizeArray() }

            user.Collections.Add(collection)

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Collections.getCollectionByIdAsync collection.Id
                |> fixture.WithConnectionAsync

            let expected: Collections.Collection =
                { Id = collection.Id
                  UserId = user.Id
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

            let user1: UserEntity =
                { Id = 100
                  Collections = ResizeArray() }

            let user2: UserEntity =
                { Id = 101
                  Collections = ResizeArray() }

            let collection1: CollectionEntity =
                { Id = 0
                  UserId = user1.Id
                  Name = "Collection 1"
                  Description = null
                  CreatedAt = createdAt
                  UpdatedAt = Nullable()
                  User = Unchecked.defaultof<UserEntity>
                  Vocabularies = ResizeArray() }

            let collection2: CollectionEntity =
                { Id = 0
                  UserId = user1.Id
                  Name = "Collection 2"
                  Description = null
                  CreatedAt = createdAt
                  UpdatedAt = Nullable()
                  User = Unchecked.defaultof<UserEntity>
                  Vocabularies = ResizeArray() }

            let collection3: CollectionEntity =
                { Id = 0
                  UserId = user2.Id
                  Name = "Collection 3"
                  Description = null
                  CreatedAt = createdAt
                  UpdatedAt = Nullable()
                  User = Unchecked.defaultof<UserEntity>
                  Vocabularies = ResizeArray() }

            user1.Collections.AddRange([ collection1; collection2 ])
            user2.Collections.Add(collection3)

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user1; user2 ]
                |> Seeder.saveChangesAsync

            let! actual =
                Collections.getCollectionsByUserIdAsync user1.Id
                |> fixture.WithConnectionAsync

            let expected: Collections.Collection list =
                [ { Id = collection1.Id
                    UserId = user1.Id
                    Name = "Collection 1"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None }
                  { Id = collection2.Id
                    UserId = user1.Id
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

            let user: UserEntity =
                { Id = 100
                  Collections = ResizeArray() }

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Collections.getCollectionsByUserIdAsync user.Id
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

            let user: UserEntity =
                { Id = 101
                  Collections = ResizeArray() }

            let collection: CollectionEntity =
                { Id = 0
                  UserId = user.Id
                  Name = "Original Name"
                  Description = "Original Description"
                  CreatedAt = createdAt
                  UpdatedAt = Nullable()
                  User = Unchecked.defaultof<UserEntity>
                  Vocabularies = ResizeArray() }

            user.Collections.Add(collection)

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! affectedRows =
                Collections.updateCollectionAsync
                    { Id = collection.Id
                      Name = "Updated Name"
                      Description = Some "Updated Description"
                      UpdatedAt = updatedAt }
                |> fixture.WithConnectionAsync

            Assert.Equal(1, affectedRows)

            let! actual =
                fixture.Seeder
                |> Seeder.getAllCollectionsAsync

            Assert.Single(actual) |> ignore

            let expected: CollectionEntity =
                { Id = collection.Id
                  UserId = user.Id
                  Name = "Updated Name"
                  Description = "Updated Description"
                  CreatedAt = createdAt
                  UpdatedAt = Nullable(updatedAt)
                  User = Unchecked.defaultof<UserEntity>
                  Vocabularies = Unchecked.defaultof<ResizeArray<VocabularyEntity>> }

            assertCollectionEquivalent expected actual.[0]
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

            let user: UserEntity =
                { Id = 102
                  Collections = ResizeArray() }

            let collection: CollectionEntity =
                { Id = 0
                  UserId = user.Id
                  Name = "Collection to delete"
                  Description = null
                  CreatedAt = createdAt
                  UpdatedAt = Nullable()
                  User = Unchecked.defaultof<UserEntity>
                  Vocabularies = ResizeArray() }

            user.Collections.Add(collection)

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
