namespace Wordfolio.Api.DataAccess.Tests.Entries

open System

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type HasVocabularyAccessInCollectionTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``hasVocabularyAccessInCollectionAsync returns true when vocabulary belongs to collection and user owns it``
        ()
        =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 319

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            let! actual =
                Entries.hasVocabularyAccessInCollectionAsync vocabulary.Id collection.Id user.Id
                |> fixture.WithConnectionAsync

            Assert.True(actual)
        }

    [<Fact>]
    member _.``hasVocabularyAccessInCollectionAsync returns false when vocabulary does not belong to specified collection``
        ()
        =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 320

            let collectionA =
                Entities.makeCollection user "Collection A" None createdAt None false

            let collectionB =
                Entities.makeCollection user "Collection B" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collectionA "Vocabulary 1" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collectionA; collectionB ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            let! actual =
                Entries.hasVocabularyAccessInCollectionAsync vocabulary.Id collectionB.Id user.Id
                |> fixture.WithConnectionAsync

            Assert.False(actual)
        }

    [<Fact>]
    member _.``hasVocabularyAccessInCollectionAsync returns false when user does not own the collection``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user1 = Entities.makeUser 321
            let user2 = Entities.makeUser 322

            let collection =
                Entities.makeCollection user1 "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user1; user2 ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            let! actual =
                Entries.hasVocabularyAccessInCollectionAsync vocabulary.Id collection.Id user2.Id
                |> fixture.WithConnectionAsync

            Assert.False(actual)
        }

    [<Fact>]
    member _.``hasVocabularyAccessInCollectionAsync returns false when vocabulary does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! actual =
                Entries.hasVocabularyAccessInCollectionAsync 999 888 777
                |> fixture.WithConnectionAsync

            Assert.False(actual)
        }

    [<Fact>]
    member _.``hasVocabularyAccessInCollectionAsync returns false when collection does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 323

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            let! actual =
                Entries.hasVocabularyAccessInCollectionAsync vocabulary.Id 999 user.Id
                |> fixture.WithConnectionAsync

            Assert.False(actual)
        }

    [<Fact>]
    member _.``hasVocabularyAccessInCollectionAsync returns false when user does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 324

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary 1" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            let! actual =
                Entries.hasVocabularyAccessInCollectionAsync vocabulary.Id collection.Id 999
                |> fixture.WithConnectionAsync

            Assert.False(actual)
        }
