namespace Wordfolio.Api.Tests.Entries

open System
open System.Net
open System.Net.Http.Json
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Api
open Wordfolio.Api.Tests
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

module Urls = Wordfolio.Api.Urls

type GetEntryByIdTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``GET by id returns entry with hierarchy``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(306, "user@example.com", "P@ssw0rd!")

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

            let definitionExample =
                Entities.makeExampleForDefinition
                    definition
                    "Hello, world!"
                    Wordfolio.Api.DataAccess.Examples.ExampleSource.Custom

            let translation =
                Entities.makeTranslation entry "hola" Wordfolio.Api.DataAccess.Translations.TranslationSource.Manual 0

            let translationExample =
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
                |> Seeder.addExamples [ definitionExample; translationExample ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                Urls.Entries.entryById(collection.Id, vocabulary.Id, entry.Id)

            let! response = client.GetAsync(url)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<EntryResponse>()

            let expectedDefinitionExample: ExampleResponse =
                { Id = actual.Definitions.[0].Examples.[0].Id
                  ExampleText = "Hello, world!"
                  Source = ExampleSourceDto.Custom }

            let expectedDefinition: DefinitionResponse =
                { Id = actual.Definitions.[0].Id
                  DefinitionText = "a greeting"
                  Source = DefinitionSourceDto.Manual
                  DisplayOrder = 0
                  Examples = [ expectedDefinitionExample ] }

            let expectedTranslationExample: ExampleResponse =
                { Id = actual.Translations.[0].Examples.[0].Id
                  ExampleText = "Hola, mundo!"
                  Source = ExampleSourceDto.Custom }

            let expectedTranslation: TranslationResponse =
                { Id = actual.Translations.[0].Id
                  TranslationText = "hola"
                  Source = TranslationSourceDto.Manual
                  DisplayOrder = 0
                  Examples = [ expectedTranslationExample ] }

            let expected: EntryResponse =
                { Id = entry.Id
                  VocabularyId = vocabulary.Id
                  EntryText = "hello"
                  CreatedAt = actual.CreatedAt
                  UpdatedAt = None
                  Definitions = [ expectedDefinition ]
                  Translations = [ expectedTranslation ] }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``GET by id returns 404 when entry does not exist``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(307, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                Urls.Entries.entryById(1, 1, 999999)

            let! response = client.GetAsync(url)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``GET by id returns 404 when requesting another user's entry``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! _, ownerWordfolioUser = factory.CreateUserAsync(520, "owner@example.com", "P@ssw0rd!")

            let! requesterIdentityUser, requesterWordfolioUser =
                factory.CreateUserAsync(521, "requester@example.com", "P@ssw0rd!")

            let ownerCollection =
                Entities.makeCollection ownerWordfolioUser "Owner Collection" None DateTimeOffset.UtcNow None false

            let ownerVocabulary =
                Entities.makeVocabulary ownerCollection "Owner Vocabulary" None DateTimeOffset.UtcNow None false

            let ownerEntry =
                Entities.makeEntry ownerVocabulary "owner-entry" DateTimeOffset.UtcNow None

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ ownerWordfolioUser; requesterWordfolioUser ]
                |> Seeder.addCollections [ ownerCollection ]
                |> Seeder.addVocabularies [ ownerVocabulary ]
                |> Seeder.addEntries [ ownerEntry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(requesterIdentityUser)

            let! response =
                client.GetAsync(Urls.Entries.entryById(ownerCollection.Id, ownerVocabulary.Id, ownerEntry.Id))

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``GET by id without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let url = Urls.Entries.entryById(1, 1, 1)

            let! response = client.GetAsync(url)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``GET by id returns 404 when collection does not exist``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(508, "user@example.com", "P@ssw0rd!")

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

            let! response = client.GetAsync(Urls.Entries.entryById(999999, vocabulary.Id, entry.Id))

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``GET by id returns 404 when entry belongs to different vocabulary``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(512, "user@example.com", "P@ssw0rd!")

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

            let! response = client.GetAsync(Urls.Entries.entryById(collection.Id, vocabularyB.Id, entry.Id))

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``GET by id returns 404 when vocabulary does not exist``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(518, "user@example.com", "P@ssw0rd!")

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

            let! response = client.GetAsync(Urls.Entries.entryById(collection.Id, 999999, entry.Id))

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``GET by id returns 404 when vocabulary belongs to different collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(519, "user@example.com", "P@ssw0rd!")

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

            let! response = client.GetAsync(Urls.Entries.entryById(collectionA.Id, vocabulary.Id, entry.Id))

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }
