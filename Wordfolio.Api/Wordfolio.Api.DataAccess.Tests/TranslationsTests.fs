namespace Wordfolio.Api.DataAccess.Tests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type TranslationsTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``createTranslationsAsync inserts multiple translations``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 500

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None

            let entry =
                Entities.makeEntry vocabulary "serendipity" createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let parameters: Translations.TranslationCreationParameters list =
                [ { EntryId = entry.Id
                    TranslationText = "счастливая случайность"
                    Source = Translations.TranslationSource.Manual
                    DisplayOrder = 1 }
                  { EntryId = entry.Id
                    TranslationText = "серендипность"
                    Source = Translations.TranslationSource.Manual
                    DisplayOrder = 2 }
                  { EntryId = entry.Id
                    TranslationText = "удача"
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
                    TranslationText = "счастливая случайность"
                    Source = Translations.TranslationSource.Manual
                    DisplayOrder = 1 }
                  { Id = createdIds.[1]
                    EntryId = entry.Id
                    TranslationText = "серендипность"
                    Source = Translations.TranslationSource.Manual
                    DisplayOrder = 2 }
                  { Id = createdIds.[2]
                    EntryId = entry.Id
                    TranslationText = "удача"
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

            let parameters: Translations.TranslationCreationParameters list =
                [ { EntryId = 999
                    TranslationText = "Test translation"
                    Source = Translations.TranslationSource.Manual
                    DisplayOrder = 1 } ]

            let! ex =
                Assert.ThrowsAsync<Npgsql.PostgresException>(fun () ->
                    (Translations.createTranslationsAsync parameters
                     |> fixture.WithConnectionAsync
                    :> Task))

            Assert.Equal("23503", ex.SqlState)
        }

    [<Fact>]
    member _.``createTranslationsAsync fails with unique constraint violation for duplicate DisplayOrder``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 501

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None

            let entry =
                Entities.makeEntry vocabulary "test" createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let parameters: Translations.TranslationCreationParameters list =
                [ { EntryId = entry.Id
                    TranslationText = "Translation 1"
                    Source = Translations.TranslationSource.Manual
                    DisplayOrder = 1 }
                  { EntryId = entry.Id
                    TranslationText = "Translation 2"
                    Source = Translations.TranslationSource.Manual
                    DisplayOrder = 1 } ]

            let! ex =
                Assert.ThrowsAsync<Npgsql.PostgresException>(fun () ->
                    (Translations.createTranslationsAsync parameters
                     |> fixture.WithConnectionAsync
                    :> Task))

            Assert.Equal("23505", ex.SqlState)
        }

    [<Fact>]
    member _.``getTranslationsByEntryIdAsync returns translations ordered by DisplayOrder``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 502

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None

            let entry1 =
                Entities.makeEntry vocabulary "ephemeral" createdAt None

            let entry2 =
                Entities.makeEntry vocabulary "ubiquitous" createdAt None

            let trans1 =
                Entities.makeTranslation entry1 "Translation 1" Translations.TranslationSource.Manual 2

            let trans2 =
                Entities.makeTranslation entry1 "Translation 2" Translations.TranslationSource.Manual 1

            let trans3 =
                Entities.makeTranslation entry1 "Translation 3" Translations.TranslationSource.Api 3

            let _ =
                Entities.makeTranslation entry2 "Other translation" Translations.TranslationSource.Manual 1

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Translations.getTranslationsByEntryIdAsync entry1.Id
                |> fixture.WithConnectionAsync

            let expected: Translations.Translation list =
                [ { Id = trans2.Id
                    EntryId = entry1.Id
                    TranslationText = "Translation 2"
                    Source = Translations.TranslationSource.Manual
                    DisplayOrder = 1 }
                  { Id = trans1.Id
                    EntryId = entry1.Id
                    TranslationText = "Translation 1"
                    Source = Translations.TranslationSource.Manual
                    DisplayOrder = 2 }
                  { Id = trans3.Id
                    EntryId = entry1.Id
                    TranslationText = "Translation 3"
                    Source = Translations.TranslationSource.Api
                    DisplayOrder = 3 } ]

            Assert.Equal<Translations.Translation list>(expected, actual)
        }

    [<Fact>]
    member _.``getTranslationsByEntryIdAsync returns empty list when entry has no translations``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 503

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None

            let entry =
                Entities.makeEntry vocabulary "test" createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Translations.getTranslationsByEntryIdAsync entry.Id
                |> fixture.WithConnectionAsync

            Assert.Empty(actual)
        }

    [<Fact>]
    member _.``updateTranslationsAsync updates multiple translations``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 504

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None

            let entry =
                Entities.makeEntry vocabulary "test" createdAt None

            let trans1 =
                Entities.makeTranslation entry "Original 1" Translations.TranslationSource.Manual 1

            let trans2 =
                Entities.makeTranslation entry "Original 2" Translations.TranslationSource.Manual 2

            let trans3 =
                Entities.makeTranslation entry "Original 3" Translations.TranslationSource.Manual 3

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let parameters: Translations.TranslationUpdateParameters list =
                [ { Id = trans1.Id
                    TranslationText = "Updated 1"
                    Source = Translations.TranslationSource.Api
                    DisplayOrder = 9 }
                  { Id = trans2.Id
                    TranslationText = "Updated 2"
                    Source = Translations.TranslationSource.Api
                    DisplayOrder = 8 } ]

            let! affectedRows =
                Translations.updateTranslationsAsync parameters
                |> fixture.WithConnectionAsync

            Assert.Equal(2, affectedRows)

            let! actual =
                fixture.Seeder
                |> Seeder.getAllTranslationsAsync

            let expected: Translation list =
                [ { Id = trans1.Id
                    EntryId = entry.Id
                    TranslationText = "Updated 1"
                    Source = Translations.TranslationSource.Api
                    DisplayOrder = 9 }
                  { Id = trans2.Id
                    EntryId = entry.Id
                    TranslationText = "Updated 2"
                    Source = Translations.TranslationSource.Api
                    DisplayOrder = 8 }
                  { Id = trans3.Id
                    EntryId = entry.Id
                    TranslationText = "Original 3"
                    Source = Translations.TranslationSource.Manual
                    DisplayOrder = 3 } ]

            Assert.Equivalent(expected, actual)
        }

    [<Fact>]
    member _.``updateTranslationsAsync returns 0 when given empty list``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! affectedRows =
                Translations.updateTranslationsAsync []
                |> fixture.WithConnectionAsync

            Assert.Equal(0, affectedRows)
        }

    [<Fact>]
    member _.``updateTranslationsAsync returns 0 for non-existent translations``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let parameters: Translations.TranslationUpdateParameters list =
                [ { Id = 999
                    TranslationText = "Updated"
                    Source = Translations.TranslationSource.Api
                    DisplayOrder = 1 } ]

            let! affectedRows =
                Translations.updateTranslationsAsync parameters
                |> fixture.WithConnectionAsync

            Assert.Equal(0, affectedRows)
        }

    [<Fact>]
    member _.``deleteTranslationsAsync deletes multiple translations``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 505

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None

            let entry =
                Entities.makeEntry vocabulary "test" createdAt None

            let trans1 =
                Entities.makeTranslation entry "Translation 1" Translations.TranslationSource.Manual 1

            let trans2 =
                Entities.makeTranslation entry "Translation 2" Translations.TranslationSource.Manual 2

            let trans3 =
                Entities.makeTranslation entry "Translation 3" Translations.TranslationSource.Manual 3

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! affectedRows =
                Translations.deleteTranslationsAsync [ trans1.Id; trans2.Id ]
                |> fixture.WithConnectionAsync

            Assert.Equal(2, affectedRows)

            let! actual =
                fixture.Seeder
                |> Seeder.getAllTranslationsAsync

            let expected: Translation list =
                [ { Id = trans3.Id
                    EntryId = entry.Id
                    TranslationText = "Translation 3"
                    Source = Translations.TranslationSource.Manual
                    DisplayOrder = 3 } ]

            Assert.Equivalent(expected, actual)
        }

    [<Fact>]
    member _.``deleteTranslationsAsync returns 0 when given empty list``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! affectedRows =
                Translations.deleteTranslationsAsync []
                |> fixture.WithConnectionAsync

            Assert.Equal(0, affectedRows)
        }

    [<Fact>]
    member _.``deleteTranslationsAsync returns 0 for non-existent translations``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! affectedRows =
                Translations.deleteTranslationsAsync [ 999; 1000 ]
                |> fixture.WithConnectionAsync

            Assert.Equal(0, affectedRows)
        }

    [<Fact>]
    member _.``deleting entry cascades to delete translations``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 506

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None

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
