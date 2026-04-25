namespace Wordfolio.Api.DataAccess.Tests.Entries

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type GetEntryIdsByIdsForUserTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``getEntryIdsByIdsForUserAsync returns empty list when requestedIds is empty``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let user = Entities.makeUser 800

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Entries.getEntryIdsByIdsForUserAsync [] user.Id
                |> fixture.WithConnectionAsync

            Assert.Empty(actual)
        }

    [<Fact>]
    member _.``getEntryIdsByIdsForUserAsync returns only owned requested entry IDs``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 801

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
                Entries.getEntryIdsByIdsForUserAsync [ entry1.Id; entry2.Id ] user.Id
                |> fixture.WithConnectionAsync

            Assert.Equivalent([ entry1.Id; entry2.Id ], actual)
        }

    [<Fact>]
    member _.``getEntryIdsByIdsForUserAsync does not return entries owned by another user``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let owner = Entities.makeUser 802
            let other = Entities.makeUser 803

            let ownerCollection =
                Entities.makeCollection owner "Owner Collection" None createdAt createdAt false

            let ownerVocabulary =
                Entities.makeVocabulary ownerCollection "Vocabulary" None createdAt createdAt false

            let ownerEntry =
                Entities.makeEntry ownerVocabulary "owner-word" createdAt createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ owner; other ]
                |> Seeder.saveChangesAsync

            let! actual =
                Entries.getEntryIdsByIdsForUserAsync [ ownerEntry.Id ] other.Id
                |> fixture.WithConnectionAsync

            Assert.Empty(actual)
        }

    [<Fact>]
    member _.``getEntryIdsByIdsForUserAsync filters out non-existent IDs alongside owned entry``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 806

            let collection =
                Entities.makeCollection user "Collection" None createdAt createdAt false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None createdAt createdAt false

            let ownedEntry =
                Entities.makeEntry vocabulary "word" createdAt createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Entries.getEntryIdsByIdsForUserAsync [ ownedEntry.Id; 99999 ] user.Id
                |> fixture.WithConnectionAsync

            Assert.Equivalent([ ownedEntry.Id ], actual)
        }

    [<Fact>]
    member _.``getEntryIdsByIdsForUserAsync filters out IDs not belonging to the user``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 804
            let otherUser = Entities.makeUser 805

            let userCollection =
                Entities.makeCollection user "User Collection" None createdAt createdAt false

            let otherCollection =
                Entities.makeCollection otherUser "Other Collection" None createdAt createdAt false

            let userVocabulary =
                Entities.makeVocabulary userCollection "User Vocabulary" None createdAt createdAt false

            let otherVocabulary =
                Entities.makeVocabulary otherCollection "Other Vocabulary" None createdAt createdAt false

            let ownedEntry =
                Entities.makeEntry userVocabulary "owned" createdAt createdAt

            let foreignEntry =
                Entities.makeEntry otherVocabulary "foreign" createdAt createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user; otherUser ]
                |> Seeder.saveChangesAsync

            let! actual =
                Entries.getEntryIdsByIdsForUserAsync [ ownedEntry.Id; foreignEntry.Id ] user.Id
                |> fixture.WithConnectionAsync

            Assert.Equivalent([ ownedEntry.Id ], actual)
        }
