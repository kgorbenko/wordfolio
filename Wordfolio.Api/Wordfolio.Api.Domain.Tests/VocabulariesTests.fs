namespace Wordfolio.Api.Domain.Tests

open System

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Collections
open Wordfolio.Api.Domain.Vocabularies
open Wordfolio.Api.Domain.Vocabularies.Operations
open Wordfolio.Api.Domain.Tests.TestHelpers

type VocabulariesTests() =

    [<Fact>]
    member _.``getById returns vocabulary when user owns parent collection``() =
        task {
            let collection =
                makeCollection 1 1 "Collection" None

            let vocabulary =
                makeVocabulary 1 1 "Vocabulary" (Some "Description")

            let collections =
                ref(Map.ofList [ 1, collection ])

            let vocabularies =
                ref(Map.ofList [ 1, vocabulary ])

            let env =
                TestVocabulariesEnv(collections, vocabularies)

            let! result = getById env (UserId 1) (VocabularyId 1)

            match result with
            | Error _ -> Assert.Fail("Expected Ok result")
            | Ok v ->
                Assert.Equal(VocabularyId 1, v.Id)
                Assert.Equal("Vocabulary", v.Name)
                Assert.Equal(Some "Description", v.Description)
        }

    [<Fact>]
    member _.``getById returns VocabularyNotFound when vocabulary does not exist``() =
        task {
            let collections = ref Map.empty
            let vocabularies = ref Map.empty

            let env =
                TestVocabulariesEnv(collections, vocabularies)

            let! result = getById env (UserId 1) (VocabularyId 999)

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(VocabularyNotFound(VocabularyId 999), error)
        }

    [<Fact>]
    member _.``getById returns VocabularyAccessDenied when user does not own parent collection``() =
        task {
            let collection =
                makeCollection 1 1 "Collection" None

            let vocabulary =
                makeVocabulary 1 1 "Vocabulary" None

            let collections =
                ref(Map.ofList [ 1, collection ])

            let vocabularies =
                ref(Map.ofList [ 1, vocabulary ])

            let env =
                TestVocabulariesEnv(collections, vocabularies)

            let! result = getById env (UserId 2) (VocabularyId 1)

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(VocabularyAccessDenied(VocabularyId 1), error)
        }

    [<Fact>]
    member _.``getByCollectionId returns vocabularies when user owns collection``() =
        task {
            let collection =
                makeCollection 1 1 "Collection" None

            let vocabulary1 =
                makeVocabulary 1 1 "Vocabulary 1" None

            let vocabulary2 =
                makeVocabulary 2 1 "Vocabulary 2" None

            let vocabulary3 =
                makeVocabulary 3 2 "Other Collection Vocabulary" None

            let collections =
                ref(Map.ofList [ 1, collection ])

            let vocabularies =
                ref(Map.ofList [ 1, vocabulary1; 2, vocabulary2; 3, vocabulary3 ])

            let env =
                TestVocabulariesEnv(collections, vocabularies)

            let! result = getByCollectionId env (UserId 1) (CollectionId 1)

            match result with
            | Error _ -> Assert.Fail("Expected Ok result")
            | Ok vs ->
                Assert.Equal(2, vs.Length)

                Assert.True(
                    vs
                    |> List.exists(fun v -> v.Name = "Vocabulary 1")
                )

                Assert.True(
                    vs
                    |> List.exists(fun v -> v.Name = "Vocabulary 2")
                )
        }

    [<Fact>]
    member _.``getByCollectionId returns error when user does not own collection``() =
        task {
            let collection =
                makeCollection 1 1 "Collection" None

            let collections =
                ref(Map.ofList [ 1, collection ])

            let vocabularies = ref Map.empty

            let env =
                TestVocabulariesEnv(collections, vocabularies)

            let! result = getByCollectionId env (UserId 2) (CollectionId 1)

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(VocabularyCollectionNotFound(CollectionId 1), error)
        }

    [<Fact>]
    member _.``getByCollectionId returns error when collection does not exist``() =
        task {
            let collections = ref Map.empty
            let vocabularies = ref Map.empty

            let env =
                TestVocabulariesEnv(collections, vocabularies)

            let! result = getByCollectionId env (UserId 1) (CollectionId 999)

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(VocabularyCollectionNotFound(CollectionId 999), error)
        }

    [<Fact>]
    member _.``create creates vocabulary when user owns collection``() =
        task {
            let collection =
                makeCollection 1 1 "Collection" None

            let collections =
                ref(Map.ofList [ 1, collection ])

            let vocabularies = ref Map.empty

            let env =
                TestVocabulariesEnv(collections, vocabularies)

            let now =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let! result = create env (UserId 1) (CollectionId 1) "New Vocabulary" (Some "A new vocabulary") now

            match result with
            | Error _ -> Assert.Fail("Expected Ok result")
            | Ok vocabulary ->
                Assert.Equal("New Vocabulary", vocabulary.Name)
                Assert.Equal(Some "A new vocabulary", vocabulary.Description)
                Assert.Equal(CollectionId 1, vocabulary.CollectionId)
                Assert.Equal(now, vocabulary.CreatedAt)
        }

    [<Fact>]
    member _.``create returns error when user does not own collection``() =
        task {
            let collection =
                makeCollection 1 1 "Collection" None

            let collections =
                ref(Map.ofList [ 1, collection ])

            let vocabularies = ref Map.empty

            let env =
                TestVocabulariesEnv(collections, vocabularies)

            let now =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let! result = create env (UserId 2) (CollectionId 1) "New Vocabulary" None now

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(VocabularyCollectionNotFound(CollectionId 1), error)
        }

    [<Fact>]
    member _.``create returns error when collection does not exist``() =
        task {
            let collections = ref Map.empty
            let vocabularies = ref Map.empty

            let env =
                TestVocabulariesEnv(collections, vocabularies)

            let now =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let! result = create env (UserId 1) (CollectionId 999) "New Vocabulary" None now

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(VocabularyCollectionNotFound(CollectionId 999), error)
        }

    [<Fact>]
    member _.``create returns error when name is empty``() =
        task {
            let collection =
                makeCollection 1 1 "Collection" None

            let collections =
                ref(Map.ofList [ 1, collection ])

            let vocabularies = ref Map.empty

            let env =
                TestVocabulariesEnv(collections, vocabularies)

            let now =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let! result = create env (UserId 1) (CollectionId 1) "" None now

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(VocabularyNameRequired, error)
        }

    [<Fact>]
    member _.``create returns error when name exceeds max length``() =
        task {
            let collection =
                makeCollection 1 1 "Collection" None

            let collections =
                ref(Map.ofList [ 1, collection ])

            let vocabularies = ref Map.empty

            let env =
                TestVocabulariesEnv(collections, vocabularies)

            let now =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let longName = String.replicate 256 "a"

            let! result = create env (UserId 1) (CollectionId 1) longName None now

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(VocabularyNameTooLong MaxNameLength, error)
        }

    [<Fact>]
    member _.``update updates vocabulary when user owns parent collection``() =
        task {
            let collection =
                makeCollection 1 1 "Collection" None

            let vocabulary =
                makeVocabulary 1 1 "Original Name" None

            let collections =
                ref(Map.ofList [ 1, collection ])

            let vocabularies =
                ref(Map.ofList [ 1, vocabulary ])

            let env =
                TestVocabulariesEnv(collections, vocabularies)

            let now =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let! result = update env (UserId 1) (VocabularyId 1) "Updated Name" (Some "Updated Description") now

            match result with
            | Error _ -> Assert.Fail("Expected Ok result")
            | Ok v ->
                Assert.Equal("Updated Name", v.Name)
                Assert.Equal(Some "Updated Description", v.Description)
                Assert.Equal(Some now, v.UpdatedAt)
        }

    [<Fact>]
    member _.``update returns VocabularyNotFound when vocabulary does not exist``() =
        task {
            let collections = ref Map.empty
            let vocabularies = ref Map.empty

            let env =
                TestVocabulariesEnv(collections, vocabularies)

            let now =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let! result = update env (UserId 1) (VocabularyId 999) "Updated Name" None now

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(VocabularyNotFound(VocabularyId 999), error)
        }

    [<Fact>]
    member _.``update returns VocabularyAccessDenied when user does not own parent collection``() =
        task {
            let collection =
                makeCollection 1 1 "Collection" None

            let vocabulary =
                makeVocabulary 1 1 "Original Name" None

            let collections =
                ref(Map.ofList [ 1, collection ])

            let vocabularies =
                ref(Map.ofList [ 1, vocabulary ])

            let env =
                TestVocabulariesEnv(collections, vocabularies)

            let now =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let! result = update env (UserId 2) (VocabularyId 1) "Updated Name" None now

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(VocabularyAccessDenied(VocabularyId 1), error)
        }

    [<Fact>]
    member _.``update returns error when name is empty``() =
        task {
            let collection =
                makeCollection 1 1 "Collection" None

            let vocabulary =
                makeVocabulary 1 1 "Original Name" None

            let collections =
                ref(Map.ofList [ 1, collection ])

            let vocabularies =
                ref(Map.ofList [ 1, vocabulary ])

            let env =
                TestVocabulariesEnv(collections, vocabularies)

            let now =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let! result = update env (UserId 1) (VocabularyId 1) "" None now

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(VocabularyNameRequired, error)
        }

    [<Fact>]
    member _.``delete deletes vocabulary when user owns parent collection``() =
        task {
            let collection =
                makeCollection 1 1 "Collection" None

            let vocabulary =
                makeVocabulary 1 1 "Vocabulary" None

            let collections =
                ref(Map.ofList [ 1, collection ])

            let vocabularies =
                ref(Map.ofList [ 1, vocabulary ])

            let env =
                TestVocabulariesEnv(collections, vocabularies)

            let! result = delete env (UserId 1) (VocabularyId 1)

            match result with
            | Error _ -> Assert.Fail("Expected Ok result")
            | Ok() -> Assert.Empty(vocabularies.Value)
        }

    [<Fact>]
    member _.``delete returns VocabularyNotFound when vocabulary does not exist``() =
        task {
            let collections = ref Map.empty
            let vocabularies = ref Map.empty

            let env =
                TestVocabulariesEnv(collections, vocabularies)

            let! result = delete env (UserId 1) (VocabularyId 999)

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(VocabularyNotFound(VocabularyId 999), error)
        }

    [<Fact>]
    member _.``delete returns VocabularyAccessDenied when user does not own parent collection``() =
        task {
            let collection =
                makeCollection 1 1 "Collection" None

            let vocabulary =
                makeVocabulary 1 1 "Vocabulary" None

            let collections =
                ref(Map.ofList [ 1, collection ])

            let vocabularies =
                ref(Map.ofList [ 1, vocabulary ])

            let env =
                TestVocabulariesEnv(collections, vocabularies)

            let! result = delete env (UserId 2) (VocabularyId 1)

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(VocabularyAccessDenied(VocabularyId 1), error)
        }
