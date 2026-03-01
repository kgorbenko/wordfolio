namespace Wordfolio.Api.DataAccess.Tests

open System

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type MoveEntryTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``moveEntryAsync updates vocabulary and updated at``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let updatedAt =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 317

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let sourceVocabulary =
                Entities.makeVocabulary collection "Source" None createdAt None false

            let targetVocabulary =
                Entities.makeVocabulary collection "Target" None createdAt None false

            let entry =
                Entities.makeEntry sourceVocabulary "move-me" createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ sourceVocabulary; targetVocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.saveChangesAsync

            let! affectedRows =
                Entries.moveEntryAsync
                    { Id = entry.Id
                      OldVocabularyId = sourceVocabulary.Id
                      NewVocabularyId = targetVocabulary.Id
                      UpdatedAt = updatedAt }
                |> fixture.WithConnectionAsync

            Assert.Equal(1, affectedRows)

            let! actual = Seeder.getEntryByIdAsync entry.Id fixture.Seeder

            let expected: Entry option =
                Some
                    { Id = entry.Id
                      VocabularyId = targetVocabulary.Id
                      EntryText = "move-me"
                      CreatedAt = createdAt
                      UpdatedAt = Some updatedAt }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``moveEntryAsync returns 0 when entry does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let updatedAt =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let! affectedRows =
                Entries.moveEntryAsync
                    { Id = 999
                      OldVocabularyId = 1000
                      NewVocabularyId = 1001
                      UpdatedAt = updatedAt }
                |> fixture.WithConnectionAsync

            Assert.Equal(0, affectedRows)
        }

    [<Fact>]
    member _.``moveEntryAsync returns 0 when old vocabulary does not match``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let updatedAt =
                DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 318

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let sourceVocabulary =
                Entities.makeVocabulary collection "Source" None createdAt None false

            let targetVocabulary =
                Entities.makeVocabulary collection "Target" None createdAt None false

            let anotherVocabulary =
                Entities.makeVocabulary collection "Another" None createdAt None false

            let entry =
                Entities.makeEntry sourceVocabulary "move-me" createdAt None

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ sourceVocabulary; targetVocabulary; anotherVocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.saveChangesAsync

            let! affectedRows =
                Entries.moveEntryAsync
                    { Id = entry.Id
                      OldVocabularyId = anotherVocabulary.Id
                      NewVocabularyId = targetVocabulary.Id
                      UpdatedAt = updatedAt }
                |> fixture.WithConnectionAsync

            Assert.Equal(0, affectedRows)

            let! actual = Seeder.getEntryByIdAsync entry.Id fixture.Seeder

            let expected: Entry option =
                Some
                    { Id = entry.Id
                      VocabularyId = sourceVocabulary.Id
                      EntryText = "move-me"
                      CreatedAt = createdAt
                      UpdatedAt = None }

            Assert.Equal(expected, actual)
        }
