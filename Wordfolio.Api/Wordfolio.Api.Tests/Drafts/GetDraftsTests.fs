namespace Wordfolio.Api.Tests.Drafts

open System
open System.Net
open System.Net.Http.Json
open System.Threading.Tasks
open Microsoft.Extensions.DependencyInjection

open Xunit

open Wordfolio.Api.Api.Drafts.Types
open Wordfolio.Api.Api.Types
open Wordfolio.Api.Infrastructure.ResourceIdEncoder
open Wordfolio.Api.Tests
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

module Urls = Wordfolio.Api.Urls

type GetDraftsTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``GET /drafts returns 401 when not authenticated``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            use client = factory.CreateClient()

            let! response = client.GetAsync(Urls.Drafts.allDrafts())

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``GET /drafts returns 404 when no default vocabulary``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(100, "user@example.com", "P@ssw0rd!")

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Col" None createdAt createdAt false

            let vocabulary =
                Entities.makeVocabulary collection "NotDefault" None createdAt createdAt false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.GetAsync(Urls.Drafts.allDrafts())

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``GET /drafts returns drafts with empty entries``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(100, "user@example.com", "P@ssw0rd!")

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Col" None createdAt createdAt false

            let defaultVocab =
                Entities.makeVocabulary collection "Drafts" (Some "My drafts") createdAt createdAt true

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ defaultVocab ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.GetAsync(Urls.Drafts.allDrafts())
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<DraftsVocabularyDataResponse>()

            let expected: DraftsVocabularyDataResponse =
                { Vocabulary =
                    { Id = encoder.Encode defaultVocab.Id
                      Name = "Drafts"
                      Description = Some "My drafts"
                      CreatedAt = actual.Vocabulary.CreatedAt
                      UpdatedAt = actual.Vocabulary.CreatedAt }
                  Entries = [] }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``GET /drafts returns drafts with entries and hierarchy``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(100, "user@example.com", "P@ssw0rd!")

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Col" None createdAt createdAt false

            let defaultVocab =
                Entities.makeVocabulary collection "Drafts" None createdAt createdAt true

            let entry =
                Entities.makeEntry defaultVocab "ephemeral" createdAt createdAt

            let definition =
                Entities.makeDefinition
                    entry
                    "lasting for a short time"
                    Wordfolio.Api.DataAccess.Definitions.DefinitionSource.Api
                    1

            let translation =
                Entities.makeTranslation
                    entry
                    "эфемерный"
                    Wordfolio.Api.DataAccess.Translations.TranslationSource.Manual
                    1

            let example =
                Entities.makeExampleForDefinition
                    definition
                    "ephemeral pleasures"
                    Wordfolio.Api.DataAccess.Examples.ExampleSource.Custom

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ defaultVocab ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.addDefinitions [ definition ]
                |> Seeder.addTranslations [ translation ]
                |> Seeder.addExamples [ example ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.GetAsync(Urls.Drafts.allDrafts())
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<DraftsVocabularyDataResponse>()

            let expected: DraftsVocabularyDataResponse =
                { Vocabulary =
                    { Id = encoder.Encode defaultVocab.Id
                      Name = "Drafts"
                      Description = None
                      CreatedAt = actual.Vocabulary.CreatedAt
                      UpdatedAt = actual.Vocabulary.CreatedAt }
                  Entries =
                    [ { Id = encoder.Encode entry.Id
                        VocabularyId = encoder.Encode defaultVocab.Id
                        EntryText = "ephemeral"
                        CreatedAt = actual.Entries.[0].CreatedAt
                        UpdatedAt = actual.Entries.[0].CreatedAt
                        Definitions =
                          [ { Id = encoder.Encode definition.Id
                              DefinitionText = "lasting for a short time"
                              Source = DefinitionSource.Api
                              DisplayOrder = 1
                              Examples =
                                [ { Id = encoder.Encode example.Id
                                    ExampleText = "ephemeral pleasures"
                                    Source = ExampleSource.Custom } ] } ]
                        Translations =
                          [ { Id = encoder.Encode translation.Id
                              TranslationText = "эфемерный"
                              Source = TranslationSource.Manual
                              DisplayOrder = 1
                              Examples = [] } ] } ] }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``GET /drafts does not return other user's data``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser1, wordfolioUser1 = factory.CreateUserAsync(100, "user1@example.com", "P@ssw0rd!")

            let! _, wordfolioUser2 = factory.CreateUserAsync(200, "user2@example.com", "P@ssw0rd!")

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let collection1 =
                Entities.makeCollection wordfolioUser1 "Col1" None createdAt createdAt false

            let collection2 =
                Entities.makeCollection wordfolioUser2 "Col2" None createdAt createdAt false

            let vocab1 =
                Entities.makeVocabulary collection1 "Drafts1" None createdAt createdAt true

            let vocab2 =
                Entities.makeVocabulary collection2 "Drafts2" None createdAt createdAt true

            let entry1 =
                Entities.makeEntry vocab1 "myword" createdAt createdAt

            let entry2 =
                Entities.makeEntry vocab2 "otherword" createdAt createdAt

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser1; wordfolioUser2 ]
                |> Seeder.addCollections [ collection1; collection2 ]
                |> Seeder.addVocabularies [ vocab1; vocab2 ]
                |> Seeder.addEntries [ entry1; entry2 ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser1)

            let! response = client.GetAsync(Urls.Drafts.allDrafts())
            let! actual = response.Content.ReadFromJsonAsync<DraftsVocabularyDataResponse>()

            let expected: DraftsVocabularyDataResponse =
                { Vocabulary =
                    { Id = encoder.Encode vocab1.Id
                      Name = "Drafts1"
                      Description = None
                      CreatedAt = actual.Vocabulary.CreatedAt
                      UpdatedAt = actual.Vocabulary.CreatedAt }
                  Entries =
                    [ { Id = encoder.Encode entry1.Id
                        VocabularyId = encoder.Encode vocab1.Id
                        EntryText = "myword"
                        CreatedAt = actual.Entries.[0].CreatedAt
                        UpdatedAt = actual.Entries.[0].CreatedAt
                        Definitions = []
                        Translations = [] } ] }

            Assert.Equal(expected, actual)
        }
