namespace Wordfolio.Api.DataAccess.Tests.Vocabularies

open System

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type GetVocabularyByIdTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``getVocabularyByIdAsync returns vocabulary when it exists``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 200

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "My Vocabulary" (Some "Test vocabulary") createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Vocabularies.getVocabularyByIdAsync vocabulary.Id
                |> fixture.WithConnectionAsync

            let expected: Vocabularies.Vocabulary option =
                Some
                    { Id = vocabulary.Id
                      CollectionId = collection.Id
                      Name = "My Vocabulary"
                      Description = Some "Test vocabulary"
                      CreatedAt = createdAt
                      UpdatedAt = None }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``getVocabularyByIdAsync returns None when vocabulary does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! actual =
                Vocabularies.getVocabularyByIdAsync 999
                |> fixture.WithConnectionAsync

            Assert.Equal(None, actual)
        }

    [<Fact>]
    member _.``getVocabularyByIdAsync returns None when vocabulary is default``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

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
                Vocabularies.getVocabularyByIdAsync defaultVocabulary.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(None, actual)
        }

    [<Fact>]
    member _.``getVocabularyByIdAsync returns None when vocabulary is in system collection``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

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
                Vocabularies.getVocabularyByIdAsync vocabulary.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(None, actual)
        }
