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
                    [ { Id = 1
                        UserId = 200
                        Name = "Collection 1"
                        Description = null
                        CreatedAtDateTime = createdAt
                        CreatedAtOffset = int16 createdAt.Offset.TotalMinutes
                        UpdatedAtDateTime = createdAt
                        UpdatedAtOffset = int16 createdAt.Offset.TotalMinutes } ]
                |> Seeder.saveChangesAsync

            let vocabularyId = 1

            do!
                Vocabularies.createVocabularyAsync
                    { Id = vocabularyId
                      CollectionId = 1
                      Name = "My Vocabulary"
                      Description = Some "Test vocabulary"
                      CreatedAt = createdAt }
                |> fixture.WithConnectionAsync

            let! vocabulary =
                Vocabularies.getVocabularyByIdAsync vocabularyId
                |> fixture.WithConnectionAsync

            Assert.True(vocabulary.IsSome)
            Assert.Equal("My Vocabulary", vocabulary.Value.Name)
            Assert.Equal(Some "Test vocabulary", vocabulary.Value.Description)
        }

    [<Fact>]
    member _.``getVocabularyByIdAsync returns None when vocabulary does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! vocabulary =
                Vocabularies.getVocabularyByIdAsync 999
                |> fixture.WithConnectionAsync

            Assert.True(vocabulary.IsNone)
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
                    [ { Id = 2
                        UserId = 201
                        Name = "Collection 2"
                        Description = null
                        CreatedAtDateTime = createdAt
                        CreatedAtOffset = int16 createdAt.Offset.TotalMinutes
                        UpdatedAtDateTime = createdAt
                        UpdatedAtOffset = int16 createdAt.Offset.TotalMinutes } ]
                |> Seeder.saveChangesAsync

            let vocabularyId = 2

            do!
                Vocabularies.createVocabularyAsync
                    { Id = vocabularyId
                      CollectionId = 2
                      Name = "Original Name"
                      Description = Some "Original Description"
                      CreatedAt = createdAt }
                |> fixture.WithConnectionAsync

            let! affectedRows =
                Vocabularies.updateVocabularyAsync
                    { Id = vocabularyId
                      Name = "Updated Name"
                      Description = Some "Updated Description"
                      UpdatedAt = updatedAt }
                |> fixture.WithConnectionAsync

            Assert.Equal(1, affectedRows)

            let! updatedVocabulary =
                Vocabularies.getVocabularyByIdAsync vocabularyId
                |> fixture.WithConnectionAsync

            Assert.Equal("Updated Name", updatedVocabulary.Value.Name)
            Assert.Equal(Some "Updated Description", updatedVocabulary.Value.Description)
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
                    [ { Id = 3
                        UserId = 202
                        Name = "Collection 3"
                        Description = null
                        CreatedAtDateTime = createdAt
                        CreatedAtOffset = int16 createdAt.Offset.TotalMinutes
                        UpdatedAtDateTime = createdAt
                        UpdatedAtOffset = int16 createdAt.Offset.TotalMinutes } ]
                |> Seeder.saveChangesAsync

            let vocabularyId = 3

            do!
                Vocabularies.createVocabularyAsync
                    { Id = vocabularyId
                      CollectionId = 3
                      Name = "Vocabulary to delete"
                      Description = None
                      CreatedAt = createdAt }
                |> fixture.WithConnectionAsync

            let! affectedRows =
                Vocabularies.deleteVocabularyAsync vocabularyId
                |> fixture.WithConnectionAsync

            Assert.Equal(1, affectedRows)

            let! deletedVocabulary =
                Vocabularies.getVocabularyByIdAsync vocabularyId
                |> fixture.WithConnectionAsync

            Assert.True(deletedVocabulary.IsNone)
        }
