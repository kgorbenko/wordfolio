namespace Wordfolio.Api.DataAccess.Tests

open System

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type VocabulariesTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``createVocabularyAsync inserts a vocabulary``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.FromHours(0.0))

            do!
                fixture.Seeder
                |> Seeder.addUsers [ { Id = 200 } ]
                |> Seeder.addCollections
                    [ { Id = 0
                        UserId = 200
                        Name = "Collection 1"
                        Description = null
                        CreatedAt = createdAt
                        UpdatedAt = Nullable() } ]
                |> Seeder.saveChangesAsync

            let! allCollections =
                fixture.Seeder
                |> Seeder.getAllCollectionsAsync

            let collectionId = allCollections.[0].Id

            do!
                Vocabularies.createVocabularyAsync
                    { CollectionId = collectionId
                      Name = "My Vocabulary"
                      Description = Some "Test vocabulary"
                      CreatedAt = createdAt }
                |> fixture.WithConnectionAsync

            let! actual =
                fixture.Seeder
                |> Seeder.getAllVocabulariesAsync

            Assert.Single(actual) |> ignore

            let vocabulary = actual.[0]
            Assert.Equal(collectionId, vocabulary.CollectionId)
            Assert.Equal("My Vocabulary", vocabulary.Name)
            Assert.Equal("Test vocabulary", vocabulary.Description)
            Assert.Equal(createdAt, vocabulary.CreatedAt)
            Assert.False(vocabulary.UpdatedAt.HasValue)
        }

    [<Fact>]
    member _.``getVocabularyByIdAsync returns vocabulary when it exists``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.FromHours(0.0))

            do!
                fixture.Seeder
                |> Seeder.addUsers [ { Id = 200 } ]
                |> Seeder.addCollections
                    [ { Id = 0
                        UserId = 200
                        Name = "Collection 1"
                        Description = null
                        CreatedAt = createdAt
                        UpdatedAt = Nullable() } ]
                |> Seeder.saveChangesAsync

            let! allCollections =
                fixture.Seeder
                |> Seeder.getAllCollectionsAsync

            let collectionId = allCollections.[0].Id

            do!
                fixture.Seeder
                |> Seeder.addVocabularies
                    [ { Id = 0
                        CollectionId = collectionId
                        Name = "My Vocabulary"
                        Description = "Test vocabulary"
                        CreatedAt = createdAt
                        UpdatedAt = Nullable() } ]
                |> Seeder.saveChangesAsync

            let! allVocabularies =
                fixture.Seeder
                |> Seeder.getAllVocabulariesAsync

            let vocabularyId = allVocabularies.[0].Id

            let! actual =
                Vocabularies.getVocabularyByIdAsync vocabularyId
                |> fixture.WithConnectionAsync

            let expected: Vocabularies.Vocabulary =
                { Id = vocabularyId
                  CollectionId = collectionId
                  Name = "My Vocabulary"
                  Description = Some "Test vocabulary"
                  CreatedAt = createdAt
                  UpdatedAt = None }

            Assert.Equivalent(Some expected, actual)
        }

    [<Fact>]
    member _.``getVocabularyByIdAsync returns None when vocabulary does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! actual =
                Vocabularies.getVocabularyByIdAsync 999
                |> fixture.WithConnectionAsync

            Assert.True(actual.IsNone)
        }

    [<Fact>]
    member _.``getVocabulariesByCollectionIdAsync returns vocabularies for collection``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.FromHours(0.0))

            do!
                fixture.Seeder
                |> Seeder.addUsers [ { Id = 200 } ]
                |> Seeder.addCollections
                    [ { Id = 0
                        UserId = 200
                        Name = "Collection 1"
                        Description = null
                        CreatedAt = createdAt
                        UpdatedAt = Nullable() }
                      { Id = 0
                        UserId = 200
                        Name = "Collection 2"
                        Description = null
                        CreatedAt = createdAt
                        UpdatedAt = Nullable() } ]
                |> Seeder.saveChangesAsync

            let! allCollections =
                fixture.Seeder
                |> Seeder.getAllCollectionsAsync

            let collection1Id = allCollections.[0].Id
            let collection2Id = allCollections.[1].Id

            do!
                fixture.Seeder
                |> Seeder.addVocabularies
                    [ { Id = 0
                        CollectionId = collection1Id
                        Name = "Vocab 1"
                        Description = null
                        CreatedAt = createdAt
                        UpdatedAt = Nullable() }
                      { Id = 0
                        CollectionId = collection1Id
                        Name = "Vocab 2"
                        Description = null
                        CreatedAt = createdAt
                        UpdatedAt = Nullable() }
                      { Id = 0
                        CollectionId = collection2Id
                        Name = "Vocab 3"
                        Description = null
                        CreatedAt = createdAt
                        UpdatedAt = Nullable() } ]
                |> Seeder.saveChangesAsync

            let! actual =
                Vocabularies.getVocabulariesByCollectionIdAsync collection1Id
                |> fixture.WithConnectionAsync

            Assert.Equal(2, actual.Length)

            Assert.True(
                actual
                |> List.forall(fun v -> v.CollectionId = collection1Id)
            )
        }

    [<Fact>]
    member _.``getVocabulariesByCollectionIdAsync returns empty list when collection has no vocabularies``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.FromHours(0.0))

            do!
                fixture.Seeder
                |> Seeder.addUsers [ { Id = 200 } ]
                |> Seeder.addCollections
                    [ { Id = 0
                        UserId = 200
                        Name = "Collection 1"
                        Description = null
                        CreatedAt = createdAt
                        UpdatedAt = Nullable() } ]
                |> Seeder.saveChangesAsync

            let! allCollections =
                fixture.Seeder
                |> Seeder.getAllCollectionsAsync

            let collectionId = allCollections.[0].Id

            let! actual =
                Vocabularies.getVocabulariesByCollectionIdAsync collectionId
                |> fixture.WithConnectionAsync

            Assert.Empty(actual)
        }

    [<Fact>]
    member _.``updateVocabularyAsync updates an existing vocabulary``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.FromHours(0.0))

            let updatedAt =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.FromHours(0.0))

            do!
                fixture.Seeder
                |> Seeder.addUsers [ { Id = 201 } ]
                |> Seeder.addCollections
                    [ { Id = 0
                        UserId = 201
                        Name = "Collection 2"
                        Description = null
                        CreatedAt = createdAt
                        UpdatedAt = Nullable() } ]
                |> Seeder.saveChangesAsync

            let! allCollections =
                fixture.Seeder
                |> Seeder.getAllCollectionsAsync

            let collectionId = allCollections.[0].Id

            do!
                fixture.Seeder
                |> Seeder.addVocabularies
                    [ { Id = 0
                        CollectionId = collectionId
                        Name = "Original Name"
                        Description = "Original Description"
                        CreatedAt = createdAt
                        UpdatedAt = Nullable() } ]
                |> Seeder.saveChangesAsync

            let! allVocabularies =
                fixture.Seeder
                |> Seeder.getAllVocabulariesAsync

            let vocabularyId = allVocabularies.[0].Id

            let! affectedRows =
                Vocabularies.updateVocabularyAsync
                    { Id = vocabularyId
                      Name = "Updated Name"
                      Description = Some "Updated Description"
                      UpdatedAt = updatedAt }
                |> fixture.WithConnectionAsync

            Assert.Equal(1, affectedRows)

            let! actual =
                fixture.Seeder
                |> Seeder.getAllVocabulariesAsync

            let expected: VocabularyEntity list =
                [ { Id = vocabularyId
                    CollectionId = collectionId
                    Name = "Updated Name"
                    Description = "Updated Description"
                    CreatedAt = createdAt
                    UpdatedAt = Nullable(updatedAt) } ]

            Assert.Equivalent(expected, actual)
        }

    [<Fact>]
    member _.``updateVocabularyAsync returns 0 when vocabulary does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let updatedAt =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.FromHours(0.0))

            let! affectedRows =
                Vocabularies.updateVocabularyAsync
                    { Id = 999
                      Name = "Updated Name"
                      Description = Some "Updated Description"
                      UpdatedAt = updatedAt }
                |> fixture.WithConnectionAsync

            Assert.Equal(0, affectedRows)
        }

    [<Fact>]
    member _.``deleteVocabularyAsync deletes an existing vocabulary``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.FromHours(0.0))

            do!
                fixture.Seeder
                |> Seeder.addUsers [ { Id = 202 } ]
                |> Seeder.addCollections
                    [ { Id = 0
                        UserId = 202
                        Name = "Collection 3"
                        Description = null
                        CreatedAt = createdAt
                        UpdatedAt = Nullable() } ]
                |> Seeder.saveChangesAsync

            let! allCollections =
                fixture.Seeder
                |> Seeder.getAllCollectionsAsync

            let collectionId = allCollections.[0].Id

            do!
                fixture.Seeder
                |> Seeder.addVocabularies
                    [ { Id = 0
                        CollectionId = collectionId
                        Name = "Vocabulary to delete"
                        Description = null
                        CreatedAt = createdAt
                        UpdatedAt = Nullable() } ]
                |> Seeder.saveChangesAsync

            let! allVocabularies =
                fixture.Seeder
                |> Seeder.getAllVocabulariesAsync

            let vocabularyId = allVocabularies.[0].Id

            let! affectedRows =
                Vocabularies.deleteVocabularyAsync vocabularyId
                |> fixture.WithConnectionAsync

            Assert.Equal(1, affectedRows)

            let! actual =
                fixture.Seeder
                |> Seeder.getAllVocabulariesAsync

            Assert.Empty(actual)
        }

    [<Fact>]
    member _.``deleteVocabularyAsync returns 0 when vocabulary does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! affectedRows =
                Vocabularies.deleteVocabularyAsync 999
                |> fixture.WithConnectionAsync

            Assert.Equal(0, affectedRows)
        }
