namespace Wordfolio.Api.DataAccess.Tests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type GetDefaultVocabularyByUserIdTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``getDefaultVocabularyByUserIdAsync returns default vocabulary when it exists``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let collection =
                Entities.makeCollection user "Collection" None createdAt None false

            let defaultVocabulary =
                Entities.makeVocabulary collection "Unsorted" None createdAt None true

            let regularVocabulary =
                Entities.makeVocabulary collection "Regular" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ defaultVocabulary; regularVocabulary ]
                |> Seeder.saveChangesAsync

            let! actual =
                Vocabularies.getDefaultVocabularyByUserIdAsync user.Id
                |> fixture.WithConnectionAsync

            let expected: Vocabularies.Vocabulary option =
                Some
                    { Id = defaultVocabulary.Id
                      CollectionId = collection.Id
                      Name = "Unsorted"
                      Description = None
                      CreatedAt = createdAt
                      UpdatedAt = None }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``getDefaultVocabularyByUserIdAsync returns None when no default vocabulary exists``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let collection =
                Entities.makeCollection user "Collection" None createdAt None false

            let regularVocabulary =
                Entities.makeVocabulary collection "Regular" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ regularVocabulary ]
                |> Seeder.saveChangesAsync

            let! actual =
                Vocabularies.getDefaultVocabularyByUserIdAsync user.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(None, actual)
        }

    [<Fact>]
    member _.``getDefaultVocabularyByUserIdAsync returns default vocabulary for requested user when other users have defaults``
        ()
        =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user1 = Entities.makeUser 100
            let user2 = Entities.makeUser 101

            let collection1 =
                Entities.makeCollection user1 "Collection 1" None createdAt None false

            let collection2 =
                Entities.makeCollection user2 "Collection 2" None createdAt None false

            let user1DefaultVocabulary =
                Entities.makeVocabulary collection1 "User 1 Default" None createdAt None true

            let _ =
                Entities.makeVocabulary collection2 "User 2 Default" None createdAt None true

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user1; user2 ]
                |> Seeder.saveChangesAsync

            let! actual =
                Vocabularies.getDefaultVocabularyByUserIdAsync user1.Id
                |> fixture.WithConnectionAsync

            let expected: Vocabularies.Vocabulary option =
                Some
                    { Id = user1DefaultVocabulary.Id
                      CollectionId = collection1.Id
                      Name = "User 1 Default"
                      Description = None
                      CreatedAt = createdAt
                      UpdatedAt = None }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``getDefaultVocabularyByUserIdAsync throws when multiple default vocabularies exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let collection =
                Entities.makeCollection user "Collection" None createdAt None false

            let defaultVocabulary1 =
                Entities.makeVocabulary collection "Default 1" None createdAt None true

            let defaultVocabulary2 =
                Entities.makeVocabulary collection "Default 2" None createdAt None true

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ defaultVocabulary1; defaultVocabulary2 ]
                |> Seeder.saveChangesAsync

            let! ex =
                Assert.ThrowsAsync<System.Exception>(fun () ->
                    Vocabularies.getDefaultVocabularyByUserIdAsync user.Id
                    |> fixture.WithConnectionAsync
                    :> Task)

            Assert.Equal("Query returned more than one element when at most one was expected", ex.Message)
        }
