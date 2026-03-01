namespace Wordfolio.Api.DataAccess.Tests

open System
open System.Threading.Tasks

open Npgsql
open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type CreateTranslationsTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``createTranslationsAsync inserts multiple translations``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 500

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "serendipity" createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let parameters: Translations.CreateTranslationParameters list =
                [ { EntryId = entry.Id
                    TranslationText = "happy accident"
                    Source = Translations.TranslationSource.Manual
                    DisplayOrder = 1 }
                  { EntryId = entry.Id
                    TranslationText = "fortunate discovery"
                    Source = Translations.TranslationSource.Manual
                    DisplayOrder = 2 }
                  { EntryId = entry.Id
                    TranslationText = "luck"
                    Source = Translations.TranslationSource.Api
                    DisplayOrder = 3 } ]

            let! createdIds =
                Translations.createTranslationsAsync parameters
                |> fixture.WithConnectionAsync

            Assert.Equal(3, createdIds.Length)

            let! actualTranslations =
                fixture.Seeder
                |> Seeder.getAllTranslationsAsync

            let expected: Translation list =
                [ { Id = createdIds.[0]
                    EntryId = entry.Id
                    TranslationText = "happy accident"
                    Source = Translations.TranslationSource.Manual
                    DisplayOrder = 1 }
                  { Id = createdIds.[1]
                    EntryId = entry.Id
                    TranslationText = "fortunate discovery"
                    Source = Translations.TranslationSource.Manual
                    DisplayOrder = 2 }
                  { Id = createdIds.[2]
                    EntryId = entry.Id
                    TranslationText = "luck"
                    Source = Translations.TranslationSource.Api
                    DisplayOrder = 3 } ]

            Assert.Equivalent(expected, actualTranslations)
        }

    [<Fact>]
    member _.``createTranslationsAsync returns empty list when given empty list``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! createdIds =
                Translations.createTranslationsAsync []
                |> fixture.WithConnectionAsync

            Assert.Empty(createdIds)
        }

    [<Fact>]
    member _.``createTranslationsAsync fails with foreign key violation for non-existent entry``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let parameters: Translations.CreateTranslationParameters list =
                [ { EntryId = 999
                    TranslationText = "Test translation"
                    Source = Translations.TranslationSource.Manual
                    DisplayOrder = 1 } ]

            let! ex =
                Assert.ThrowsAsync<PostgresException>(fun () ->
                    (Translations.createTranslationsAsync parameters
                     |> fixture.WithConnectionAsync
                    :> Task))

            Assert.Equal(SqlErrorCodes.ForeignKeyViolation, ex.SqlState)
        }

    [<Fact>]
    member _.``createTranslationsAsync fails with unique constraint violation for duplicate DisplayOrder``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 501

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "test" createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let parameters: Translations.CreateTranslationParameters list =
                [ { EntryId = entry.Id
                    TranslationText = "Translation 1"
                    Source = Translations.TranslationSource.Manual
                    DisplayOrder = 1 }
                  { EntryId = entry.Id
                    TranslationText = "Translation 2"
                    Source = Translations.TranslationSource.Manual
                    DisplayOrder = 1 } ]

            let! ex =
                Assert.ThrowsAsync<PostgresException>(fun () ->
                    (Translations.createTranslationsAsync parameters
                     |> fixture.WithConnectionAsync
                    :> Task))

            Assert.Equal(SqlErrorCodes.UniqueViolation, ex.SqlState)
        }

    [<Fact>]
    member _.``deleting entry cascades to delete translations``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 506

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "test" createdAt None

            let _ =
                Entities.makeTranslation entry "Translation 1" Translations.TranslationSource.Manual 1

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! _ =
                Entries.deleteEntryAsync entry.Id
                |> fixture.WithConnectionAsync

            let! actualTranslations =
                fixture.Seeder
                |> Seeder.getAllTranslationsAsync

            Assert.Empty(actualTranslations)
        }
