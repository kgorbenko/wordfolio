namespace Wordfolio.Api.DataAccess.Tests.Vocabularies

open System
open System.Threading.Tasks

open Npgsql
open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.DataAccess.Tests
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type CreateVocabularyTests(fixture: WordfolioTestFixture) =
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
                Assert.ThrowsAsync<PostgresException>(fun () ->
                    (Vocabularies.createVocabularyAsync
                        { CollectionId = 999
                          Name = "My Vocabulary"
                          Description = Some "Test vocabulary"
                          CreatedAt = createdAt }
                     |> fixture.WithConnectionAsync
                    :> Task))

            Assert.Equal(SqlErrorCodes.ForeignKeyViolation, ex.SqlState)
        }
