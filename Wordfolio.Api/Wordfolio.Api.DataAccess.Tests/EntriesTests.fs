namespace Wordfolio.Api.DataAccess.Tests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type EntriesTests(fixture: WordfolioTestFixture) =
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
                Assert.ThrowsAsync<Npgsql.PostgresException>(fun () ->
                    (Entries.createEntryAsync
                        { VocabularyId = 999
                          EntryText = "serendipity"
                          CreatedAt = createdAt }
                     |> fixture.WithConnectionAsync
                    :> Task))

            Assert.Equal(SqlErrorCodes.ForeignKeyViolation, ex.SqlState)
        }

    [<Fact>]
    member _.``getEntryByIdAsync returns entry when it exists``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 301

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "ephemeral" createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Entries.getEntryByIdAsync entry.Id
                |> fixture.WithConnectionAsync

            let expected: Entries.Entry option =
                Some
                    { Id = entry.Id
                      VocabularyId = vocabulary.Id
                      EntryText = "ephemeral"
                      CreatedAt = createdAt
                      UpdatedAt = None }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``getEntryByIdAsync returns None when entry does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! actual =
                Entries.getEntryByIdAsync 999
                |> fixture.WithConnectionAsync

            Assert.Equal(None, actual)
        }

    [<Fact>]
    member _.``getEntriesByVocabularyIdAsync returns entries for vocabulary``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 302

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary1 =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let vocabulary2 =
                Entities.makeVocabulary collection "Vocabulary 2" None createdAt None false

            let entry1 =
                Entities.makeEntry vocabulary1 "ubiquitous" createdAt None

            let entry2 =
                Entities.makeEntry vocabulary1 "meticulous" createdAt None

            let _ =
                Entities.makeEntry vocabulary2 "tenacious" createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Entries.getEntriesByVocabularyIdAsync vocabulary1.Id
                |> fixture.WithConnectionAsync

            let expected: Entries.Entry list =
                [ { Id = entry1.Id
                    VocabularyId = vocabulary1.Id
                    EntryText = "ubiquitous"
                    CreatedAt = createdAt
                    UpdatedAt = None }
                  { Id = entry2.Id
                    VocabularyId = vocabulary1.Id
                    EntryText = "meticulous"
                    CreatedAt = createdAt
                    UpdatedAt = None } ]

            Assert.Equivalent(expected, actual)
        }

    [<Fact>]
    member _.``getEntriesByVocabularyIdAsync returns empty list when vocabulary has no entries``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 303

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Entries.getEntriesByVocabularyIdAsync vocabulary.Id
                |> fixture.WithConnectionAsync

            Assert.Empty(actual)
        }

    [<Fact>]
    member _.``updateEntryAsync updates an existing entry``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let updatedAt =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 304

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "original" createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! affectedRows =
                Entries.updateEntryAsync
                    { Id = entry.Id
                      EntryText = "updated"
                      UpdatedAt = updatedAt }
                |> fixture.WithConnectionAsync

            Assert.Equal(1, affectedRows)

            let! actual = Seeder.getEntryByIdAsync entry.Id fixture.Seeder

            let expected: Entry option =
                Some
                    { Id = entry.Id
                      VocabularyId = vocabulary.Id
                      EntryText = "updated"
                      CreatedAt = createdAt
                      UpdatedAt = Some updatedAt }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``updateEntryAsync returns 0 when entry does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let updatedAt =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let! affectedRows =
                Entries.updateEntryAsync
                    { Id = 999
                      EntryText = "updated"
                      UpdatedAt = updatedAt }
                |> fixture.WithConnectionAsync

            Assert.Equal(0, affectedRows)
        }

    [<Fact>]
    member _.``deleteEntryAsync deletes an existing entry``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 305

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry =
                Entities.makeEntry vocabulary "deleteme" createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! affectedRows =
                Entries.deleteEntryAsync entry.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(1, affectedRows)

            let! actual =
                fixture.Seeder
                |> Seeder.getAllEntriesAsync

            Assert.Empty(actual)
        }

    [<Fact>]
    member _.``deleteEntryAsync returns 0 when entry does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! affectedRows =
                Entries.deleteEntryAsync 999
                |> fixture.WithConnectionAsync

            Assert.Equal(0, affectedRows)
        }

    [<Fact>]
    member _.``deleting vocabulary cascades to delete entries``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 306

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let entry1 =
                Entities.makeEntry vocabulary "cascade1" createdAt None

            let entry2 =
                Entities.makeEntry vocabulary "cascade2" createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! _ =
                Vocabularies.deleteVocabularyAsync vocabulary.Id
                |> fixture.WithConnectionAsync

            let! actualEntries =
                fixture.Seeder
                |> Seeder.getAllEntriesAsync

            Assert.Empty(actualEntries)
        }

    [<Fact>]
    member _.``getEntryByTextAndVocabularyIdAsync returns entry when it exists``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 307

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

            let! actual =
                Entries.getEntryByTextAndVocabularyIdAsync vocabulary.Id "serendipity"
                |> fixture.WithConnectionAsync

            let expected: Entries.Entry option =
                Some
                    { Id = entry.Id
                      VocabularyId = vocabulary.Id
                      EntryText = "serendipity"
                      CreatedAt = createdAt
                      UpdatedAt = None }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``getEntryByTextAndVocabularyIdAsync returns None when entry text does not match``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 308

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let _ =
                Entities.makeEntry vocabulary "ephemeral" createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Entries.getEntryByTextAndVocabularyIdAsync vocabulary.Id "serendipity"
                |> fixture.WithConnectionAsync

            Assert.Equal(None, actual)
        }

    [<Fact>]
    member _.``getEntryByTextAndVocabularyIdAsync returns None when vocabulary does not match``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 309

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary1 =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let vocabulary2 =
                Entities.makeVocabulary collection "Vocabulary 2" None createdAt None false

            let _ =
                Entities.makeEntry vocabulary1 "serendipity" createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Entries.getEntryByTextAndVocabularyIdAsync vocabulary2.Id "serendipity"
                |> fixture.WithConnectionAsync

            Assert.Equal(None, actual)
        }

    [<Fact>]
    member _.``getEntryByTextAndVocabularyIdAsync returns None when both vocabulary and entry text do not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! actual =
                Entries.getEntryByTextAndVocabularyIdAsync 999 "nonexistent"
                |> fixture.WithConnectionAsync

            Assert.Equal(None, actual)
        }

    [<Fact>]
    member _.``hasVocabularyAccessAsync returns true when vocabulary belongs to user``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 310

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" (Some "Description") createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Entries.hasVocabularyAccessAsync vocabulary.Id user.Id
                |> fixture.WithConnectionAsync

            Assert.True(actual)
        }

    [<Fact>]
    member _.``hasVocabularyAccessAsync returns false when vocabulary does not belong to user``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user1 = Entities.makeUser 311
            let user2 = Entities.makeUser 312

            let collection =
                Entities.makeCollection user1 "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user1; user2 ]
                |> Seeder.saveChangesAsync

            let! actual =
                Entries.hasVocabularyAccessAsync vocabulary.Id user2.Id
                |> fixture.WithConnectionAsync

            Assert.False(actual)
        }

    [<Fact>]
    member _.``hasVocabularyAccessAsync returns false when vocabulary does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let user = Entities.makeUser 313

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Entries.hasVocabularyAccessAsync 999 user.Id
                |> fixture.WithConnectionAsync

            Assert.False(actual)
        }

    [<Fact>]
    member _.``hasVocabularyAccessAsync returns false when user does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 314

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Entries.hasVocabularyAccessAsync vocabulary.Id 999
                |> fixture.WithConnectionAsync

            Assert.False(actual)
        }

    [<Fact>]
    member _.``hasVocabularyAccessAsync returns true when vocabulary is default``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 315

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
                Entries.hasVocabularyAccessAsync defaultVocabulary.Id user.Id
                |> fixture.WithConnectionAsync

            Assert.True(actual)
        }

    [<Fact>]
    member _.``hasVocabularyAccessAsync returns true when vocabulary is in system collection``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 316

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
                Entries.hasVocabularyAccessAsync vocabulary.Id user.Id
                |> fixture.WithConnectionAsync

            Assert.True(actual)
        }
