namespace Wordfolio.Api.DataAccess.Tests.Vocabularies

open System

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type GetVocabulariesByCollectionIdTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``getVocabulariesByCollectionIdAsync returns vocabularies for collection``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 200

            let collection1 =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let collection2 =
                Entities.makeCollection user "Collection 2" None createdAt None false

            let vocab1 =
                Entities.makeVocabulary collection1 "Vocab 1" None createdAt None false

            let vocab2 =
                Entities.makeVocabulary collection1 "Vocab 2" None createdAt None false

            let _ =
                Entities.makeVocabulary collection2 "Vocab 3" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Vocabularies.getVocabulariesByCollectionIdAsync collection1.Id
                |> fixture.WithConnectionAsync

            let expected: Vocabularies.Vocabulary list =
                [ { Id = vocab1.Id
                    CollectionId = collection1.Id
                    Name = "Vocab 1"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None }
                  { Id = vocab2.Id
                    CollectionId = collection1.Id
                    Name = "Vocab 2"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None } ]

            Assert.Equivalent(expected, actual)
        }

    [<Fact>]
    member _.``getVocabulariesByCollectionIdAsync returns empty list when collection has no vocabularies``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 200

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! actual =
                Vocabularies.getVocabulariesByCollectionIdAsync collection.Id
                |> fixture.WithConnectionAsync

            Assert.Empty(actual)
        }

    [<Fact>]
    member _.``getVocabulariesByCollectionIdAsync filters out default vocabularies``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let collection =
                Entities.makeCollection user "Collection 1" None createdAt None false

            let defaultVocabulary =
                Entities.makeVocabulary collection "Default" None createdAt None true

            let regularVocabulary =
                Entities.makeVocabulary collection "Regular" None createdAt None false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ defaultVocabulary; regularVocabulary ]
                |> Seeder.saveChangesAsync

            let! actual =
                Vocabularies.getVocabulariesByCollectionIdAsync collection.Id
                |> fixture.WithConnectionAsync

            let expected: Vocabularies.Vocabulary list =
                [ { Id = regularVocabulary.Id
                    CollectionId = collection.Id
                    Name = "Regular"
                    Description = None
                    CreatedAt = createdAt
                    UpdatedAt = None } ]

            Assert.Equal<Vocabularies.Vocabulary list>(expected, actual)
        }

    [<Fact>]
    member _.``getVocabulariesByCollectionIdAsync returns empty list when collection is system``() =
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
                Vocabularies.getVocabulariesByCollectionIdAsync systemCollection.Id
                |> fixture.WithConnectionAsync

            Assert.Empty(actual)
        }
