namespace Wordfolio.Api.Tests

open System
open System.Net
open System.Net.Http.Json
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.DataAccess.Definitions
open Wordfolio.Api.DataAccess.Examples
open Wordfolio.Api.DataAccess.Translations
open Wordfolio.Api.Domain.Entries
open Wordfolio.Api.Handlers.Entries
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

module Dto = Wordfolio.Api.Handlers.Entries

module Urls = Wordfolio.Api.Urls

type EntriesTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``POST creates entry with definitions and translations``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(300, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None DateTimeOffset.UtcNow None

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: CreateEntryRequest =
                { VocabularyId = vocabulary.Id
                  EntryText = "hello"
                  Definitions =
                    [ { DefinitionText = "a greeting"
                        Source = DefinitionSourceDto.Manual
                        Examples =
                          [ { ExampleText = "Hello, world!"
                              Source = ExampleSourceDto.Custom } ] } ]
                  Translations =
                    [ { TranslationText = "hola"
                        Source = TranslationSourceDto.Manual
                        Examples =
                          [ { ExampleText = "Hola, mundo!"
                              Source = ExampleSourceDto.Custom } ] } ] }

            let url = Urls.Entries.Path

            let! response = client.PostAsJsonAsync(url, request)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.Created, response.StatusCode)

            let! result = response.Content.ReadFromJsonAsync<EntryResponse>()

            Assert.NotNull(result)

            let expectedDefinitionExample: ExampleResponse =
                { Id = result.Definitions.[0].Examples.[0].Id
                  ExampleText = "Hello, world!"
                  Source = ExampleSourceDto.Custom }

            let expectedDefinition: DefinitionResponse =
                { Id = result.Definitions.[0].Id
                  DefinitionText = "a greeting"
                  Source = DefinitionSourceDto.Manual
                  DisplayOrder = 0
                  Examples = [ expectedDefinitionExample ] }

            let expectedTranslationExample: ExampleResponse =
                { Id = result.Translations.[0].Examples.[0].Id
                  ExampleText = "Hola, mundo!"
                  Source = ExampleSourceDto.Custom }

            let expectedTranslation: TranslationResponse =
                { Id = result.Translations.[0].Id
                  TranslationText = "hola"
                  Source = TranslationSourceDto.Manual
                  DisplayOrder = 0
                  Examples = [ expectedTranslationExample ] }

            let expectedEntry: EntryResponse =
                { Id = result.Id
                  VocabularyId = vocabulary.Id
                  EntryText = "hello"
                  CreatedAt = result.CreatedAt
                  UpdatedAt = None
                  Definitions = [ expectedDefinition ]
                  Translations = [ expectedTranslation ] }

            Assert.Equal(expectedEntry, result)

            let! dbEntries = Seeder.getAllEntriesAsync fixture.WordfolioSeeder

            let expectedDbEntries =
                [ { dbEntries.[0] with
                      EntryText = "hello"
                      VocabularyId = vocabulary.Id } ]

            Assert.Equal<Entry list>(expectedDbEntries, dbEntries)

            let! dbDefinitions = Seeder.getAllDefinitionsAsync fixture.WordfolioSeeder

            let expectedDbDefinitions =
                [ { dbDefinitions.[0] with
                      DefinitionText = "a greeting"
                      Source = Wordfolio.Api.DataAccess.Definitions.DefinitionSource.Manual
                      DisplayOrder = 0 } ]

            Assert.Equal<Definition list>(expectedDbDefinitions, dbDefinitions)

            let! dbTranslations = Seeder.getAllTranslationsAsync fixture.WordfolioSeeder

            let expectedDbTranslations =
                [ { dbTranslations.[0] with
                      TranslationText = "hola"
                      Source = Wordfolio.Api.DataAccess.Translations.TranslationSource.Manual
                      DisplayOrder = 0 } ]

            Assert.Equal<Translation list>(expectedDbTranslations, dbTranslations)

            let! dbExamples = Seeder.getAllExamplesAsync fixture.WordfolioSeeder

            let dbExamplesSorted =
                dbExamples
                |> List.sortBy(fun e -> e.ExampleText)

            let expectedDbExamples =
                [ { dbExamplesSorted.[0] with
                      ExampleText = "Hello, world!"
                      Source = Wordfolio.Api.DataAccess.Examples.ExampleSource.Custom }
                  { dbExamplesSorted.[1] with
                      ExampleText = "Hola, mundo!"
                      Source = Wordfolio.Api.DataAccess.Examples.ExampleSource.Custom } ]

            Assert.Equal<Example list>(expectedDbExamples, dbExamplesSorted)
        }

    [<Fact>]
    member _.``POST without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let request: CreateEntryRequest =
                { VocabularyId = 1
                  EntryText = "hello"
                  Definitions =
                    [ { DefinitionText = "a greeting"
                        Source = DefinitionSourceDto.Manual
                        Examples = [] } ]
                  Translations = [] }

            let url = Urls.Entries.Path

            let! response = client.PostAsJsonAsync(url, request)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``POST with empty entry text fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(301, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None DateTimeOffset.UtcNow None

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: CreateEntryRequest =
                { VocabularyId = vocabulary.Id
                  EntryText = ""
                  Definitions =
                    [ { DefinitionText = "a greeting"
                        Source = DefinitionSourceDto.Manual
                        Examples = [] } ]
                  Translations = [] }

            let url = Urls.Entries.Path

            let! response = client.PostAsJsonAsync(url, request)

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }

    [<Fact>]
    member _.``POST with no definitions or translations fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(302, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None DateTimeOffset.UtcNow None

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: CreateEntryRequest =
                { VocabularyId = vocabulary.Id
                  EntryText = "hello"
                  Definitions = []
                  Translations = [] }

            let url = Urls.Entries.Path

            let! response = client.PostAsJsonAsync(url, request)

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }

    [<Fact>]
    member _.``POST with too many examples fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(303, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None DateTimeOffset.UtcNow None

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: CreateEntryRequest =
                { VocabularyId = vocabulary.Id
                  EntryText = "hello"
                  Definitions =
                    [ { DefinitionText = "a greeting"
                        Source = DefinitionSourceDto.Manual
                        Examples =
                          [ { ExampleText = "Example 1"
                              Source = ExampleSourceDto.Custom }
                            { ExampleText = "Example 2"
                              Source = ExampleSourceDto.Custom }
                            { ExampleText = "Example 3"
                              Source = ExampleSourceDto.Custom }
                            { ExampleText = "Example 4"
                              Source = ExampleSourceDto.Custom }
                            { ExampleText = "Example 5"
                              Source = ExampleSourceDto.Custom }
                            { ExampleText = "Example 6"
                              Source = ExampleSourceDto.Custom } ] } ]
                  Translations = [] }

            let url = Urls.Entries.Path

            let! response = client.PostAsJsonAsync(url, request)

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }

    [<Fact>]
    member _.``POST with duplicate entry text fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(304, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None DateTimeOffset.UtcNow None

            let existingEntry =
                Entities.makeEntry vocabulary "hello" DateTimeOffset.UtcNow None

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ existingEntry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: CreateEntryRequest =
                { VocabularyId = vocabulary.Id
                  EntryText = "hello"
                  Definitions =
                    [ { DefinitionText = "a greeting"
                        Source = DefinitionSourceDto.Manual
                        Examples = [] } ]
                  Translations = [] }

            let url = Urls.Entries.Path

            let! response = client.PostAsJsonAsync(url, request)

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode)
        }

    [<Fact>]
    member _.``POST for non-existent vocabulary fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(305, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: CreateEntryRequest =
                { VocabularyId = 999999
                  EntryText = "hello"
                  Definitions =
                    [ { DefinitionText = "a greeting"
                        Source = DefinitionSourceDto.Manual
                        Examples = [] } ]
                  Translations = [] }

            let url = Urls.Entries.Path

            let! response = client.PostAsJsonAsync(url, request)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``GET by id returns entry with hierarchy``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(306, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None DateTimeOffset.UtcNow None

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

            let url = Urls.Entries.entryById entry.Id

            let! response = client.GetAsync(url)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! result = response.Content.ReadFromJsonAsync<EntryResponse>()

            Assert.NotNull(result)

            let expectedDefinitionExample: ExampleResponse =
                { Id = result.Definitions.[0].Examples.[0].Id
                  ExampleText = "Hello, world!"
                  Source = ExampleSourceDto.Custom }

            let expectedDefinition: DefinitionResponse =
                { Id = result.Definitions.[0].Id
                  DefinitionText = "a greeting"
                  Source = DefinitionSourceDto.Manual
                  DisplayOrder = 0
                  Examples = [ expectedDefinitionExample ] }

            let expectedTranslationExample: ExampleResponse =
                { Id = result.Translations.[0].Examples.[0].Id
                  ExampleText = "Hola, mundo!"
                  Source = ExampleSourceDto.Custom }

            let expectedTranslation: TranslationResponse =
                { Id = result.Translations.[0].Id
                  TranslationText = "hola"
                  Source = TranslationSourceDto.Manual
                  DisplayOrder = 0
                  Examples = [ expectedTranslationExample ] }

            let expectedEntry: EntryResponse =
                { Id = entry.Id
                  VocabularyId = vocabulary.Id
                  EntryText = "hello"
                  CreatedAt = result.CreatedAt
                  UpdatedAt = None
                  Definitions = [ expectedDefinition ]
                  Translations = [ expectedTranslation ] }

            Assert.Equal(expectedEntry, result)
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

            let url = Urls.Entries.entryById 999999

            let! response = client.GetAsync(url)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``GET by id without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let url = Urls.Entries.entryById 1

            let! response = client.GetAsync(url)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``GET by vocabulary id returns all entries``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(308, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None DateTimeOffset.UtcNow None

            let entry1 =
                Entities.makeEntry vocabulary "hello" DateTimeOffset.UtcNow None

            let entry2 =
                Entities.makeEntry vocabulary "world" DateTimeOffset.UtcNow None

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry1; entry2 ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                Urls.Entries.entriesByVocabulary vocabulary.Id

            let! response = client.GetAsync(url)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! result = response.Content.ReadFromJsonAsync<EntryResponse[]>()

            Assert.NotNull(result)
            Assert.Equal(2, result.Length)

            let sortedResult =
                result
                |> Array.sortBy(fun e -> e.EntryText)

            let expectedEntry1: EntryResponse =
                { Id = sortedResult.[0].Id
                  VocabularyId = vocabulary.Id
                  EntryText = "hello"
                  CreatedAt = sortedResult.[0].CreatedAt
                  UpdatedAt = None
                  Definitions = []
                  Translations = [] }

            let expectedEntry2: EntryResponse =
                { Id = sortedResult.[1].Id
                  VocabularyId = vocabulary.Id
                  EntryText = "world"
                  CreatedAt = sortedResult.[1].CreatedAt
                  UpdatedAt = None
                  Definitions = []
                  Translations = [] }

            Assert.Equal(expectedEntry1, sortedResult.[0])
            Assert.Equal(expectedEntry2, sortedResult.[1])
        }

    [<Fact>]
    member _.``GET by vocabulary id returns empty list when no entries``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(309, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None DateTimeOffset.UtcNow None

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                Urls.Entries.entriesByVocabulary vocabulary.Id

            let! response = client.GetAsync(url)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! result = response.Content.ReadFromJsonAsync<EntryResponse[]>()

            Assert.NotNull(result)
            Assert.Empty(result)
        }

    [<Fact>]
    member _.``GET by vocabulary id without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let url = Urls.Entries.entriesByVocabulary 1

            let! response = client.GetAsync(url)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``GET by vocabulary id for non-existent vocabulary fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(310, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                Urls.Entries.entriesByVocabulary 999999

            let! response = client.GetAsync(url)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``PUT updates entry with new definitions and translations``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(311, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None DateTimeOffset.UtcNow None

            let entry =
                Entities.makeEntry vocabulary "hello" DateTimeOffset.UtcNow None

            let oldDefinition =
                Entities.makeDefinition
                    entry
                    "old greeting"
                    Wordfolio.Api.DataAccess.Definitions.DefinitionSource.Manual
                    0

            let oldTranslation =
                Entities.makeTranslation
                    entry
                    "viejo hola"
                    Wordfolio.Api.DataAccess.Translations.TranslationSource.Manual
                    0

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.addDefinitions [ oldDefinition ]
                |> Seeder.addTranslations [ oldTranslation ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: UpdateEntryRequest =
                { EntryText = "hello updated"
                  Definitions =
                    [ { DefinitionText = "a new greeting"
                        Source = DefinitionSourceDto.Manual
                        Examples =
                          [ { ExampleText = "Hello there!"
                              Source = ExampleSourceDto.Custom } ] } ]
                  Translations =
                    [ { TranslationText = "hola actualizado"
                        Source = TranslationSourceDto.Manual
                        Examples =
                          [ { ExampleText = "Hola ahi!"
                              Source = ExampleSourceDto.Custom } ] } ] }

            let url = Urls.Entries.entryById entry.Id

            let! response = client.PutAsJsonAsync(url, request)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let! result = response.Content.ReadFromJsonAsync<EntryResponse>()

            Assert.NotNull(result)
            Assert.True(result.UpdatedAt.IsSome)

            let expectedDefinitionExample: ExampleResponse =
                { Id = result.Definitions.[0].Examples.[0].Id
                  ExampleText = "Hello there!"
                  Source = ExampleSourceDto.Custom }

            let expectedDefinition: DefinitionResponse =
                { Id = result.Definitions.[0].Id
                  DefinitionText = "a new greeting"
                  Source = DefinitionSourceDto.Manual
                  DisplayOrder = 0
                  Examples = [ expectedDefinitionExample ] }

            let expectedTranslationExample: ExampleResponse =
                { Id = result.Translations.[0].Examples.[0].Id
                  ExampleText = "Hola ahi!"
                  Source = ExampleSourceDto.Custom }

            let expectedTranslation: TranslationResponse =
                { Id = result.Translations.[0].Id
                  TranslationText = "hola actualizado"
                  Source = TranslationSourceDto.Manual
                  DisplayOrder = 0
                  Examples = [ expectedTranslationExample ] }

            let expectedEntry: EntryResponse =
                { Id = entry.Id
                  VocabularyId = vocabulary.Id
                  EntryText = "hello updated"
                  CreatedAt = result.CreatedAt
                  UpdatedAt = result.UpdatedAt
                  Definitions = [ expectedDefinition ]
                  Translations = [ expectedTranslation ] }

            Assert.Equal(expectedEntry, result)

            let! dbEntries = Seeder.getAllEntriesAsync fixture.WordfolioSeeder

            let expectedDbEntries =
                [ { dbEntries.[0] with
                      EntryText = "hello updated"
                      VocabularyId = vocabulary.Id } ]

            Assert.Equal<Entry list>(expectedDbEntries, dbEntries)

            let! dbDefinitions = Seeder.getAllDefinitionsAsync fixture.WordfolioSeeder

            let expectedDbDefinitions =
                [ { dbDefinitions.[0] with
                      DefinitionText = "a new greeting"
                      Source = Wordfolio.Api.DataAccess.Definitions.DefinitionSource.Manual
                      DisplayOrder = 0 } ]

            Assert.Equal<Definition list>(expectedDbDefinitions, dbDefinitions)

            let! dbTranslations = Seeder.getAllTranslationsAsync fixture.WordfolioSeeder

            let expectedDbTranslations =
                [ { dbTranslations.[0] with
                      TranslationText = "hola actualizado"
                      Source = Wordfolio.Api.DataAccess.Translations.TranslationSource.Manual
                      DisplayOrder = 0 } ]

            Assert.Equal<Translation list>(expectedDbTranslations, dbTranslations)

            let! dbExamples = Seeder.getAllExamplesAsync fixture.WordfolioSeeder

            let dbExamplesSorted =
                dbExamples
                |> List.sortBy(fun e -> e.ExampleText)

            let expectedDbExamples =
                [ { dbExamplesSorted.[0] with
                      ExampleText = "Hello there!"
                      Source = Wordfolio.Api.DataAccess.Examples.ExampleSource.Custom }
                  { dbExamplesSorted.[1] with
                      ExampleText = "Hola ahi!"
                      Source = Wordfolio.Api.DataAccess.Examples.ExampleSource.Custom } ]

            Assert.Equal<Example list>(expectedDbExamples, dbExamplesSorted)
        }

    [<Fact>]
    member _.``PUT without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let request: UpdateEntryRequest =
                { EntryText = "hello"
                  Definitions =
                    [ { DefinitionText = "a greeting"
                        Source = DefinitionSourceDto.Manual
                        Examples = [] } ]
                  Translations = [] }

            let url = Urls.Entries.entryById 1

            let! response = client.PutAsJsonAsync(url, request)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``PUT with empty entry text fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(312, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None DateTimeOffset.UtcNow None

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

            let request: UpdateEntryRequest =
                { EntryText = ""
                  Definitions =
                    [ { DefinitionText = "a greeting"
                        Source = DefinitionSourceDto.Manual
                        Examples = [] } ]
                  Translations = [] }

            let url = Urls.Entries.entryById entry.Id

            let! response = client.PutAsJsonAsync(url, request)

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }

    [<Fact>]
    member _.``PUT with no definitions or translations fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(313, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None DateTimeOffset.UtcNow None

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

            let request: UpdateEntryRequest =
                { EntryText = "hello"
                  Definitions = []
                  Translations = [] }

            let url = Urls.Entries.entryById entry.Id

            let! response = client.PutAsJsonAsync(url, request)

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }

    [<Fact>]
    member _.``PUT for non-existent entry fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(314, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: UpdateEntryRequest =
                { EntryText = "hello"
                  Definitions =
                    [ { DefinitionText = "a greeting"
                        Source = DefinitionSourceDto.Manual
                        Examples = [] } ]
                  Translations = [] }

            let url = Urls.Entries.entryById 999999

            let! response = client.PutAsJsonAsync(url, request)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``PUT with duplicate entry text in same vocabulary fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(315, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None DateTimeOffset.UtcNow None

            let entry1 =
                Entities.makeEntry vocabulary "hello" DateTimeOffset.UtcNow None

            let entry2 =
                Entities.makeEntry vocabulary "world" DateTimeOffset.UtcNow None

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry1; entry2 ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: UpdateEntryRequest =
                { EntryText = "hello"
                  Definitions =
                    [ { DefinitionText = "a greeting"
                        Source = DefinitionSourceDto.Manual
                        Examples = [] } ]
                  Translations = [] }

            let url = Urls.Entries.entryById entry2.Id

            let! response = client.PutAsJsonAsync(url, request)

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode)
        }

    [<Fact>]
    member _.``PUT with same entry text succeeds``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(316, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None DateTimeOffset.UtcNow None

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

            let request: UpdateEntryRequest =
                { EntryText = "hello"
                  Definitions =
                    [ { DefinitionText = "updated definition"
                        Source = DefinitionSourceDto.Manual
                        Examples = [] } ]
                  Translations = [] }

            let url = Urls.Entries.entryById entry.Id

            let! response = client.PutAsJsonAsync(url, request)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let! result = response.Content.ReadFromJsonAsync<EntryResponse>()

            Assert.NotNull(result)

            let expectedDefinition: DefinitionResponse =
                { Id = result.Definitions.[0].Id
                  DefinitionText = "updated definition"
                  Source = DefinitionSourceDto.Manual
                  DisplayOrder = 0
                  Examples = [] }

            let expectedEntry: EntryResponse =
                { Id = entry.Id
                  VocabularyId = vocabulary.Id
                  EntryText = "hello"
                  CreatedAt = result.CreatedAt
                  UpdatedAt = result.UpdatedAt
                  Definitions = [ expectedDefinition ]
                  Translations = [] }

            Assert.Equal(expectedEntry, result)

            let! dbEntries = Seeder.getAllEntriesAsync fixture.WordfolioSeeder

            let expectedDbEntries =
                [ { dbEntries.[0] with
                      EntryText = "hello"
                      VocabularyId = vocabulary.Id } ]

            Assert.Equal<Entry list>(expectedDbEntries, dbEntries)

            let! dbDefinitions = Seeder.getAllDefinitionsAsync fixture.WordfolioSeeder

            let expectedDbDefinitions =
                [ { dbDefinitions.[0] with
                      DefinitionText = "updated definition"
                      Source = Wordfolio.Api.DataAccess.Definitions.DefinitionSource.Manual
                      DisplayOrder = 0 } ]

            Assert.Equal<Definition list>(expectedDbDefinitions, dbDefinitions)

            let! dbTranslations = Seeder.getAllTranslationsAsync fixture.WordfolioSeeder
            Assert.Empty(dbTranslations)

            let! dbExamples = Seeder.getAllExamplesAsync fixture.WordfolioSeeder
            Assert.Empty(dbExamples)
        }

    [<Fact>]
    member _.``PUT for another user's entry fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser1, wordfolioUser1 = factory.CreateUserAsync(317, "user1@example.com", "P@ssw0rd!")
            let! identityUser2, wordfolioUser2 = factory.CreateUserAsync(318, "user2@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser1 "Test Collection" None DateTimeOffset.UtcNow None

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None DateTimeOffset.UtcNow None

            let entry =
                Entities.makeEntry vocabulary "hello" DateTimeOffset.UtcNow None

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser1; wordfolioUser2 ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser2)

            let request: UpdateEntryRequest =
                { EntryText = "hello updated"
                  Definitions =
                    [ { DefinitionText = "a greeting"
                        Source = DefinitionSourceDto.Manual
                        Examples = [] } ]
                  Translations = [] }

            let url = Urls.Entries.entryById entry.Id

            let! response = client.PutAsJsonAsync(url, request)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }
