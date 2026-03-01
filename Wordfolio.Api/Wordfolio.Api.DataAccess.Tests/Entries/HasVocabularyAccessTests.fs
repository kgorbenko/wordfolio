namespace Wordfolio.Api.DataAccess.Tests

open System

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type HasVocabularyAccessTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

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
