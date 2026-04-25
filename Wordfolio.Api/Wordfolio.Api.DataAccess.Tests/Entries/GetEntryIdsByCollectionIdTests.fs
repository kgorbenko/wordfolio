namespace Wordfolio.Api.DataAccess.Tests.Entries

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type GetEntryIdsByCollectionIdTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``getEntryIdsByCollectionIdAsync returns empty list when collection has no entries``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 790

            let collection =
                Entities.makeCollection user "Collection" None createdAt createdAt false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Entries.getEntryIdsByCollectionIdAsync collection.Id user.Id
                |> fixture.WithConnectionAsync

            Assert.Empty(actual)
        }

    [<Fact>]
    member _.``getEntryIdsByCollectionIdAsync returns entry IDs across all vocabularies in collection``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 791

            let collection =
                Entities.makeCollection user "Collection" None createdAt createdAt false

            let vocabulary1 =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt createdAt false

            let vocabulary2 =
                Entities.makeVocabulary collection "Vocabulary 2" None createdAt createdAt false

            let entry1 =
                Entities.makeEntry vocabulary1 "word1" createdAt createdAt

            let entry2 =
                Entities.makeEntry vocabulary2 "word2" createdAt createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Entries.getEntryIdsByCollectionIdAsync collection.Id user.Id
                |> fixture.WithConnectionAsync

            Assert.Equivalent([ entry1.Id; entry2.Id ], actual)
        }

    [<Fact>]
    member _.``getEntryIdsByCollectionIdAsync does not return entries from unowned collection``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let owner = Entities.makeUser 792
            let other = Entities.makeUser 793

            let ownerCollection =
                Entities.makeCollection owner "Owner Collection" None createdAt createdAt false

            let ownerVocabulary =
                Entities.makeVocabulary ownerCollection "Vocabulary" None createdAt createdAt false

            let _ =
                Entities.makeEntry ownerVocabulary "word" createdAt createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ owner; other ]
                |> Seeder.saveChangesAsync

            let! actual =
                Entries.getEntryIdsByCollectionIdAsync ownerCollection.Id other.Id
                |> fixture.WithConnectionAsync

            Assert.Empty(actual)
        }

    [<Fact>]
    member _.``getEntryIdsByCollectionIdAsync does not return entries from other collections``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 794

            let collection1 =
                Entities.makeCollection user "Collection 1" None createdAt createdAt false

            let collection2 =
                Entities.makeCollection user "Collection 2" None createdAt createdAt false

            let vocabulary1 =
                Entities.makeVocabulary collection1 "Vocabulary 1" None createdAt createdAt false

            let vocabulary2 =
                Entities.makeVocabulary collection2 "Vocabulary 2" None createdAt createdAt false

            let entry1 =
                Entities.makeEntry vocabulary1 "word1" createdAt createdAt

            let _ =
                Entities.makeEntry vocabulary2 "word2" createdAt createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Entries.getEntryIdsByCollectionIdAsync collection1.Id user.Id
                |> fixture.WithConnectionAsync

            Assert.Equivalent([ entry1.Id ], actual)
        }
