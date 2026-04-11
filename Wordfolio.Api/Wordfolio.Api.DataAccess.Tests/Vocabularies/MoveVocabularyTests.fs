namespace Wordfolio.Api.DataAccess.Tests.Vocabularies

open System
open System.Threading.Tasks

open Npgsql
open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.DataAccess.Tests
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type MoveVocabularyTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``moveVocabularyAsync updates CollectionId and UpdatedAt``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let updatedAt =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 400

            let sourceCollection =
                Entities.makeCollection user "Source" None createdAt createdAt false

            let targetCollection =
                Entities.makeCollection user "Target" None createdAt createdAt false

            let vocabulary =
                Entities.makeVocabulary sourceCollection "My Vocabulary" None createdAt createdAt false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ sourceCollection; targetCollection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            let! result =
                Vocabularies.moveVocabularyAsync
                    { Id = vocabulary.Id
                      OldCollectionId = sourceCollection.Id
                      NewCollectionId = targetCollection.Id
                      UpdatedAt = updatedAt }
                |> fixture.WithConnectionAsync

            Assert.Equal(Ok(), result)

            let! actual = Seeder.getVocabularyByIdAsync vocabulary.Id fixture.Seeder

            let expected: Vocabulary option =
                Some
                    { Id = vocabulary.Id
                      CollectionId = targetCollection.Id
                      Name = "My Vocabulary"
                      Description = None
                      CreatedAt = createdAt
                      UpdatedAt = updatedAt
                      IsDefault = false }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``moveVocabularyAsync returns Error when vocabulary does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let updatedAt =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let! result =
                Vocabularies.moveVocabularyAsync
                    { Id = 999
                      OldCollectionId = 1000
                      NewCollectionId = 1001
                      UpdatedAt = updatedAt }
                |> fixture.WithConnectionAsync

            Assert.Equal(Error(), result)
        }

    [<Fact>]
    member _.``moveVocabularyAsync returns Error when OldCollectionId does not match``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let updatedAt =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 401

            let sourceCollection =
                Entities.makeCollection user "Source" None createdAt createdAt false

            let targetCollection =
                Entities.makeCollection user "Target" None createdAt createdAt false

            let otherCollection =
                Entities.makeCollection user "Other" None createdAt createdAt false

            let vocabulary =
                Entities.makeVocabulary sourceCollection "My Vocabulary" None createdAt createdAt false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ sourceCollection; targetCollection; otherCollection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            let! result =
                Vocabularies.moveVocabularyAsync
                    { Id = vocabulary.Id
                      OldCollectionId = otherCollection.Id
                      NewCollectionId = targetCollection.Id
                      UpdatedAt = updatedAt }
                |> fixture.WithConnectionAsync

            Assert.Equal(Error(), result)

            let! actual = Seeder.getVocabularyByIdAsync vocabulary.Id fixture.Seeder

            let expected: Vocabulary option =
                Some
                    { Id = vocabulary.Id
                      CollectionId = sourceCollection.Id
                      Name = "My Vocabulary"
                      Description = None
                      CreatedAt = createdAt
                      UpdatedAt = createdAt
                      IsDefault = false }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``moveVocabularyAsync fails with foreign key violation when target collection does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let updatedAt =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 402

            let sourceCollection =
                Entities.makeCollection user "Source" None createdAt createdAt false

            let vocabulary =
                Entities.makeVocabulary sourceCollection "My Vocabulary" None createdAt createdAt false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ sourceCollection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            let! ex =
                Assert.ThrowsAsync<PostgresException>(fun () ->
                    (Vocabularies.moveVocabularyAsync
                        { Id = vocabulary.Id
                          OldCollectionId = sourceCollection.Id
                          NewCollectionId = 999999
                          UpdatedAt = updatedAt }
                     |> fixture.WithConnectionAsync
                    :> Task))

            Assert.Equal(SqlErrorCodes.ForeignKeyViolation, ex.SqlState)

            let! actual = Seeder.getVocabularyByIdAsync vocabulary.Id fixture.Seeder

            let expected: Vocabulary option =
                Some
                    { Id = vocabulary.Id
                      CollectionId = sourceCollection.Id
                      Name = "My Vocabulary"
                      Description = None
                      CreatedAt = createdAt
                      UpdatedAt = createdAt
                      IsDefault = false }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``moveVocabularyAsync returns Error when target collection is system``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let updatedAt =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 404

            let sourceCollection =
                Entities.makeCollection user "Source" None createdAt createdAt false

            let systemCollection =
                Entities.makeCollection user "System" None createdAt createdAt true

            let vocabulary =
                Entities.makeVocabulary sourceCollection "My Vocabulary" None createdAt createdAt false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ sourceCollection; systemCollection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            let! result =
                Vocabularies.moveVocabularyAsync
                    { Id = vocabulary.Id
                      OldCollectionId = sourceCollection.Id
                      NewCollectionId = systemCollection.Id
                      UpdatedAt = updatedAt }
                |> fixture.WithConnectionAsync

            Assert.Equal(Error(), result)

            let! actual = Seeder.getVocabularyByIdAsync vocabulary.Id fixture.Seeder

            let expected: Vocabulary option =
                Some
                    { Id = vocabulary.Id
                      CollectionId = sourceCollection.Id
                      Name = "My Vocabulary"
                      Description = None
                      CreatedAt = createdAt
                      UpdatedAt = createdAt
                      IsDefault = false }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``moveVocabularyAsync returns Error when vocabulary is default``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let updatedAt =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 403

            let sourceCollection =
                Entities.makeCollection user "Source" None createdAt createdAt false

            let targetCollection =
                Entities.makeCollection user "Target" None createdAt createdAt false

            let defaultVocabulary =
                Entities.makeVocabulary sourceCollection "Default" None createdAt createdAt true

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ sourceCollection; targetCollection ]
                |> Seeder.addVocabularies [ defaultVocabulary ]
                |> Seeder.saveChangesAsync

            let! result =
                Vocabularies.moveVocabularyAsync
                    { Id = defaultVocabulary.Id
                      OldCollectionId = sourceCollection.Id
                      NewCollectionId = targetCollection.Id
                      UpdatedAt = updatedAt }
                |> fixture.WithConnectionAsync

            Assert.Equal(Error(), result)

            let! actual = Seeder.getVocabularyByIdAsync defaultVocabulary.Id fixture.Seeder

            let expected: Vocabulary option =
                Some
                    { Id = defaultVocabulary.Id
                      CollectionId = sourceCollection.Id
                      Name = "Default"
                      Description = None
                      CreatedAt = createdAt
                      UpdatedAt = createdAt
                      IsDefault = true }

            Assert.Equal(expected, actual)
        }
