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
                |> Seeder.saveChangesAsync

            do!
                fixture.Seeder
                |> Seeder.addCollections
                    [ { Id = 1
                        UserId = 200
                        Name = "Collection 1"
                        Description = null
                        CreatedAt = createdAt
                        UpdatedAt = createdAt } ]
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

            let! actual =
                fixture.Seeder
                |> Seeder.getAllVocabulariesAsync

            let expected: VocabularyEntity list =
                [ { Id = vocabularyId
                    CollectionId = 1
                    Name = "My Vocabulary"
                    Description = "Test vocabulary"
                    CreatedAt = createdAt
                    UpdatedAt = createdAt } ]

            Assert.Equivalent(expected, actual)
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
                |> Seeder.saveChangesAsync

            do!
                fixture.Seeder
                |> Seeder.addCollections
                    [ { Id = 2
                        UserId = 201
                        Name = "Collection 2"
                        Description = null
                        CreatedAt = createdAt
                        UpdatedAt = createdAt } ]
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

            let! actual =
                fixture.Seeder
                |> Seeder.getAllVocabulariesAsync

            let expected: VocabularyEntity list =
                [ { Id = vocabularyId
                    CollectionId = 2
                    Name = "Updated Name"
                    Description = "Updated Description"
                    CreatedAt = createdAt
                    UpdatedAt = updatedAt } ]

            Assert.Equivalent(expected, actual)
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
                |> Seeder.saveChangesAsync

            do!
                fixture.Seeder
                |> Seeder.addCollections
                    [ { Id = 3
                        UserId = 202
                        Name = "Collection 3"
                        Description = null
                        CreatedAt = createdAt
                        UpdatedAt = createdAt } ]
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

            let! actual =
                fixture.Seeder
                |> Seeder.getAllVocabulariesAsync

            Assert.Empty(actual)
        }
