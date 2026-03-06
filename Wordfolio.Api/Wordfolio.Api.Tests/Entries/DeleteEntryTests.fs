namespace Wordfolio.Api.Tests.Entries

open System
open System.Net
open System.Net.Http.Json
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Api.Types
open Wordfolio.Api.Tests
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

module Urls = Wordfolio.Api.Urls

type DeleteEntryTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``DELETE deletes entry successfully``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(402, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None false

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None DateTimeOffset.UtcNow None false

            let entry =
                Entities.makeEntry vocabulary "hello" DateTimeOffset.UtcNow None

            let unaffectedEntry =
                Entities.makeEntry vocabulary "remain" DateTimeOffset.UtcNow None

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry; unaffectedEntry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                Urls.Entries.entryById(collection.Id, vocabulary.Id, entry.Id)

            let! response = client.DeleteAsync(url)

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode)

            let! actual = Seeder.getEntryByIdAsync entry.Id fixture.WordfolioSeeder
            let expected: Entry option = None

            Assert.Equal<Entry option>(expected, actual)

            let! dbEntries = Seeder.getAllEntriesAsync fixture.WordfolioSeeder

            let unaffectedEntryInDatabase =
                dbEntries
                |> List.find(fun currentEntry -> currentEntry.Id = unaffectedEntry.Id)

            let expectedDbEntries =
                [ { unaffectedEntryInDatabase with
                      Id = unaffectedEntry.Id
                      VocabularyId = vocabulary.Id
                      EntryText = "remain" } ]

            Assert.Equal<Entry list>(expectedDbEntries, dbEntries)
        }

    [<Fact>]
    member _.``DELETE without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! _, wordfolioUser = factory.CreateUserAsync(403, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None false

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None DateTimeOffset.UtcNow None false

            let entry =
                Entities.makeEntry vocabulary "hello" DateTimeOffset.UtcNow None

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.saveChangesAsync

            use client = factory.CreateClient()

            let url =
                Urls.Entries.entryById(collection.Id, vocabulary.Id, entry.Id)

            let! response = client.DeleteAsync(url)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)

            let! dbEntries = Seeder.getAllEntriesAsync fixture.WordfolioSeeder

            let expectedDbEntries =
                [ { dbEntries.[0] with
                      Id = entry.Id
                      VocabularyId = vocabulary.Id
                      EntryText = "hello" } ]

            Assert.Equal<Entry list>(expectedDbEntries, dbEntries)
        }

    [<Fact>]
    member _.``DELETE for non-existent entry returns 404``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(404, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                Urls.Entries.entryById(1, 1, 999999)

            let! response = client.DeleteAsync(url)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``DELETE for another user's entry returns 404``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! _, wordfolioUser1 = factory.CreateUserAsync(405, "user1@example.com", "P@ssw0rd!")
            let! identityUser2, wordfolioUser2 = factory.CreateUserAsync(406, "user2@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser1 "Test Collection" None DateTimeOffset.UtcNow None false

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None DateTimeOffset.UtcNow None false

            let entry =
                Entities.makeEntry vocabulary "hello" DateTimeOffset.UtcNow None

            let requesterCollection =
                Entities.makeCollection wordfolioUser2 "Requester Collection" None DateTimeOffset.UtcNow None false

            let requesterVocabulary =
                Entities.makeVocabulary requesterCollection "Requester Vocabulary" None DateTimeOffset.UtcNow None false

            let requesterEntry =
                Entities.makeEntry requesterVocabulary "requester entry" DateTimeOffset.UtcNow None

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser1; wordfolioUser2 ]
                |> Seeder.addCollections [ collection; requesterCollection ]
                |> Seeder.addVocabularies [ vocabulary; requesterVocabulary ]
                |> Seeder.addEntries [ entry; requesterEntry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser2)

            let url =
                Urls.Entries.entryById(collection.Id, vocabulary.Id, entry.Id)

            let! response = client.DeleteAsync(url)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)

            let! dbEntries = Seeder.getAllEntriesAsync fixture.WordfolioSeeder

            let ownerEntryInDatabase =
                dbEntries
                |> List.find(fun currentEntry -> currentEntry.Id = entry.Id)

            let requesterEntryInDatabase =
                dbEntries
                |> List.find(fun currentEntry -> currentEntry.Id = requesterEntry.Id)

            let expectedDbEntries =
                [ { ownerEntryInDatabase with
                      Id = entry.Id
                      VocabularyId = vocabulary.Id
                      EntryText = "hello" }
                  { requesterEntryInDatabase with
                      Id = requesterEntry.Id
                      VocabularyId = requesterVocabulary.Id
                      EntryText = "requester entry" } ]
                |> List.sortBy(fun currentEntry -> currentEntry.Id)

            let actualDbEntries =
                dbEntries
                |> List.sortBy(fun currentEntry -> currentEntry.Id)

            Assert.Equal<Entry list>(expectedDbEntries, actualDbEntries)
        }

    [<Fact>]
    member _.``DELETE cascades to delete definitions, translations, and examples``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(407, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None false

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None DateTimeOffset.UtcNow None false

            let entry =
                Entities.makeEntry vocabulary "hello" DateTimeOffset.UtcNow None

            let definition =
                Entities.makeDefinition
                    entry
                    "a greeting"
                    Wordfolio.Api.DataAccess.Definitions.DefinitionSource.Manual
                    0

            let translation =
                Entities.makeTranslation entry "hola" Wordfolio.Api.DataAccess.Translations.TranslationSource.Manual 0

            let defExample =
                Entities.makeExampleForDefinition
                    definition
                    "Hello, world!"
                    Wordfolio.Api.DataAccess.Examples.ExampleSource.Custom

            let transExample =
                Entities.makeExampleForTranslation
                    translation
                    "Hola, mundo!"
                    Wordfolio.Api.DataAccess.Examples.ExampleSource.Custom

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.addDefinitions [ definition ]
                |> Seeder.addTranslations [ translation ]
                |> Seeder.addExamples [ defExample; transExample ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                Urls.Entries.entryById(collection.Id, vocabulary.Id, entry.Id)

            let! response = client.DeleteAsync(url)

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode)

            let! entries = Seeder.getAllEntriesAsync fixture.WordfolioSeeder
            Assert.Empty(entries)

            let! definitions = Seeder.getAllDefinitionsAsync fixture.WordfolioSeeder
            Assert.Empty(definitions)

            let! translations = Seeder.getAllTranslationsAsync fixture.WordfolioSeeder
            Assert.Empty(translations)

            let! examples = Seeder.getAllExamplesAsync fixture.WordfolioSeeder
            Assert.Empty(examples)
        }

    [<Fact>]
    member _.``DELETE returns 404 when collection does not exist``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(510, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None false

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None DateTimeOffset.UtcNow None false

            let entry =
                Entities.makeEntry vocabulary "hello" DateTimeOffset.UtcNow None

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.DeleteAsync(Urls.Entries.entryById(999999, vocabulary.Id, entry.Id))

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``DELETE returns 404 when entry belongs to different vocabulary``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(514, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None false

            let vocabularyA =
                Entities.makeVocabulary collection "Vocabulary A" None DateTimeOffset.UtcNow None false

            let vocabularyB =
                Entities.makeVocabulary collection "Vocabulary B" None DateTimeOffset.UtcNow None false

            let entry =
                Entities.makeEntry vocabularyA "hello" DateTimeOffset.UtcNow None

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabularyA; vocabularyB ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.DeleteAsync(Urls.Entries.entryById(collection.Id, vocabularyB.Id, entry.Id))

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``DELETE returns 404 when vocabulary belongs to different collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(521, "user@example.com", "P@ssw0rd!")

            let collectionA =
                Entities.makeCollection wordfolioUser "Collection A" None DateTimeOffset.UtcNow None false

            let collectionB =
                Entities.makeCollection wordfolioUser "Collection B" None DateTimeOffset.UtcNow None false

            let vocabulary =
                Entities.makeVocabulary collectionB "Test Vocabulary" None DateTimeOffset.UtcNow None false

            let entry =
                Entities.makeEntry vocabulary "hello" DateTimeOffset.UtcNow None

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collectionA; collectionB ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.DeleteAsync(Urls.Entries.entryById(collectionA.Id, vocabulary.Id, entry.Id))

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }
