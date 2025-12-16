namespace Wordfolio.Api.Domain.Tests

open System

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Collections
open Wordfolio.Api.Domain.Collections.Operations
open Wordfolio.Api.Domain.Tests.TestHelpers

type CollectionsTests() =

    [<Fact>]
    member _.``getById returns collection when user owns it``() =
        task {
            let collection =
                makeCollection 1 1 "Test Collection" (Some "Description")

            let collections =
                ref(Map.ofList [ 1, collection ])

            let appEnv = TestCollectionsEnv(collections)
            let env = TestTransactionalEnv(appEnv)

            let! result = getById env (UserId 1) (CollectionId 1)

            match result with
            | Error _ -> Assert.Fail("Expected Ok result")
            | Ok c ->
                Assert.Equal(CollectionId 1, c.Id)
                Assert.Equal(UserId 1, c.UserId)
                Assert.Equal("Test Collection", c.Name)
                Assert.Equal(Some "Description", c.Description)
        }

    [<Fact>]
    member _.``getById returns CollectionNotFound when collection does not exist``() =
        task {
            let collections = ref Map.empty
            let appEnv = TestCollectionsEnv(collections)
            let env = TestTransactionalEnv(appEnv)

            let! result = getById env (UserId 1) (CollectionId 999)

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(CollectionNotFound(CollectionId 999), error)
        }

    [<Fact>]
    member _.``getById returns CollectionAccessDenied when user does not own collection``() =
        task {
            let collection =
                makeCollection 1 1 "Test Collection" None

            let collections =
                ref(Map.ofList [ 1, collection ])

            let appEnv = TestCollectionsEnv(collections)
            let env = TestTransactionalEnv(appEnv)

            let! result = getById env (UserId 2) (CollectionId 1)

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(CollectionAccessDenied(CollectionId 1), error)
        }

    [<Fact>]
    member _.``getByUserId returns collections for user``() =
        task {
            let collection1 =
                makeCollection 1 1 "Collection 1" None

            let collection2 =
                makeCollection 2 1 "Collection 2" None

            let collection3 =
                makeCollection 3 2 "Other User Collection" None

            let collections =
                ref(Map.ofList [ 1, collection1; 2, collection2; 3, collection3 ])

            let appEnv = TestCollectionsEnv(collections)
            let env = TestTransactionalEnv(appEnv)

            let! result = getByUserId env (UserId 1)

            Assert.Equal(2, result.Length)

            Assert.True(
                result
                |> List.exists(fun c -> c.Name = "Collection 1")
            )

            Assert.True(
                result
                |> List.exists(fun c -> c.Name = "Collection 2")
            )
        }

    [<Fact>]
    member _.``getByUserId returns empty list when user has no collections``() =
        task {
            let collections = ref Map.empty
            let appEnv = TestCollectionsEnv(collections)
            let env = TestTransactionalEnv(appEnv)

            let! result = getByUserId env (UserId 1)

            Assert.Empty(result)
        }

    [<Fact>]
    member _.``create creates collection with valid data``() =
        task {
            let collections = ref Map.empty
            let appEnv = TestCollectionsEnv(collections)
            let env = TestTransactionalEnv(appEnv)

            let now =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let! result = create env (UserId 1) "New Collection" (Some "A new collection") now

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
    member _.``create returns error when name is empty``() =
        task {
            let collections = ref Map.empty
            let appEnv = TestCollectionsEnv(collections)
            let env = TestTransactionalEnv(appEnv)

            let now =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let! result = create env (UserId 1) "" None now

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(CollectionNameRequired, error)
        }

    [<Fact>]
    member _.``create returns error when name is whitespace only``() =
        task {
            let collections = ref Map.empty
            let appEnv = TestCollectionsEnv(collections)
            let env = TestTransactionalEnv(appEnv)

            let now =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let! result = create env (UserId 1) "   " None now

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(CollectionNameRequired, error)
        }

    [<Fact>]
    member _.``create returns error when name exceeds max length``() =
        task {
            let collections = ref Map.empty
            let appEnv = TestCollectionsEnv(collections)
            let env = TestTransactionalEnv(appEnv)

            let now =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let longName = String.replicate 256 "a"

            let! result = create env (UserId 1) longName None now

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(CollectionNameTooLong MaxNameLength, error)
        }

    [<Fact>]
    member _.``update updates collection when user owns it``() =
        task {
            let collection =
                makeCollection 1 1 "Original Name" (Some "Original")

            let collections =
                ref(Map.ofList [ 1, collection ])

            let appEnv = TestCollectionsEnv(collections)
            let env = TestTransactionalEnv(appEnv)

            let now =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let! result = update env (UserId 1) (CollectionId 1) "Updated Name" (Some "Updated Description") now

            match result with
            | Error _ -> Assert.Fail("Expected Ok result")
            | Ok c ->
                Assert.Equal("Updated Name", c.Name)
                Assert.Equal(Some "Updated Description", c.Description)
                Assert.Equal(Some now, c.UpdatedAt)
        }

    [<Fact>]
    member _.``update returns CollectionNotFound when collection does not exist``() =
        task {
            let collections = ref Map.empty
            let appEnv = TestCollectionsEnv(collections)
            let env = TestTransactionalEnv(appEnv)

            let now =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let! result = update env (UserId 1) (CollectionId 999) "Updated Name" None now

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(CollectionNotFound(CollectionId 999), error)
        }

    [<Fact>]
    member _.``update returns CollectionAccessDenied when user does not own collection``() =
        task {
            let collection =
                makeCollection 1 1 "Original Name" None

            let collections =
                ref(Map.ofList [ 1, collection ])

            let appEnv = TestCollectionsEnv(collections)
            let env = TestTransactionalEnv(appEnv)

            let now =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let! result = update env (UserId 2) (CollectionId 1) "Updated Name" None now

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(CollectionAccessDenied(CollectionId 1), error)
        }

    [<Fact>]
    member _.``update returns error when name is empty``() =
        task {
            let collection =
                makeCollection 1 1 "Original Name" None

            let collections =
                ref(Map.ofList [ 1, collection ])

            let appEnv = TestCollectionsEnv(collections)
            let env = TestTransactionalEnv(appEnv)

            let now =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let! result = update env (UserId 1) (CollectionId 1) "" None now

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(CollectionNameRequired, error)
        }

    [<Fact>]
    member _.``delete deletes collection when user owns it``() =
        task {
            let collection =
                makeCollection 1 1 "Collection to delete" None

            let collections =
                ref(Map.ofList [ 1, collection ])

            let appEnv = TestCollectionsEnv(collections)
            let env = TestTransactionalEnv(appEnv)

            let! result = delete env (UserId 1) (CollectionId 1)

            match result with
            | Error _ -> Assert.Fail("Expected Ok result")
            | Ok() -> Assert.Empty(collections.Value)
        }

    [<Fact>]
    member _.``delete returns CollectionNotFound when collection does not exist``() =
        task {
            let collections = ref Map.empty
            let appEnv = TestCollectionsEnv(collections)
            let env = TestTransactionalEnv(appEnv)

            let! result = delete env (UserId 1) (CollectionId 999)

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(CollectionNotFound(CollectionId 999), error)
        }

    [<Fact>]
    member _.``delete returns CollectionAccessDenied when user does not own collection``() =
        task {
            let collection =
                makeCollection 1 1 "Collection" None

            let collections =
                ref(Map.ofList [ 1, collection ])

            let appEnv = TestCollectionsEnv(collections)
            let env = TestTransactionalEnv(appEnv)

            let! result = delete env (UserId 2) (CollectionId 1)

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(CollectionAccessDenied(CollectionId 1), error)
        }
