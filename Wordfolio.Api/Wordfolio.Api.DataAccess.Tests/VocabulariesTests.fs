namespace Wordfolio.Api.DataAccess.Tests

open System
open System.Threading.Tasks

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
                Entities.makeCollection user "Collection 1" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! createdId =
                Vocabularies.createVocabularyAsync
                    { CollectionId = collection.Id
                      Name = "My Vocabulary"
                      Description = Some "Test vocabulary"
                      CreatedAt = createdAt }
                |> fixture.WithConnectionAsync

            let! actualVocabulary =
                fixture.Seeder
                |> Seeder.getVocabularyByIdAsync createdId

            let expected: Vocabulary option =
                Some
                    { Id = createdId
                      CollectionId = collection.Id
                      Name = "My Vocabulary"
                      Description = Some "Test vocabulary"
                      CreatedAt = createdAt
                      UpdatedAt = None
                      IsDefault = false }

            Assert.Equivalent(expected, actualVocabulary)
        }

    [<Fact>]
    member _.``createVocabularyAsync inserts a vocabulary with None description``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 203

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! createdId =
                Vocabularies.createVocabularyAsync
                    { CollectionId = collection.Id
                      Name = "My Vocabulary"
                      Description = None
                      CreatedAt = createdAt }
                |> fixture.WithConnectionAsync

            let! actualVocabulary =
                fixture.Seeder
                |> Seeder.getVocabularyByIdAsync createdId

            let expected: Vocabulary option =
                Some
                    { Id = createdId
                      CollectionId = collection.Id
                      Name = "My Vocabulary"
                      Description = None
                      CreatedAt = createdAt
                      UpdatedAt = None
                      IsDefault = false }

            Assert.Equivalent(expected, actualVocabulary)
        }

    [<Fact>]
    member _.``createVocabularyAsync fails with foreign key violation for non-existent collection``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let! ex =
                Assert.ThrowsAsync<Npgsql.PostgresException>(fun () ->
                    (Vocabularies.createVocabularyAsync
                        { CollectionId = 999
                          Name = "My Vocabulary"
                          Description = Some "Test vocabulary"
                          CreatedAt = createdAt }
                     |> fixture.WithConnectionAsync
                    :> Task))

            Assert.Equal(SqlErrorCodes.ForeignKeyViolation, ex.SqlState)
        }

    [<Fact>]
    member _.``getVocabularyByIdAsync returns vocabulary when it exists``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 200

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "My Vocabulary" (Some "Test vocabulary") createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Vocabularies.getVocabularyByIdAsync vocabulary.Id
                |> fixture.WithConnectionAsync

            let expected: Vocabularies.Vocabulary option =
                Some
                    { Id = vocabulary.Id
                      CollectionId = collection.Id
                      Name = "My Vocabulary"
                      Description = Some "Test vocabulary"
                      CreatedAt = createdAt
                      UpdatedAt = None }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``getVocabularyByIdAsync returns None when vocabulary does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! actual =
                Vocabularies.getVocabularyByIdAsync 999
                |> fixture.WithConnectionAsync

            Assert.Equal(None, actual)
        }

    [<Fact>]
    member _.``getVocabulariesByCollectionIdAsync returns vocabularies for collection``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 200

            let collection1 =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let collection2 =
                Entities.makeCollection user "Collection 2" None createdAt None false

            let vocab1 =
                Entities.makeVocabulary collection1 "Vocab 1" None createdAt None false

            let vocab2 =
                Entities.makeVocabulary collection1 "Vocab 2" None createdAt None false

            let _ =
                Entities.makeVocabulary collection2 "Vocab 3" None createdAt None false

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
                Entities.makeCollection user "Collection 1" None createdAt None false

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
    member _.``deleteVocabularyAsync deletes an existing vocabulary``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 202

            let collection =
                Entities.makeCollection user "Collection 3" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary to delete" None createdAt None false

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

    [<Fact>]
    member _.``getVocabularyByIdAndUserIdAsync returns vocabulary when it belongs to user``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 205

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" (Some "Description") createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Vocabularies.getVocabularyByIdAndUserIdAsync vocabulary.Id user.Id
                |> fixture.WithConnectionAsync

            let expected: Vocabularies.Vocabulary option =
                Some
                    { Id = vocabulary.Id
                      CollectionId = collection.Id
                      Name = "Vocabulary 1"
                      Description = Some "Description"
                      CreatedAt = createdAt
                      UpdatedAt = None }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``getVocabularyByIdAndUserIdAsync returns None when vocabulary does not belong to user``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user1 = Entities.makeUser 206
            let user2 = Entities.makeUser 207

            let collection =
                Entities.makeCollection user1 "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user1; user2 ]
                |> Seeder.saveChangesAsync

            let! actual =
                Vocabularies.getVocabularyByIdAndUserIdAsync vocabulary.Id user2.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(None, actual)
        }

    [<Fact>]
    member _.``getVocabularyByIdAndUserIdAsync returns None when vocabulary does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let user = Entities.makeUser 208

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Vocabularies.getVocabularyByIdAndUserIdAsync 999 user.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(None, actual)
        }

    [<Fact>]
    member _.``getVocabularyByIdAndUserIdAsync returns None when user does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 209

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Vocabularies.getVocabularyByIdAndUserIdAsync vocabulary.Id 999
                |> fixture.WithConnectionAsync

            Assert.Equal(None, actual)
        }

    [<Fact>]
    member _.``getVocabularyByIdAsync returns None when vocabulary is default``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

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

            let! actual =
                Vocabularies.getVocabularyByIdAsync defaultVocabulary.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(None, actual)
        }

    [<Fact>]
    member _.``getVocabulariesByCollectionIdAsync filters out default vocabularies``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let defaultVocabulary =
                Entities.makeVocabulary collection "Default" None createdAt None true

            let regularVocabulary =
                Entities.makeVocabulary collection "Regular" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ defaultVocabulary; regularVocabulary ]
                |> Seeder.saveChangesAsync

            let! actual =
                Vocabularies.getVocabulariesByCollectionIdAsync collection.Id
                |> fixture.WithConnectionAsync

            let expected: Vocabularies.Vocabulary list =
                [ { Id = regularVocabulary.Id
                    CollectionId = collection.Id
                    Name = "Regular"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None } ]

            Assert.Equal<Vocabularies.Vocabulary list>(expected, actual)
        }

    [<Fact>]
    member _.``getVocabularyByIdAndUserIdAsync returns None when vocabulary is default``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

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

            let! actual =
                Vocabularies.getVocabularyByIdAndUserIdAsync defaultVocabulary.Id user.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(None, actual)
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
    member _.``deleteVocabularyAsync returns 0 when vocabulary is default``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

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
                Vocabularies.deleteVocabularyAsync defaultVocabulary.Id
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
    member _.``getVocabularyByIdAsync returns None when vocabulary is in system collection``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

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

            let! actual =
                Vocabularies.getVocabularyByIdAsync vocabulary.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(None, actual)
        }

    [<Fact>]
    member _.``getVocabulariesByCollectionIdAsync returns empty list when collection is system``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

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

            let! actual =
                Vocabularies.getVocabulariesByCollectionIdAsync systemCollection.Id
                |> fixture.WithConnectionAsync

            Assert.Empty(actual)
        }

    [<Fact>]
    member _.``getVocabularyByIdAndUserIdAsync returns None when vocabulary is in system collection``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

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

            let! actual =
                Vocabularies.getVocabularyByIdAndUserIdAsync vocabulary.Id user.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(None, actual)
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

    [<Fact>]
    member _.``deleteVocabularyAsync returns 0 when vocabulary is in system collection``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

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
                Vocabularies.deleteVocabularyAsync vocabulary.Id
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

    [<Fact>]
    member _.``getDefaultVocabularyByUserIdAsync returns default vocabulary when it exists``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let collection =
                Entities.makeCollection user "Collection" None createdAt None false

            let defaultVocabulary =
                Entities.makeVocabulary collection "Unsorted" None createdAt None true

            let regularVocabulary =
                Entities.makeVocabulary collection "Regular" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ defaultVocabulary; regularVocabulary ]
                |> Seeder.saveChangesAsync

            let! actual =
                Vocabularies.getDefaultVocabularyByUserIdAsync user.Id
                |> fixture.WithConnectionAsync

            let expected: Vocabularies.Vocabulary option =
                Some
                    { Id = defaultVocabulary.Id
                      CollectionId = collection.Id
                      Name = "Unsorted"
                      Description = None
                      CreatedAt = createdAt
                      UpdatedAt = None }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``getDefaultVocabularyByUserIdAsync returns None when no default vocabulary exists``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let collection =
                Entities.makeCollection user "Collection" None createdAt None false

            let regularVocabulary =
                Entities.makeVocabulary collection "Regular" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ regularVocabulary ]
                |> Seeder.saveChangesAsync

            let! actual =
                Vocabularies.getDefaultVocabularyByUserIdAsync user.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(None, actual)
        }

    [<Fact>]
    member _.``createDefaultVocabularyAsync creates default vocabulary``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let collection =
                Entities.makeCollection user "Collection" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.saveChangesAsync

            let! createdId =
                Vocabularies.createDefaultVocabularyAsync
                    { CollectionId = collection.Id
                      Name = "Unsorted"
                      Description = None
                      CreatedAt = createdAt }
                |> fixture.WithConnectionAsync

            let! actualVocabulary =
                fixture.Seeder
                |> Seeder.getVocabularyByIdAsync createdId

            let expected: Vocabulary option =
                Some
                    { Id = createdId
                      CollectionId = collection.Id
                      Name = "Unsorted"
                      Description = None
                      CreatedAt = createdAt
                      UpdatedAt = None
                      IsDefault = true }

            Assert.Equivalent(expected, actualVocabulary)
        }

    [<Fact>]
    member _.``getDefaultVocabularyByUserIdAsync throws when multiple default vocabularies exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let collection =
                Entities.makeCollection user "Collection" None createdAt None false

            let defaultVocabulary1 =
                Entities.makeVocabulary collection "Default 1" None createdAt None true

            let defaultVocabulary2 =
                Entities.makeVocabulary collection "Default 2" None createdAt None true

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ defaultVocabulary1; defaultVocabulary2 ]
                |> Seeder.saveChangesAsync

            let! ex =
                Assert.ThrowsAsync<System.Exception>(fun () ->
                    Vocabularies.getDefaultVocabularyByUserIdAsync user.Id
                    |> fixture.WithConnectionAsync
                    :> Task)

            Assert.Equal("Query returned more than one element when at most one was expected", ex.Message)
        }
