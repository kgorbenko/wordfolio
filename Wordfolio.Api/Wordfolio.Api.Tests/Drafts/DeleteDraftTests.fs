namespace Wordfolio.Api.Tests.Drafts

open System
open System.Net
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Tests
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

module Urls = Wordfolio.Api.Urls

type DeleteDraftTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``DELETE draft deletes entry successfully``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(706, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None now now false

            let entry =
                Entities.makeEntry vocabulary "hello" now now

            let unaffectedEntry =
                Entities.makeEntry vocabulary "remain" now now

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry; unaffectedEntry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url = Urls.Drafts.draftById entry.Id

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
    member _.``DELETE draft without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! _, wordfolioUser = factory.CreateUserAsync(707, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None now now false

            let entry =
                Entities.makeEntry vocabulary "hello" now now

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.saveChangesAsync

            use client = factory.CreateClient()

            let url = Urls.Drafts.draftById entry.Id

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
    member _.``DELETE draft for non-existent entry returns 404``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(708, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url = Urls.Drafts.draftById 999999

            let! response = client.DeleteAsync(url)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``DELETE draft for another user's entry returns 404``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! _, wordfolioUser1 = factory.CreateUserAsync(715, "user1@example.com", "P@ssw0rd!")
            let! identityUser2, wordfolioUser2 = factory.CreateUserAsync(716, "user2@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser1 "Test Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None now now false

            let entry =
                Entities.makeEntry vocabulary "hello" now now

            let requesterCollection =
                Entities.makeCollection wordfolioUser2 "Requester Collection" None now now false

            let requesterVocabulary =
                Entities.makeVocabulary requesterCollection "Requester Vocabulary" None now now false

            let requesterEntry =
                Entities.makeEntry requesterVocabulary "requester entry" now now

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser1; wordfolioUser2 ]
                |> Seeder.addCollections [ collection; requesterCollection ]
                |> Seeder.addVocabularies [ vocabulary; requesterVocabulary ]
                |> Seeder.addEntries [ entry; requesterEntry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser2)

            let url = Urls.Drafts.draftById entry.Id

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
    member _.``DELETE draft cascades to delete definitions, translations, and examples``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(717, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None now now false

            let entry =
                Entities.makeEntry vocabulary "hello" now now

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

            let url = Urls.Drafts.draftById entry.Id

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
