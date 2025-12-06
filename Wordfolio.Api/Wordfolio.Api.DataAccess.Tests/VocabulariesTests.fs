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
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 200

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            do!
                Vocabularies.createVocabularyAsync
                    { CollectionId = collection.Id
                      Name = "My Vocabulary"
                      Description = Some "Test vocabulary"
                      CreatedAt = createdAt }
                |> fixture.WithConnectionAsync

            let! actual =
                fixture.Seeder
                |> Seeder.getAllVocabulariesAsync

            let expected: Vocabulary list =
                [ { Id = actual.[0].Id
                    CollectionId = collection.Id
                    Name = "My Vocabulary"
                    Description = Some "Test vocabulary"
                    CreatedAt = createdAt
                    UpdatedAt = None } ]

            Assert.Equivalent(expected, actual)
        }

    [<Fact>]
    member _.``getVocabularyByIdAsync returns vocabulary when it exists``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 200

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None

            let vocabulary =
                Entities.makeVocabulary collection "My Vocabulary" (Some "Test vocabulary") createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

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
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 200

            let collection1 =
                Entities.makeCollection user "Collection 1" None createdAt None

            let collection2 =
                Entities.makeCollection user "Collection 2" None createdAt None

            let vocab1 =
                Entities.makeVocabulary collection1 "Vocab 1" None createdAt None

            let vocab2 =
                Entities.makeVocabulary collection1 "Vocab 2" None createdAt None

            let _ =
                Entities.makeVocabulary collection2 "Vocab 3" None createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Vocabularies.getVocabulariesByCollectionIdAsync collection1.Id
                |> fixture.WithConnectionAsync

            let expected: Vocabularies.Vocabulary list =
                [ { Id = vocab1.Id
                    CollectionId = collection1.Id
                    Name = "Vocab 1"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None }
                  { Id = vocab2.Id
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
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 200

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

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
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let updatedAt =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 201

            let collection =
                Entities.makeCollection user "Collection 2" None createdAt None

            let vocabulary =
                Entities.makeVocabulary collection "Original Name" (Some "Original Description") createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! affectedRows =
                Vocabularies.updateVocabularyAsync
                    { Id = vocabulary.Id
                      Name = "Updated Name"
                      Description = Some "Updated Description"
                      UpdatedAt = updatedAt }
                |> fixture.WithConnectionAsync

            Assert.Equal(1, affectedRows)

            let! actual = Seeder.getVocabularyByIdAsync vocabulary.Id fixture.Seeder

            let expected: Vocabulary =
                { Id = vocabulary.Id
                  CollectionId = collection.Id
                  Name = "Updated Name"
                  Description = Some "Updated Description"
                  CreatedAt = createdAt
                  UpdatedAt = Some updatedAt }

            Assert.Equal(Some expected, actual)
        }

    [<Fact>]
    member _.``updateVocabularyAsync returns 0 when vocabulary does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let updatedAt =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

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
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 202

            let collection =
                Entities.makeCollection user "Collection 3" None createdAt None

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary to delete" None createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! affectedRows =
                Vocabularies.deleteVocabularyAsync vocabulary.Id
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
