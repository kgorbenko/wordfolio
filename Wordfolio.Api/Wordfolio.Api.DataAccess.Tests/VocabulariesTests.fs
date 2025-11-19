namespace Wordfolio.Api.DataAccess.Tests

open System

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type VocabulariesTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``CRUD operations work``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.FromHours(0.0))

            let updatedAt =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.FromHours(0.0))

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

            // Create
            let vocabularyId = 1

            do!
                Vocabularies.createVocabularyAsync
                    { Id = vocabularyId
                      CollectionId = 1
                      Name = "My Vocabulary"
                      Description = Some "Test vocabulary"
                      CreatedAt = createdAt }
                |> fixture.WithConnectionAsync

            // Read by ID
            let! vocabulary =
                Vocabularies.getVocabularyByIdAsync vocabularyId
                |> fixture.WithConnectionAsync

            Assert.True(vocabulary.IsSome)
            Assert.Equal("My Vocabulary", vocabulary.Value.Name)
            Assert.Equal(Some "Test vocabulary", vocabulary.Value.Description)

            // Update
            let! affectedRows =
                Vocabularies.updateVocabularyAsync
                    { Id = vocabularyId
                      Name = "Updated Name"
                      Description = Some "Updated Description"
                      UpdatedAt = updatedAt }
                |> fixture.WithConnectionAsync

            Assert.Equal(1, affectedRows)

            // Read updated
            let! updatedVocabulary =
                Vocabularies.getVocabularyByIdAsync vocabularyId
                |> fixture.WithConnectionAsync

            Assert.Equal("Updated Name", updatedVocabulary.Value.Name)
            Assert.Equal(Some "Updated Description", updatedVocabulary.Value.Description)

            // Delete
            let! deleteAffectedRows =
                Vocabularies.deleteVocabularyAsync vocabularyId
                |> fixture.WithConnectionAsync

            Assert.Equal(1, deleteAffectedRows)

            // Verify deleted
            let! deletedVocabulary =
                Vocabularies.getVocabularyByIdAsync vocabularyId
                |> fixture.WithConnectionAsync

            Assert.True(deletedVocabulary.IsNone)
        }
