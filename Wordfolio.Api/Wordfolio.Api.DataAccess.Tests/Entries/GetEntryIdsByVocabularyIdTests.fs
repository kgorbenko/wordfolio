namespace Wordfolio.Api.DataAccess.Tests.Entries

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type GetEntryIdsByVocabularyIdTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``getEntryIdsByVocabularyIdAsync returns empty list when vocabulary has no entries``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 780

            let collection =
                Entities.makeCollection user "Collection" None createdAt createdAt false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None createdAt createdAt false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Entries.getEntryIdsByVocabularyIdAsync vocabulary.Id user.Id
                |> fixture.WithConnectionAsync

            Assert.Empty(actual)
        }

    [<Fact>]
    member _.``getEntryIdsByVocabularyIdAsync returns entry IDs for owned vocabulary``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 781

            let collection =
                Entities.makeCollection user "Collection" None createdAt createdAt false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None createdAt createdAt false

            let entry1 =
                Entities.makeEntry vocabulary "word1" createdAt createdAt

            let entry2 =
                Entities.makeEntry vocabulary "word2" createdAt createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Entries.getEntryIdsByVocabularyIdAsync vocabulary.Id user.Id
                |> fixture.WithConnectionAsync

            Assert.Equivalent([ entry1.Id; entry2.Id ], actual)
        }

    [<Fact>]
    member _.``getEntryIdsByVocabularyIdAsync does not return entries from unowned vocabulary``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let owner = Entities.makeUser 782
            let other = Entities.makeUser 783

            let ownerCollection =
                Entities.makeCollection owner "Owner Collection" None createdAt createdAt false

            let ownerVocabulary =
                Entities.makeVocabulary ownerCollection "Owner Vocabulary" None createdAt createdAt false

            let _ =
                Entities.makeEntry ownerVocabulary "word" createdAt createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ owner; other ]
                |> Seeder.saveChangesAsync

            let! actual =
                Entries.getEntryIdsByVocabularyIdAsync ownerVocabulary.Id other.Id
                |> fixture.WithConnectionAsync

            Assert.Empty(actual)
        }

    [<Fact>]
    member _.``getEntryIdsByVocabularyIdAsync does not return entries from other vocabularies``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 784

            let collection =
                Entities.makeCollection user "Collection" None createdAt createdAt false

            let vocabulary1 =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt createdAt false

            let vocabulary2 =
                Entities.makeVocabulary collection "Vocabulary 2" None createdAt createdAt false

            let entry1 =
                Entities.makeEntry vocabulary1 "word1" createdAt createdAt

            let _ =
                Entities.makeEntry vocabulary2 "word2" createdAt createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Entries.getEntryIdsByVocabularyIdAsync vocabulary1.Id user.Id
                |> fixture.WithConnectionAsync

            Assert.Equivalent([ entry1.Id ], actual)
        }
