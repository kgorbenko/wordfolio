namespace Wordfolio.Api.DataAccess.Tests.Vocabularies

open System
open System.Threading.Tasks

open Npgsql
open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.DataAccess.Tests
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type CreateDefaultVocabularyTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

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
    member _.``createDefaultVocabularyAsync fails with foreign key violation for non-existent collection``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let! ex =
                Assert.ThrowsAsync<PostgresException>(fun () ->
                    (Vocabularies.createDefaultVocabularyAsync
                        { CollectionId = 999
                          Name = "Unsorted"
                          Description = None
                          CreatedAt = createdAt }
                     |> fixture.WithConnectionAsync
                    :> Task))

            Assert.Equal(SqlErrorCodes.ForeignKeyViolation, ex.SqlState)
        }
