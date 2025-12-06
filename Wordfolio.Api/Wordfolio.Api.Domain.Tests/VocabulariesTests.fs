namespace Wordfolio.Api.Domain.Tests

open System
open System.Threading

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Vocabularies
open Wordfolio.Api.Domain.Tests.TestHelpers

type VocabulariesTests() =

    [<Fact>]
    member _.``getByIdAsync returns vocabulary when user owns parent collection``() =
        task {
            let collectionRepo = MockCollectionRepository()
            let vocabularyRepo = MockVocabularyRepository()
            let userId = UserId 1
            let vocabularyId = VocabularyId 1

            collectionRepo.AddCollection(makeCollectionData 1 1 "Collection" None)
            vocabularyRepo.AddVocabulary(makeVocabularyData 1 1 "Vocabulary" (Some "Description"))

            let! result =
                Vocabularies.getByIdAsync vocabularyRepo collectionRepo userId vocabularyId CancellationToken.None

            match result with
            | Error _ -> Assert.Fail("Expected Ok result")
            | Ok vocabulary ->
                Assert.Equal(vocabularyId, vocabulary.Id)
                Assert.Equal("Vocabulary", vocabulary.Name)
                Assert.Equal(Some "Description", vocabulary.Description)
        }

    [<Fact>]
    member _.``getByIdAsync returns VocabularyNotFound when vocabulary does not exist``() =
        task {
            let collectionRepo = MockCollectionRepository()
            let vocabularyRepo = MockVocabularyRepository()
            let userId = UserId 1
            let vocabularyId = VocabularyId 999

            let! result =
                Vocabularies.getByIdAsync vocabularyRepo collectionRepo userId vocabularyId CancellationToken.None

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(VocabularyNotFound(VocabularyId 999), error)
        }

    [<Fact>]
    member _.``getByIdAsync returns VocabularyAccessDenied when user does not own parent collection``() =
        task {
            let collectionRepo = MockCollectionRepository()
            let vocabularyRepo = MockVocabularyRepository()
            let otherUserId = UserId 2
            let vocabularyId = VocabularyId 1

            collectionRepo.AddCollection(makeCollectionData 1 1 "Collection" None)
            vocabularyRepo.AddVocabulary(makeVocabularyData 1 1 "Vocabulary" None)

            let! result =
                Vocabularies.getByIdAsync vocabularyRepo collectionRepo otherUserId vocabularyId CancellationToken.None

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(VocabularyAccessDenied(VocabularyId 1), error)
        }

    [<Fact>]
    member _.``getByCollectionIdAsync returns vocabularies when user owns collection``() =
        task {
            let collectionRepo = MockCollectionRepository()
            let vocabularyRepo = MockVocabularyRepository()
            let userId = UserId 1
            let collectionId = CollectionId 1

            collectionRepo.AddCollection(makeCollectionData 1 1 "Collection" None)
            vocabularyRepo.AddVocabulary(makeVocabularyData 1 1 "Vocabulary 1" None)
            vocabularyRepo.AddVocabulary(makeVocabularyData 2 1 "Vocabulary 2" None)
            vocabularyRepo.AddVocabulary(makeVocabularyData 3 2 "Other Collection Vocabulary" None)

            let! result =
                Vocabularies.getByCollectionIdAsync
                    vocabularyRepo
                    collectionRepo
                    userId
                    collectionId
                    CancellationToken.None

            match result with
            | Error _ -> Assert.Fail("Expected Ok result")
            | Ok vocabularies ->
                Assert.Equal(2, vocabularies.Length)
                Assert.True(vocabularies |> List.exists(fun v -> v.Name = "Vocabulary 1"))
                Assert.True(vocabularies |> List.exists(fun v -> v.Name = "Vocabulary 2"))
        }

    [<Fact>]
    member _.``getByCollectionIdAsync returns error when user does not own collection``() =
        task {
            let collectionRepo = MockCollectionRepository()
            let vocabularyRepo = MockVocabularyRepository()
            let otherUserId = UserId 2
            let collectionId = CollectionId 1

            collectionRepo.AddCollection(makeCollectionData 1 1 "Collection" None)

            let! result =
                Vocabularies.getByCollectionIdAsync
                    vocabularyRepo
                    collectionRepo
                    otherUserId
                    collectionId
                    CancellationToken.None

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(VocabularyCollectionNotFound(CollectionId 1), error)
        }

    [<Fact>]
    member _.``getByCollectionIdAsync returns error when collection does not exist``() =
        task {
            let collectionRepo = MockCollectionRepository()
            let vocabularyRepo = MockVocabularyRepository()
            let userId = UserId 1
            let collectionId = CollectionId 999

            let! result =
                Vocabularies.getByCollectionIdAsync
                    vocabularyRepo
                    collectionRepo
                    userId
                    collectionId
                    CancellationToken.None

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(VocabularyCollectionNotFound(CollectionId 999), error)
        }

    [<Fact>]
    member _.``createAsync creates vocabulary when user owns collection``() =
        task {
            let collectionRepo = MockCollectionRepository()
            let vocabularyRepo = MockVocabularyRepository()
            let now = DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            collectionRepo.AddCollection(makeCollectionData 1 1 "Collection" None)

            let command =
                { UserId = UserId 1
                  CollectionId = CollectionId 1
                  Name = "New Vocabulary"
                  Description = Some "A new vocabulary" }

            let! result =
                Vocabularies.createAsync vocabularyRepo collectionRepo command now CancellationToken.None

            match result with
            | Error _ -> Assert.Fail("Expected Ok result")
            | Ok vocabulary ->
                Assert.Equal("New Vocabulary", vocabulary.Name)
                Assert.Equal(Some "A new vocabulary", vocabulary.Description)
                Assert.Equal(CollectionId 1, vocabulary.CollectionId)
                Assert.Equal(now, vocabulary.CreatedAt)
        }

    [<Fact>]
    member _.``createAsync returns error when user does not own collection``() =
        task {
            let collectionRepo = MockCollectionRepository()
            let vocabularyRepo = MockVocabularyRepository()
            let now = DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            collectionRepo.AddCollection(makeCollectionData 1 1 "Collection" None)

            let command =
                { UserId = UserId 2
                  CollectionId = CollectionId 1
                  Name = "New Vocabulary"
                  Description = None }

            let! result =
                Vocabularies.createAsync vocabularyRepo collectionRepo command now CancellationToken.None

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(VocabularyCollectionNotFound(CollectionId 1), error)
        }

    [<Fact>]
    member _.``createAsync returns error when collection does not exist``() =
        task {
            let collectionRepo = MockCollectionRepository()
            let vocabularyRepo = MockVocabularyRepository()
            let now = DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let command =
                { UserId = UserId 1
                  CollectionId = CollectionId 999
                  Name = "New Vocabulary"
                  Description = None }

            let! result =
                Vocabularies.createAsync vocabularyRepo collectionRepo command now CancellationToken.None

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(VocabularyCollectionNotFound(CollectionId 999), error)
        }

    [<Fact>]
    member _.``createAsync returns error when name is empty``() =
        task {
            let collectionRepo = MockCollectionRepository()
            let vocabularyRepo = MockVocabularyRepository()
            let now = DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            collectionRepo.AddCollection(makeCollectionData 1 1 "Collection" None)

            let command =
                { UserId = UserId 1
                  CollectionId = CollectionId 1
                  Name = ""
                  Description = None }

            let! result =
                Vocabularies.createAsync vocabularyRepo collectionRepo command now CancellationToken.None

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(VocabularyNameRequired, error)
        }

    [<Fact>]
    member _.``createAsync returns error when name exceeds max length``() =
        task {
            let collectionRepo = MockCollectionRepository()
            let vocabularyRepo = MockVocabularyRepository()
            let now = DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)
            let longName = String.replicate 256 "a"

            collectionRepo.AddCollection(makeCollectionData 1 1 "Collection" None)

            let command =
                { UserId = UserId 1
                  CollectionId = CollectionId 1
                  Name = longName
                  Description = None }

            let! result =
                Vocabularies.createAsync vocabularyRepo collectionRepo command now CancellationToken.None

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(VocabularyNameTooLong MaxNameLength, error)
        }

    [<Fact>]
    member _.``createAsync trims name whitespace``() =
        task {
            let collectionRepo = MockCollectionRepository()
            let vocabularyRepo = MockVocabularyRepository()
            let now = DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            collectionRepo.AddCollection(makeCollectionData 1 1 "Collection" None)

            let command =
                { UserId = UserId 1
                  CollectionId = CollectionId 1
                  Name = "  Trimmed Name  "
                  Description = None }

            let! result =
                Vocabularies.createAsync vocabularyRepo collectionRepo command now CancellationToken.None

            match result with
            | Error _ -> Assert.Fail("Expected Ok result")
            | Ok vocabulary -> Assert.Equal("Trimmed Name", vocabulary.Name)
        }

    [<Fact>]
    member _.``updateAsync updates vocabulary when user owns parent collection``() =
        task {
            let collectionRepo = MockCollectionRepository()
            let vocabularyRepo = MockVocabularyRepository()
            let now = DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            collectionRepo.AddCollection(makeCollectionData 1 1 "Collection" None)
            vocabularyRepo.AddVocabulary(makeVocabularyData 1 1 "Original Name" None)

            let command =
                { UserId = UserId 1
                  VocabularyId = VocabularyId 1
                  Name = "Updated Name"
                  Description = Some "Updated Description" }

            let! result =
                Vocabularies.updateAsync vocabularyRepo collectionRepo command now CancellationToken.None

            match result with
            | Error _ -> Assert.Fail("Expected Ok result")
            | Ok vocabulary ->
                Assert.Equal("Updated Name", vocabulary.Name)
                Assert.Equal(Some "Updated Description", vocabulary.Description)
                Assert.Equal(Some now, vocabulary.UpdatedAt)
        }

    [<Fact>]
    member _.``updateAsync returns VocabularyNotFound when vocabulary does not exist``() =
        task {
            let collectionRepo = MockCollectionRepository()
            let vocabularyRepo = MockVocabularyRepository()
            let now = DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let command =
                { UserId = UserId 1
                  VocabularyId = VocabularyId 999
                  Name = "Updated Name"
                  Description = None }

            let! result =
                Vocabularies.updateAsync vocabularyRepo collectionRepo command now CancellationToken.None

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(VocabularyNotFound(VocabularyId 999), error)
        }

    [<Fact>]
    member _.``updateAsync returns VocabularyAccessDenied when user does not own parent collection``() =
        task {
            let collectionRepo = MockCollectionRepository()
            let vocabularyRepo = MockVocabularyRepository()
            let now = DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            collectionRepo.AddCollection(makeCollectionData 1 1 "Collection" None)
            vocabularyRepo.AddVocabulary(makeVocabularyData 1 1 "Original Name" None)

            let command =
                { UserId = UserId 2
                  VocabularyId = VocabularyId 1
                  Name = "Updated Name"
                  Description = None }

            let! result =
                Vocabularies.updateAsync vocabularyRepo collectionRepo command now CancellationToken.None

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(VocabularyAccessDenied(VocabularyId 1), error)
        }

    [<Fact>]
    member _.``updateAsync returns error when name is empty``() =
        task {
            let collectionRepo = MockCollectionRepository()
            let vocabularyRepo = MockVocabularyRepository()
            let now = DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            collectionRepo.AddCollection(makeCollectionData 1 1 "Collection" None)
            vocabularyRepo.AddVocabulary(makeVocabularyData 1 1 "Original Name" None)

            let command =
                { UserId = UserId 1
                  VocabularyId = VocabularyId 1
                  Name = ""
                  Description = None }

            let! result =
                Vocabularies.updateAsync vocabularyRepo collectionRepo command now CancellationToken.None

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(VocabularyNameRequired, error)
        }

    [<Fact>]
    member _.``deleteAsync deletes vocabulary when user owns parent collection``() =
        task {
            let collectionRepo = MockCollectionRepository()
            let vocabularyRepo = MockVocabularyRepository()

            collectionRepo.AddCollection(makeCollectionData 1 1 "Collection" None)
            vocabularyRepo.AddVocabulary(makeVocabularyData 1 1 "Vocabulary" None)

            let command =
                { UserId = UserId 1
                  VocabularyId = VocabularyId 1 }

            let! result =
                Vocabularies.deleteAsync vocabularyRepo collectionRepo command CancellationToken.None

            match result with
            | Error _ -> Assert.Fail("Expected Ok result")
            | Ok() -> Assert.Empty(vocabularyRepo.Vocabularies)
        }

    [<Fact>]
    member _.``deleteAsync returns VocabularyNotFound when vocabulary does not exist``() =
        task {
            let collectionRepo = MockCollectionRepository()
            let vocabularyRepo = MockVocabularyRepository()

            let command =
                { UserId = UserId 1
                  VocabularyId = VocabularyId 999 }

            let! result =
                Vocabularies.deleteAsync vocabularyRepo collectionRepo command CancellationToken.None

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(VocabularyNotFound(VocabularyId 999), error)
        }

    [<Fact>]
    member _.``deleteAsync returns VocabularyAccessDenied when user does not own parent collection``() =
        task {
            let collectionRepo = MockCollectionRepository()
            let vocabularyRepo = MockVocabularyRepository()

            collectionRepo.AddCollection(makeCollectionData 1 1 "Collection" None)
            vocabularyRepo.AddVocabulary(makeVocabularyData 1 1 "Vocabulary" None)

            let command =
                { UserId = UserId 2
                  VocabularyId = VocabularyId 1 }

            let! result =
                Vocabularies.deleteAsync vocabularyRepo collectionRepo command CancellationToken.None

            match result with
            | Ok _ -> Assert.Fail("Expected Error result")
            | Error error -> Assert.Equal(VocabularyAccessDenied(VocabularyId 1), error)
        }
