namespace Wordfolio.Api.DataAccess.Tests.Vocabularies

open System

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type UpdateVocabularyTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

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
                Entities.makeCollection user "Collection 2" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Original Name" (Some "Original Description") createdAt None false

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

            let expected: Vocabulary option =
                Some
                    { Id = vocabulary.Id
                      CollectionId = collection.Id
                      Name = "Updated Name"
                      Description = Some "Updated Description"
                      CreatedAt = createdAt
                      UpdatedAt = Some updatedAt
                      IsDefault = false }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``updateVocabularyAsync can clear description by setting it to None``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let updatedAt =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 204

            let collection =
                Entities.makeCollection user "Collection 4" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary Name" (Some "Original Description") createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! affectedRows =
                Vocabularies.updateVocabularyAsync
                    { Id = vocabulary.Id
                      Name = "Vocabulary Name"
                      Description = None
                      UpdatedAt = updatedAt }
                |> fixture.WithConnectionAsync

            Assert.Equal(1, affectedRows)

            let! actual = Seeder.getVocabularyByIdAsync vocabulary.Id fixture.Seeder

            let expected: Vocabulary option =
                Some
                    { Id = vocabulary.Id
                      CollectionId = collection.Id
                      Name = "Vocabulary Name"
                      Description = None
                      CreatedAt = createdAt
                      UpdatedAt = Some updatedAt
                      IsDefault = false }

            Assert.Equal(expected, actual)
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
    member _.``updateVocabularyAsync returns 0 when vocabulary is default``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let updatedAt =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let defaultVocabulary =
                Entities.makeVocabulary collection "Default" None createdAt None true

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ defaultVocabulary ]
                |> Seeder.saveChangesAsync

            let! affectedRows =
                Vocabularies.updateVocabularyAsync
                    { Id = defaultVocabulary.Id
                      Name = "Updated Name"
                      Description = Some "Updated Description"
                      UpdatedAt = updatedAt }
                |> fixture.WithConnectionAsync

            Assert.Equal(0, affectedRows)

            let! actual = Seeder.getVocabularyByIdAsync defaultVocabulary.Id fixture.Seeder

            let expected: Vocabulary option =
                Some
                    { Id = defaultVocabulary.Id
                      CollectionId = collection.Id
                      Name = "Default"
                      Description = None
                      CreatedAt = createdAt
                      UpdatedAt = None
                      IsDefault = true }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``updateVocabularyAsync returns 0 when vocabulary is in system collection``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let updatedAt =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let systemCollection =
                Entities.makeCollection user "Unsorted" None createdAt None true

            let vocabulary =
                Entities.makeVocabulary systemCollection "Vocabulary" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ systemCollection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            let! affectedRows =
                Vocabularies.updateVocabularyAsync
                    { Id = vocabulary.Id
                      Name = "Updated Name"
                      Description = Some "Updated Description"
                      UpdatedAt = updatedAt }
                |> fixture.WithConnectionAsync

            Assert.Equal(0, affectedRows)

            let! actual = Seeder.getVocabularyByIdAsync vocabulary.Id fixture.Seeder

            let expected: Vocabulary option =
                Some
                    { Id = vocabulary.Id
                      CollectionId = systemCollection.Id
                      Name = "Vocabulary"
                      Description = None
                      CreatedAt = createdAt
                      UpdatedAt = None
                      IsDefault = false }

            Assert.Equal(expected, actual)
        }
