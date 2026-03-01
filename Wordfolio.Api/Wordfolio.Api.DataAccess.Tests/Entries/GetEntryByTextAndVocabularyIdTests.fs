namespace Wordfolio.Api.DataAccess.Tests.Entries

open System

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type GetEntryByTextAndVocabularyIdTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

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
