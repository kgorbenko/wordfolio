namespace Wordfolio.Api.Tests.Drafts

open System
open System.Net
open System.Net.Http.Json
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Api.Drafts.Types
open Wordfolio.Api.Api.Types
open Wordfolio.Api.Tests
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

module Urls = Wordfolio.Api.Urls

type CreateDraftTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``POST /drafts creates entry in new default vocabulary``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(400, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: CreateDraftRequest =
                { EntryText = "hello"
                  Definitions =
                    [ { DefinitionText = "a greeting"
                        Source = DefinitionSource.Manual
                        Examples = [] } ]
                  Translations = []
                  AllowDuplicate = None }

            let! response = client.PostAsJsonAsync(Urls.Drafts.Path, request)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.Created, response.StatusCode)

            let! actual = response.Content.ReadFromJsonAsync<EntryResponse>()

            let! vocabularies = Seeder.getAllVocabulariesAsync fixture.WordfolioSeeder
            Assert.Single(vocabularies) |> ignore

            let expectedDefinition: DefinitionResponse =
                { Id = actual.Definitions.[0].Id
                  DefinitionText = "a greeting"
                  Source = DefinitionSource.Manual
                  DisplayOrder = 0
                  Examples = [] }

            let expected: EntryResponse =
                { Id = actual.Id
                  VocabularyId = vocabularies.[0].Id
                  EntryText = "hello"
                  CreatedAt = actual.CreatedAt
                  UpdatedAt = actual.CreatedAt
                  Definitions = [ expectedDefinition ]
                  Translations = [] }

            Assert.Equal(expected, actual)

            let expectedVocabulary: Wordfolio.Vocabulary =
                { vocabularies.[0] with
                    Name = "[Default]"
                    Description = None
                    IsDefault = true }

            Assert.Equal<Wordfolio.Vocabulary list>([ expectedVocabulary ], vocabularies)

            let! collections = Seeder.getAllCollectionsAsync fixture.WordfolioSeeder
            Assert.Single(collections) |> ignore

            let expectedCollection: Wordfolio.Collection =
                { collections.[0] with
                    Name = "[System] Unsorted"
                    Description = None
                    IsSystem = true }

            Assert.Equal<Wordfolio.Collection list>([ expectedCollection ], collections)
            Assert.Equal(vocabularies.[0].CollectionId, collections.[0].Id)
        }

    [<Fact>]
    member _.``POST /drafts uses existing default vocabulary``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(401, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let systemCollection =
                Entities.makeCollection wordfolioUser "[System] Unsorted" None now now true

            let defaultVocabulary =
                Entities.makeVocabulary systemCollection "[Default]" None now now true

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ systemCollection ]
                |> Seeder.addVocabularies [ defaultVocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: CreateDraftRequest =
                { EntryText = "hello"
                  Definitions =
                    [ { DefinitionText = "a greeting"
                        Source = DefinitionSource.Manual
                        Examples = [] } ]
                  Translations = []
                  AllowDuplicate = None }

            let! response = client.PostAsJsonAsync(Urls.Drafts.Path, request)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.Created, response.StatusCode)

            let! actual = response.Content.ReadFromJsonAsync<EntryResponse>()

            let expectedDefinition: DefinitionResponse =
                { Id = actual.Definitions.[0].Id
                  DefinitionText = "a greeting"
                  Source = DefinitionSource.Manual
                  DisplayOrder = 0
                  Examples = [] }

            let expected: EntryResponse =
                { Id = actual.Id
                  VocabularyId = defaultVocabulary.Id
                  EntryText = "hello"
                  CreatedAt = actual.CreatedAt
                  UpdatedAt = actual.CreatedAt
                  Definitions = [ expectedDefinition ]
                  Translations = [] }

            Assert.Equal(expected, actual)

            let! vocabularies = Seeder.getAllVocabulariesAsync fixture.WordfolioSeeder
            Assert.Single(vocabularies) |> ignore

            let expectedVocabulary: Wordfolio.Vocabulary =
                { vocabularies.[0] with
                    Name = "[Default]"
                    Description = None
                    IsDefault = true }

            Assert.Equal<Wordfolio.Vocabulary list>([ expectedVocabulary ], vocabularies)

            let! collections = Seeder.getAllCollectionsAsync fixture.WordfolioSeeder
            Assert.Single(collections) |> ignore

            let expectedCollection: Wordfolio.Collection =
                { collections.[0] with
                    Name = "[System] Unsorted"
                    Description = None
                    IsSystem = true }

            Assert.Equal<Wordfolio.Collection list>([ expectedCollection ], collections)
        }

    [<Fact>]
    member _.``POST /drafts creates draft with definitions and translations``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(600, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: CreateDraftRequest =
                { EntryText = "hello"
                  Definitions =
                    [ { DefinitionText = "a greeting"
                        Source = DefinitionSource.Manual
                        Examples =
                          [ { ExampleText = "Hello, world!"
                              Source = ExampleSource.Custom } ] } ]
                  Translations =
                    [ { TranslationText = "hola"
                        Source = TranslationSource.Manual
                        Examples =
                          [ { ExampleText = "Hola, mundo!"
                              Source = ExampleSource.Custom } ] } ]
                  AllowDuplicate = None }

            let! response = client.PostAsJsonAsync(Urls.Drafts.Path, request)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.Created, response.StatusCode)

            let! actual = response.Content.ReadFromJsonAsync<EntryResponse>()

            let! vocabularies = Seeder.getAllVocabulariesAsync fixture.WordfolioSeeder

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
                { Id = actual.Id
                  VocabularyId = vocabularies.[0].Id
                  EntryText = "hello"
                  CreatedAt = actual.CreatedAt
                  UpdatedAt = actual.CreatedAt
                  Definitions = [ expectedDefinition ]
                  Translations = [ expectedTranslation ] }

            Assert.Equal(expected, actual)

            let! dbEntries = Seeder.getAllEntriesAsync fixture.WordfolioSeeder

            let expectedDbEntries =
                [ { dbEntries.[0] with
                      EntryText = "hello"
                      VocabularyId = vocabularies.[0].Id } ]

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
    member _.``POST /drafts without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let request: CreateDraftRequest =
                { EntryText = "hello"
                  Definitions =
                    [ { DefinitionText = "a greeting"
                        Source = DefinitionSource.Manual
                        Examples = [] } ]
                  Translations = []
                  AllowDuplicate = None }

            let! response = client.PostAsJsonAsync(Urls.Drafts.Path, request)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)

            let! dbCollections = Seeder.getAllCollectionsAsync fixture.WordfolioSeeder
            Assert.Equal<Wordfolio.Collection list>([], dbCollections)

            let! dbVocabularies = Seeder.getAllVocabulariesAsync fixture.WordfolioSeeder
            Assert.Equal<Wordfolio.Vocabulary list>([], dbVocabularies)

            let! dbEntries = Seeder.getAllEntriesAsync fixture.WordfolioSeeder
            Assert.Equal<Entry list>([], dbEntries)

            let! dbDefinitions = Seeder.getAllDefinitionsAsync fixture.WordfolioSeeder
            Assert.Equal<Definition list>([], dbDefinitions)

            let! dbTranslations = Seeder.getAllTranslationsAsync fixture.WordfolioSeeder
            Assert.Equal<Translation list>([], dbTranslations)

            let! dbExamples = Seeder.getAllExamplesAsync fixture.WordfolioSeeder
            Assert.Equal<Example list>([], dbExamples)
        }

    [<Fact>]
    member _.``POST /drafts with empty entry text fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(601, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: CreateDraftRequest =
                { EntryText = ""
                  Definitions =
                    [ { DefinitionText = "a greeting"
                        Source = DefinitionSource.Manual
                        Examples = [] } ]
                  Translations = []
                  AllowDuplicate = None }

            let! response = client.PostAsJsonAsync(Urls.Drafts.Path, request)

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)

            let! dbCollections = Seeder.getAllCollectionsAsync fixture.WordfolioSeeder
            Assert.Equal<Wordfolio.Collection list>([], dbCollections)

            let! dbVocabularies = Seeder.getAllVocabulariesAsync fixture.WordfolioSeeder
            Assert.Equal<Wordfolio.Vocabulary list>([], dbVocabularies)

            let! dbEntries = Seeder.getAllEntriesAsync fixture.WordfolioSeeder
            Assert.Equal<Entry list>([], dbEntries)

            let! dbDefinitions = Seeder.getAllDefinitionsAsync fixture.WordfolioSeeder
            Assert.Equal<Definition list>([], dbDefinitions)

            let! dbTranslations = Seeder.getAllTranslationsAsync fixture.WordfolioSeeder
            Assert.Equal<Translation list>([], dbTranslations)

            let! dbExamples = Seeder.getAllExamplesAsync fixture.WordfolioSeeder
            Assert.Equal<Example list>([], dbExamples)
        }

    [<Fact>]
    member _.``POST /drafts with no definitions or translations fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(602, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: CreateDraftRequest =
                { EntryText = "hello"
                  Definitions = []
                  Translations = []
                  AllowDuplicate = None }

            let! response = client.PostAsJsonAsync(Urls.Drafts.Path, request)

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)

            let! dbCollections = Seeder.getAllCollectionsAsync fixture.WordfolioSeeder
            Assert.Equal<Wordfolio.Collection list>([], dbCollections)

            let! dbVocabularies = Seeder.getAllVocabulariesAsync fixture.WordfolioSeeder
            Assert.Equal<Wordfolio.Vocabulary list>([], dbVocabularies)

            let! dbEntries = Seeder.getAllEntriesAsync fixture.WordfolioSeeder
            Assert.Equal<Entry list>([], dbEntries)

            let! dbDefinitions = Seeder.getAllDefinitionsAsync fixture.WordfolioSeeder
            Assert.Equal<Definition list>([], dbDefinitions)

            let! dbTranslations = Seeder.getAllTranslationsAsync fixture.WordfolioSeeder
            Assert.Equal<Translation list>([], dbTranslations)

            let! dbExamples = Seeder.getAllExamplesAsync fixture.WordfolioSeeder
            Assert.Equal<Example list>([], dbExamples)
        }

    [<Fact>]
    member _.``POST /drafts with too many examples fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(603, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: CreateDraftRequest =
                { EntryText = "hello"
                  Definitions =
                    [ { DefinitionText = "a greeting"
                        Source = DefinitionSource.Manual
                        Examples =
                          [ { ExampleText = "Example 1"
                              Source = ExampleSource.Custom }
                            { ExampleText = "Example 2"
                              Source = ExampleSource.Custom }
                            { ExampleText = "Example 3"
                              Source = ExampleSource.Custom }
                            { ExampleText = "Example 4"
                              Source = ExampleSource.Custom }
                            { ExampleText = "Example 5"
                              Source = ExampleSource.Custom }
                            { ExampleText = "Example 6"
                              Source = ExampleSource.Custom } ] } ]
                  Translations = []
                  AllowDuplicate = None }

            let! response = client.PostAsJsonAsync(Urls.Drafts.Path, request)

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)

            let! dbCollections = Seeder.getAllCollectionsAsync fixture.WordfolioSeeder
            Assert.Equal<Wordfolio.Collection list>([], dbCollections)

            let! dbVocabularies = Seeder.getAllVocabulariesAsync fixture.WordfolioSeeder
            Assert.Equal<Wordfolio.Vocabulary list>([], dbVocabularies)

            let! dbEntries = Seeder.getAllEntriesAsync fixture.WordfolioSeeder
            Assert.Equal<Entry list>([], dbEntries)

            let! dbDefinitions = Seeder.getAllDefinitionsAsync fixture.WordfolioSeeder
            Assert.Equal<Definition list>([], dbDefinitions)

            let! dbTranslations = Seeder.getAllTranslationsAsync fixture.WordfolioSeeder
            Assert.Equal<Translation list>([], dbTranslations)

            let! dbExamples = Seeder.getAllExamplesAsync fixture.WordfolioSeeder
            Assert.Equal<Example list>([], dbExamples)
        }

    [<Fact>]
    member _.``POST /drafts with duplicate entry text fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(604, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let systemCollection =
                Entities.makeCollection wordfolioUser "[System] Unsorted" None now now true

            let defaultVocabulary =
                Entities.makeVocabulary systemCollection "[Default]" None now now true

            let existingEntry =
                Entities.makeEntry defaultVocabulary "hello" now now

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ systemCollection ]
                |> Seeder.addVocabularies [ defaultVocabulary ]
                |> Seeder.addEntries [ existingEntry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: CreateDraftRequest =
                { EntryText = "hello"
                  Definitions =
                    [ { DefinitionText = "a greeting"
                        Source = DefinitionSource.Manual
                        Examples = [] } ]
                  Translations = []
                  AllowDuplicate = None }

            let! response = client.PostAsJsonAsync(Urls.Drafts.Path, request)

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode)
        }

    [<Fact>]
    member _.``POST /drafts creates duplicate draft when AllowDuplicate is true``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(605, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let systemCollection =
                Entities.makeCollection wordfolioUser "[System] Unsorted" None now now true

            let defaultVocabulary =
                Entities.makeVocabulary systemCollection "[Default]" None now now true

            let existingEntry =
                Entities.makeEntry defaultVocabulary "hello" now now

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ systemCollection ]
                |> Seeder.addVocabularies [ defaultVocabulary ]
                |> Seeder.addEntries [ existingEntry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: CreateDraftRequest =
                { EntryText = "hello"
                  Definitions =
                    [ { DefinitionText = "a greeting"
                        Source = DefinitionSource.Manual
                        Examples = [] } ]
                  Translations = []
                  AllowDuplicate = Some true }

            let! response = client.PostAsJsonAsync(Urls.Drafts.Path, request)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.Created, response.StatusCode)

            let! actual = response.Content.ReadFromJsonAsync<EntryResponse>()

            let expectedDefinition: DefinitionResponse =
                { Id = actual.Definitions.[0].Id
                  DefinitionText = "a greeting"
                  Source = DefinitionSource.Manual
                  DisplayOrder = 0
                  Examples = [] }

            let expected: EntryResponse =
                { Id = actual.Id
                  VocabularyId = defaultVocabulary.Id
                  EntryText = "hello"
                  CreatedAt = actual.CreatedAt
                  UpdatedAt = actual.CreatedAt
                  Definitions = [ expectedDefinition ]
                  Translations = [] }

            Assert.Equal(expected, actual)

            let! dbEntries = Seeder.getAllEntriesAsync fixture.WordfolioSeeder

            let dbEntriesSorted =
                dbEntries |> List.sortBy(fun e -> e.Id)

            let expectedDbEntries =
                [ { dbEntriesSorted.[0] with
                      EntryText = "hello"
                      VocabularyId = defaultVocabulary.Id }
                  { dbEntriesSorted.[1] with
                      EntryText = "hello"
                      VocabularyId = defaultVocabulary.Id } ]

            Assert.Equal<Entry list>(expectedDbEntries, dbEntriesSorted)
        }
