namespace Wordfolio.Api.DataAccess.Tests.Collections

open System

open Xunit

open Wordfolio.Api.DataAccess
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

type DeleteCollectionTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``deleteCollectionAsync deletes an existing collection``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 102

            let collection =
                Entities.makeCollection user "Collection to delete" None createdAt createdAt false

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.saveChangesAsync

            let! affectedRows =
                Collections.deleteCollectionAsync collection.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(1, affectedRows)

            let! actual =
                fixture.Seeder
                |> Seeder.getAllCollectionsAsync

            Assert.Empty(actual)
        }

    [<Fact>]
    member _.``deleteCollectionAsync cascades to vocabularies and entries``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 103

            let collectionToDelete =
                Entities.makeCollection user "Collection to delete" None createdAt createdAt false

            let untouchedCollection =
                Entities.makeCollection user "Untouched Collection" None createdAt createdAt false

            let vocabulary =
                Entities.makeVocabulary collectionToDelete "Vocabulary to delete" None createdAt createdAt false

            let untouchedVocabulary =
                Entities.makeVocabulary untouchedCollection "Untouched Vocabulary" None createdAt createdAt false

            let entry =
                Entities.makeEntry vocabulary "word" createdAt createdAt

            let untouchedEntry =
                Entities.makeEntry untouchedVocabulary "untouched word" createdAt createdAt

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ collectionToDelete; untouchedCollection ]
                |> Seeder.addVocabularies [ vocabulary; untouchedVocabulary ]
                |> Seeder.addEntries [ entry; untouchedEntry ]
                |> Seeder.saveChangesAsync

            let! affectedRows =
                Collections.deleteCollectionAsync collectionToDelete.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(1, affectedRows)

            let! actualCollections =
                fixture.Seeder
                |> Seeder.getAllCollectionsAsync

            let actualCollection =
                Assert.Single(actualCollections)

            let expectedCollection: Collection =
                { Id = untouchedCollection.Id
                  UserId = 103
                  Name = "Untouched Collection"
                  Description = None
                  CreatedAt = createdAt
                  UpdatedAt = createdAt
                  IsSystem = false }

            Assert.Equal(expectedCollection, actualCollection)

            let! actualVocabularies =
                fixture.Seeder
                |> Seeder.getAllVocabulariesAsync

            let actualVocabulary =
                Assert.Single(actualVocabularies)

            let expectedVocabulary: Vocabulary =
                { Id = untouchedVocabulary.Id
                  CollectionId = untouchedCollection.Id
                  Name = "Untouched Vocabulary"
                  Description = None
                  CreatedAt = createdAt
                  UpdatedAt = createdAt
                  IsDefault = false }

            Assert.Equal(expectedVocabulary, actualVocabulary)

            let! actualEntries =
                fixture.Seeder
                |> Seeder.getAllEntriesAsync

            let actualEntry =
                Assert.Single(actualEntries)

            let expectedEntry: Entry =
                { Id = untouchedEntry.Id
                  VocabularyId = untouchedVocabulary.Id
                  EntryText = "untouched word"
                  CreatedAt = createdAt
                  UpdatedAt = createdAt }

            Assert.Equal(expectedEntry, actualEntry)
        }

    [<Fact>]
    member _.``deleteCollectionAsync returns 0 when collection does not exist``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let! affectedRows =
                Collections.deleteCollectionAsync 999
                |> fixture.WithConnectionAsync

            Assert.Equal(0, affectedRows)
        }

    [<Fact>]
    member _.``deleteCollectionAsync returns 0 when collection is system``() =
        task {
            do! fixture.ResetDatabaseAsync()

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let user = Entities.makeUser 100

            let systemCollection =
                Entities.makeCollection user "Unsorted" None createdAt createdAt true

            do!
                fixture.Seeder
                |> Seeder.addUsers [ user ]
                |> Seeder.addCollections [ systemCollection ]
                |> Seeder.saveChangesAsync

            let! affectedRows =
                Collections.deleteCollectionAsync systemCollection.Id
                |> fixture.WithConnectionAsync

            Assert.Equal(0, affectedRows)

            let! actual = Seeder.getCollectionByIdAsync systemCollection.Id fixture.Seeder

            Assert.True(actual.IsSome)
        }
