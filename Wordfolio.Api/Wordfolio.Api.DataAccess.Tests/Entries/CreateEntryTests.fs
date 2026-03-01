namespace Wordfolio.Api.DataAccess.Tests.Entries

open System
open System.Threading.Tasks

open Npgsql
open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.DataAccess.Tests
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type CreateEntryTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``createEntryAsync inserts an entry``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 300

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! createdId =
                Entries.createEntryAsync
                    { VocabularyId = vocabulary.Id
                      EntryText = "serendipity"
                      CreatedAt = createdAt }
                |> fixture.WithConnectionAsync

            let! actualEntry =
                fixture.Seeder
                |> Seeder.getEntryByIdAsync createdId

            let expected: Entry option =
                Some
                    { Id = createdId
                      VocabularyId = vocabulary.Id
                      EntryText = "serendipity"
                      CreatedAt = createdAt
                      UpdatedAt = None }

            Assert.Equivalent(expected, actualEntry)
        }

    [<Fact>]
    member _.``createEntryAsync fails with foreign key violation for non-existent vocabulary``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let! ex =
                Assert.ThrowsAsync<PostgresException>(fun () ->
                    (Entries.createEntryAsync
                        { VocabularyId = 999
                          EntryText = "serendipity"
                          CreatedAt = createdAt }
                     |> fixture.WithConnectionAsync
                    :> Task))

            Assert.Equal(SqlErrorCodes.ForeignKeyViolation, ex.SqlState)
        }
