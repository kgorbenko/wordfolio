namespace Wordfolio.Api.Tests.Drafts

open System
open System.Net
open System.Net.Http.Json
open System.Threading.Tasks
open Microsoft.Extensions.DependencyInjection

open Xunit

open Wordfolio.Api.Api.Types
open Wordfolio.Api.Infrastructure.ResourceIdEncoder
open Wordfolio.Api.Tests
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

module Urls = Wordfolio.Api.Urls

type GetDraftByIdTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``GET draft by id returns entry with hierarchy``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(700, "user@example.com", "P@ssw0rd!")

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
                Urls.Drafts.draftById(encoder.Encode entry.Id)

            let! response = client.GetAsync(url)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<EntryResponse>()

            let expectedDefinitionExample: ExampleResponse =
                { Id = actual.Definitions.[0].Examples.[0].Id
                  ExampleText = "Hello, world!"
                  Source = ExampleSource.Custom }

            let expectedDefinition: DefinitionResponse =
                { Id = actual.Definitions.[0].Id
                  DefinitionText = "a greeting"
                  Source = DefinitionSource.Manual
                  DisplayOrder = 0
                  Examples = [ expectedDefinitionExample ] }

            let expectedTranslationExample: ExampleResponse =
                { Id = actual.Translations.[0].Examples.[0].Id
                  ExampleText = "Hola, mundo!"
                  Source = ExampleSource.Custom }

            let expectedTranslation: TranslationResponse =
                { Id = actual.Translations.[0].Id
                  TranslationText = "hola"
                  Source = TranslationSource.Manual
                  DisplayOrder = 0
                  Examples = [ expectedTranslationExample ] }

            let expected: EntryResponse =
                { Id = encoder.Encode entry.Id
                  VocabularyId = encoder.Encode vocabulary.Id
                  EntryText = "hello"
                  CreatedAt = entry.CreatedAt
                  UpdatedAt = entry.UpdatedAt
                  Definitions = [ expectedDefinition ]
                  Translations = [ expectedTranslation ] }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``GET draft by id returns 404 when entry does not exist``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(701, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                Urls.Drafts.draftById(encoder.Encode 999999)

            let! response = client.GetAsync(url)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``GET draft by id without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            use client = factory.CreateClient()

            let url =
                Urls.Drafts.draftById(encoder.Encode 1)

            let! response = client.GetAsync(url)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }
