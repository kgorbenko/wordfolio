namespace Wordfolio.Api.DataAccess.Tests.Entries

open System

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type UpdateEntryTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

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
    member _.``updateEntryAsync only updates targeted entry``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let updatedAt =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 325

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            let targetEntry =
                Entities.makeEntry vocabulary "target-original" createdAt None

            let untouchedEntry =
                Entities.makeEntry vocabulary "untouched" createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! affectedRows =
                Entries.updateEntryAsync
                    { Id = targetEntry.Id
                      EntryText = "target-updated"
                      UpdatedAt = updatedAt }
                |> fixture.WithConnectionAsync

            Assert.Equal(1, affectedRows)

            let! actualTargetEntry = Seeder.getEntryByIdAsync targetEntry.Id fixture.Seeder
            let! actualUntouchedEntry = Seeder.getEntryByIdAsync untouchedEntry.Id fixture.Seeder

            let expectedTargetEntry: Entry option =
                Some
                    { Id = targetEntry.Id
                      VocabularyId = vocabulary.Id
                      EntryText = "target-updated"
                      CreatedAt = createdAt
                      UpdatedAt = Some updatedAt }

            let expectedUntouchedEntry: Entry option =
                Some
                    { Id = untouchedEntry.Id
                      VocabularyId = vocabulary.Id
                      EntryText = "untouched"
                      CreatedAt = createdAt
                      UpdatedAt = None }

            Assert.Equal(expectedTargetEntry, actualTargetEntry)
            Assert.Equal(expectedUntouchedEntry, actualUntouchedEntry)
        }
