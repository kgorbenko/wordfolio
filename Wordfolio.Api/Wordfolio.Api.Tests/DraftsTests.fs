namespace Wordfolio.Api.Tests

open System
open System.Net
open System.Net.Http.Json
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Handlers.Drafts
open Wordfolio.Api.Handlers.Entries
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

module Urls = Wordfolio.Api.Urls

type DraftsTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``GET /drafts returns 401 when not authenticated``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let! response = client.GetAsync(Urls.Drafts.Path)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``GET /drafts returns 404 when no default vocabulary``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(100, "user@example.com", "P@ssw0rd!")

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Col" None createdAt None false

            let vocabulary =
                Entities.makeVocabulary collection "NotDefault" None createdAt None false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.GetAsync(Urls.Drafts.Path)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``GET /drafts returns drafts with empty entries``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(100, "user@example.com", "P@ssw0rd!")

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Col" None createdAt None false

            let defaultVocab =
                Entities.makeVocabulary collection "Drafts" (Some "My drafts") createdAt None true

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ defaultVocab ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.GetAsync(Urls.Drafts.Path)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<DraftsResponse>()

            let expected: DraftsResponse =
                { Vocabulary =
                    { Id = defaultVocab.Id
                      Name = "Drafts"
                      Description = Some "My drafts"
                      CreatedAt = actual.Vocabulary.CreatedAt
                      UpdatedAt = None }
                  Entries = [] }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``GET /drafts returns drafts with entries and hierarchy``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(100, "user@example.com", "P@ssw0rd!")

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Col" None createdAt None false

            let defaultVocab =
                Entities.makeVocabulary collection "Drafts" None createdAt None true

            let entry =
                Entities.makeEntry defaultVocab "ephemeral" createdAt None

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

            let! response = client.GetAsync(Urls.Drafts.Path)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<DraftsResponse>()

            let expected: DraftsResponse =
                { Vocabulary =
                    { Id = defaultVocab.Id
                      Name = "Drafts"
                      Description = None
                      CreatedAt = actual.Vocabulary.CreatedAt
                      UpdatedAt = None }
                  Entries =
                    [ { Id = entry.Id
                        VocabularyId = defaultVocab.Id
                        EntryText = "ephemeral"
                        CreatedAt = actual.Entries.[0].CreatedAt
                        UpdatedAt = None
                        Definitions =
                          [ { Id = definition.Id
                              DefinitionText = "lasting for a short time"
                              Source = DefinitionSourceDto.Api
                              DisplayOrder = 1
                              Examples =
                                [ { Id = example.Id
                                    ExampleText = "ephemeral pleasures"
                                    Source = ExampleSourceDto.Custom } ] } ]
                        Translations =
                          [ { Id = translation.Id
                              TranslationText = "эфемерный"
                              Source = TranslationSourceDto.Manual
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

            let! identityUser1, wordfolioUser1 = factory.CreateUserAsync(100, "user1@example.com", "P@ssw0rd!")

            let! _, wordfolioUser2 = factory.CreateUserAsync(200, "user2@example.com", "P@ssw0rd!")

            let createdAt =
                DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

            let collection1 =
                Entities.makeCollection wordfolioUser1 "Col1" None createdAt None false

            let collection2 =
                Entities.makeCollection wordfolioUser2 "Col2" None createdAt None false

            let vocab1 =
                Entities.makeVocabulary collection1 "Drafts1" None createdAt None true

            let vocab2 =
                Entities.makeVocabulary collection2 "Drafts2" None createdAt None true

            let entry1 =
                Entities.makeEntry vocab1 "myword" createdAt None

            let entry2 =
                Entities.makeEntry vocab2 "otherword" createdAt None

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser1; wordfolioUser2 ]
                |> Seeder.addCollections [ collection1; collection2 ]
                |> Seeder.addVocabularies [ vocab1; vocab2 ]
                |> Seeder.addEntries [ entry1; entry2 ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser1)

            let! response = client.GetAsync(Urls.Drafts.Path)
            let! actual = response.Content.ReadFromJsonAsync<DraftsResponse>()

            let expected: DraftsResponse =
                { Vocabulary =
                    { Id = vocab1.Id
                      Name = "Drafts1"
                      Description = None
                      CreatedAt = actual.Vocabulary.CreatedAt
                      UpdatedAt = None }
                  Entries =
                    [ { Id = entry1.Id
                        VocabularyId = vocab1.Id
                        EntryText = "myword"
                        CreatedAt = actual.Entries.[0].CreatedAt
                        UpdatedAt = None
                        Definitions = []
                        Translations = [] } ] }

            Assert.Equal(expected, actual)
        }
