namespace Wordfolio.Api.Domain.Tests

open System
open System.Threading

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Collections
open Wordfolio.Api.Domain.Tests.TestHelpers

type CollectionsTests() =

    [<Fact>]
    member _.``getByIdAsync returns collection when user owns it``() =
        task {
            let repository = MockCollectionRepository()
            let userId = UserId 1
            let collectionId = CollectionId 1
            let collectionData = makeCollectionData 1 1 "Test Collection" (Some "Description")
            repository.AddCollection(collectionData)

            let! result =
                Collections.getByIdAsync repository userId collectionId CancellationToken.None

            match result with
            | Error _ -> Assert.Fail("Expected Ok result")
            | Ok collection ->
                Assert.Equal(collectionId, collection.Id)
                Assert.Equal(userId, collection.UserId)
                Assert.Equal("Test Collection", collection.Name)
                Assert.Equal(Some "Description", collection.Description)
        }

    [<Fact>]
    member _.``getByIdAsync returns CollectionNotFound when collection does not exist``() =
        task {
            let repository = MockCollectionRepository()
            let userId = UserId 1
            let collectionId = CollectionId 999

            let! result =
                Collections.getByIdAsync repository userId collectionId CancellationToken.None

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(CollectionNotFound(CollectionId 999), error)
        }

    [<Fact>]
    member _.``getByIdAsync returns CollectionAccessDenied when user does not own collection``() =
        task {
            let repository = MockCollectionRepository()
            let ownerId = UserId 1
            let otherUserId = UserId 2
            let collectionId = CollectionId 1
            let collectionData = makeCollectionData 1 1 "Test Collection" None
            repository.AddCollection(collectionData)

            let! result =
                Collections.getByIdAsync repository otherUserId collectionId CancellationToken.None

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(CollectionAccessDenied(CollectionId 1), error)
        }

    [<Fact>]
    member _.``getByUserIdAsync returns collections for user``() =
        task {
            let repository = MockCollectionRepository()
            let userId = UserId 1
            repository.AddCollection(makeCollectionData 1 1 "Collection 1" None)
            repository.AddCollection(makeCollectionData 2 1 "Collection 2" None)
            repository.AddCollection(makeCollectionData 3 2 "Other User Collection" None)

            let! collections =
                Collections.getByUserIdAsync repository userId CancellationToken.None

            Assert.Equal(2, collections.Length)
            Assert.True(collections |> List.exists(fun c -> c.Name = "Collection 1"))
            Assert.True(collections |> List.exists(fun c -> c.Name = "Collection 2"))
        }

    [<Fact>]
    member _.``getByUserIdAsync returns empty list when user has no collections``() =
        task {
            let repository = MockCollectionRepository()
            let userId = UserId 1

            let! collections =
                Collections.getByUserIdAsync repository userId CancellationToken.None

            Assert.Empty(collections)
        }

    [<Fact>]
    member _.``createAsync creates collection with valid data``() =
        task {
            let repository = MockCollectionRepository()
            let now = DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let command =
                { UserId = UserId 1
                  Name = "New Collection"
                  Description = Some "A new collection" }

            let! result =
                Collections.createAsync repository command now CancellationToken.None

            match result with
            | Error _ -> Assert.Fail("Expected Ok result")
            | Ok collection ->
                Assert.Equal("New Collection", collection.Name)
                Assert.Equal(Some "A new collection", collection.Description)
                Assert.Equal(UserId 1, collection.UserId)
                Assert.Equal(now, collection.CreatedAt)
                Assert.Equal(None, collection.UpdatedAt)
        }

    [<Fact>]
    member _.``createAsync returns error when name is empty``() =
        task {
            let repository = MockCollectionRepository()
            let now = DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let command =
                { UserId = UserId 1
                  Name = ""
                  Description = None }

            let! result =
                Collections.createAsync repository command now CancellationToken.None

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(CollectionNameRequired, error)
        }

    [<Fact>]
    member _.``createAsync returns error when name is whitespace only``() =
        task {
            let repository = MockCollectionRepository()
            let now = DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let command =
                { UserId = UserId 1
                  Name = "   "
                  Description = None }

            let! result =
                Collections.createAsync repository command now CancellationToken.None

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(CollectionNameRequired, error)
        }

    [<Fact>]
    member _.``createAsync returns error when name exceeds max length``() =
        task {
            let repository = MockCollectionRepository()
            let now = DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)
            let longName = String.replicate 256 "a"

            let command =
                { UserId = UserId 1
                  Name = longName
                  Description = None }

            let! result =
                Collections.createAsync repository command now CancellationToken.None

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(CollectionNameTooLong MaxNameLength, error)
        }

    [<Fact>]
    member _.``createAsync trims name whitespace``() =
        task {
            let repository = MockCollectionRepository()
            let now = DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let command =
                { UserId = UserId 1
                  Name = "  Trimmed Name  "
                  Description = None }

            let! result =
                Collections.createAsync repository command now CancellationToken.None

            match result with
            | Error _ -> Assert.Fail("Expected Ok result")
            | Ok collection -> Assert.Equal("Trimmed Name", collection.Name)
        }

    [<Fact>]
    member _.``updateAsync updates collection when user owns it``() =
        task {
            let repository = MockCollectionRepository()
            let now = DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)
            repository.AddCollection(makeCollectionData 1 1 "Original Name" (Some "Original"))

            let command =
                { CollectionId = CollectionId 1
                  UserId = UserId 1
                  Name = "Updated Name"
                  Description = Some "Updated Description" }

            let! result =
                Collections.updateAsync repository command now CancellationToken.None

            match result with
            | Error _ -> Assert.Fail("Expected Ok result")
            | Ok collection ->
                Assert.Equal("Updated Name", collection.Name)
                Assert.Equal(Some "Updated Description", collection.Description)
                Assert.Equal(Some now, collection.UpdatedAt)
        }

    [<Fact>]
    member _.``updateAsync returns CollectionNotFound when collection does not exist``() =
        task {
            let repository = MockCollectionRepository()
            let now = DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let command =
                { CollectionId = CollectionId 999
                  UserId = UserId 1
                  Name = "Updated Name"
                  Description = None }

            let! result =
                Collections.updateAsync repository command now CancellationToken.None

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(CollectionNotFound(CollectionId 999), error)
        }

    [<Fact>]
    member _.``updateAsync returns CollectionAccessDenied when user does not own collection``() =
        task {
            let repository = MockCollectionRepository()
            let now = DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)
            repository.AddCollection(makeCollectionData 1 1 "Original Name" None)

            let command =
                { CollectionId = CollectionId 1
                  UserId = UserId 2
                  Name = "Updated Name"
                  Description = None }

            let! result =
                Collections.updateAsync repository command now CancellationToken.None

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(CollectionAccessDenied(CollectionId 1), error)
        }

    [<Fact>]
    member _.``updateAsync returns error when name is empty``() =
        task {
            let repository = MockCollectionRepository()
            let now = DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)
            repository.AddCollection(makeCollectionData 1 1 "Original Name" None)

            let command =
                { CollectionId = CollectionId 1
                  UserId = UserId 1
                  Name = ""
                  Description = None }

            let! result =
                Collections.updateAsync repository command now CancellationToken.None

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(CollectionNameRequired, error)
        }

    [<Fact>]
    member _.``deleteAsync deletes collection when user owns it``() =
        task {
            let repository = MockCollectionRepository()
            repository.AddCollection(makeCollectionData 1 1 "Collection to delete" None)

            let command =
                { CollectionId = CollectionId 1
                  UserId = UserId 1 }

            let! result =
                Collections.deleteAsync repository command CancellationToken.None

            match result with
            | Error _ -> Assert.Fail("Expected Ok result")
            | Ok() -> Assert.Empty(repository.Collections)
        }

    [<Fact>]
    member _.``deleteAsync returns CollectionNotFound when collection does not exist``() =
        task {
            let repository = MockCollectionRepository()

            let command =
                { CollectionId = CollectionId 999
                  UserId = UserId 1 }

            let! result =
                Collections.deleteAsync repository command CancellationToken.None

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(CollectionNotFound(CollectionId 999), error)
        }

    [<Fact>]
    member _.``deleteAsync returns CollectionAccessDenied when user does not own collection``() =
        task {
            let repository = MockCollectionRepository()
            repository.AddCollection(makeCollectionData 1 1 "Collection" None)

            let command =
                { CollectionId = CollectionId 1
                  UserId = UserId 2 }

            let! result =
                Collections.deleteAsync repository command CancellationToken.None

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(CollectionAccessDenied(CollectionId 1), error)
        }
