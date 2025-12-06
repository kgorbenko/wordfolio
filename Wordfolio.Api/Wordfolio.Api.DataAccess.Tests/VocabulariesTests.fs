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

            let! seeded =
                fixture.Seeder
                |> Seeder.addUsers [ { Id = 200 } ]
                |> Seeder.addCollections
                    [ { UserId = 200
                        Name = "Collection 1"
                        Description = None
                        CreatedAt = createdAt
                        UpdatedAt = None
                        Vocabularies = [] } ]
                |> Seeder.saveChangesAsync

            let collection = seeded.Collections.[0]

            do!
                Vocabularies.createVocabularyAsync
                    { CollectionId = collection.Id
                      Name = "My Vocabulary"
                      Description = Some "Test vocabulary"
                      CreatedAt = createdAt }
                |> fixture.WithConnectionAsync

            let! actual = fixture.Seeder |> Seeder.getAllVocabulariesAsync

            Assert.Single(actual) |> ignore

            let expected: Vocabulary =
                { Id = actual.[0].Id
                  CollectionId = collection.Id
                  Name = "My Vocabulary"
                  Description = Some "Test vocabulary"
                  CreatedAt = createdAt
                  UpdatedAt = None }

            Assert.Equivalent(expected, actual.[0])
        }

    [<Fact>]
    member _.``getVocabularyByIdAsync returns vocabulary when it exists``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.FromHours(0.0))

            let! seeded =
                fixture.Seeder
                |> Seeder.addUsers [ { Id = 200 } ]
                |> Seeder.addCollections
                    [ { UserId = 200
                        Name = "Collection 1"
                        Description = None
                        CreatedAt = createdAt
                        UpdatedAt = None
                        Vocabularies =
                            [ { Name = "My Vocabulary"
                                Description = Some "Test vocabulary"
                                CreatedAt = createdAt
                                UpdatedAt = None } ] } ]
                |> Seeder.saveChangesAsync

            let collection = seeded.Collections.[0]
            let vocabulary = seeded.Vocabularies.[0]

            let! actual =
                Vocabularies.getVocabularyByIdAsync vocabulary.Id
                |> fixture.WithConnectionAsync

            let expected: Vocabularies.Vocabulary =
                { Id = vocabulary.Id
                  CollectionId = collection.Id
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

            let! seeded =
                fixture.Seeder
                |> Seeder.addUsers [ { Id = 200 } ]
                |> Seeder.addCollections
                    [ { UserId = 200
                        Name = "Collection 1"
                        Description = None
                        CreatedAt = createdAt
                        UpdatedAt = None
                        Vocabularies =
                            [ { Name = "Vocab 1"
                                Description = None
                                CreatedAt = createdAt
                                UpdatedAt = None }
                              { Name = "Vocab 2"
                                Description = None
                                CreatedAt = createdAt
                                UpdatedAt = None } ] }
                      { UserId = 200
                        Name = "Collection 2"
                        Description = None
                        CreatedAt = createdAt
                        UpdatedAt = None
                        Vocabularies =
                            [ { Name = "Vocab 3"
                                Description = None
                                CreatedAt = createdAt
                                UpdatedAt = None } ] } ]
                |> Seeder.saveChangesAsync

            let collection1 = seeded.Collections.[0]

            let! actual =
                Vocabularies.getVocabulariesByCollectionIdAsync collection1.Id
                |> fixture.WithConnectionAsync

            let expected: Vocabularies.Vocabulary list =
                [ { Id = seeded.Vocabularies.[0].Id
                    CollectionId = collection1.Id
                    Name = "Vocab 1"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None }
                  { Id = seeded.Vocabularies.[1].Id
                    CollectionId = collection1.Id
                    Name = "Vocab 2"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None } ]

            Assert.Equivalent(expected, actual)
        }

    [<Fact>]
    member _.``getVocabulariesByCollectionIdAsync returns empty list when collection has no vocabularies``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.FromHours(0.0))

            let! seeded =
                fixture.Seeder
                |> Seeder.addUsers [ { Id = 200 } ]
                |> Seeder.addCollections
                    [ { UserId = 200
                        Name = "Collection 1"
                        Description = None
                        CreatedAt = createdAt
                        UpdatedAt = None
                        Vocabularies = [] } ]
                |> Seeder.saveChangesAsync

            let collection = seeded.Collections.[0]

            let! actual =
                Vocabularies.getVocabulariesByCollectionIdAsync collection.Id
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

            let! seeded =
                fixture.Seeder
                |> Seeder.addUsers [ { Id = 201 } ]
                |> Seeder.addCollections
                    [ { UserId = 201
                        Name = "Collection 2"
                        Description = None
                        CreatedAt = createdAt
                        UpdatedAt = None
                        Vocabularies =
                            [ { Name = "Original Name"
                                Description = Some "Original Description"
                                CreatedAt = createdAt
                                UpdatedAt = None } ] } ]
                |> Seeder.saveChangesAsync

            let collection = seeded.Collections.[0]
            let vocabulary = seeded.Vocabularies.[0]

            let! affectedRows =
                Vocabularies.updateVocabularyAsync
                    { Id = vocabulary.Id
                      Name = "Updated Name"
                      Description = Some "Updated Description"
                      UpdatedAt = updatedAt }
                |> fixture.WithConnectionAsync

            Assert.Equal(1, affectedRows)

            let! actual = fixture.Seeder |> Seeder.getAllVocabulariesAsync

            Assert.Single(actual) |> ignore

            let expected: Vocabulary =
                { Id = vocabulary.Id
                  CollectionId = collection.Id
                  Name = "Updated Name"
                  Description = Some "Updated Description"
                  CreatedAt = createdAt
                  UpdatedAt = Some updatedAt }

            Assert.Equivalent(expected, actual.[0])
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

            let! seeded =
                fixture.Seeder
                |> Seeder.addUsers [ { Id = 202 } ]
                |> Seeder.addCollections
                    [ { UserId = 202
                        Name = "Collection 3"
                        Description = None
                        CreatedAt = createdAt
                        UpdatedAt = None
                        Vocabularies =
                            [ { Name = "Vocabulary to delete"
                                Description = None
                                CreatedAt = createdAt
                                UpdatedAt = None } ] } ]
                |> Seeder.saveChangesAsync

            let vocabulary = seeded.Vocabularies.[0]

            let! affectedRows =
                Vocabularies.deleteVocabularyAsync vocabulary.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(1, affectedRows)

            let! actual = fixture.Seeder |> Seeder.getAllVocabulariesAsync

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
