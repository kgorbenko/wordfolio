namespace Wordfolio.Api.Tests

open System
open System.Net
open System.Net.Http.Json
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Api.Drafts.Types
open Wordfolio.Api.Api.Entries.Types
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

            let! response = client.GetAsync(Urls.Drafts.allDrafts())

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

            let! response = client.GetAsync(Urls.Drafts.allDrafts())

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

            let! response = client.GetAsync(Urls.Drafts.allDrafts())
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

            let! response = client.GetAsync(Urls.Drafts.allDrafts())
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

            let! response = client.GetAsync(Urls.Drafts.allDrafts())
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
                        Source = DefinitionSourceDto.Manual
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
                  Source = DefinitionSourceDto.Manual
                  DisplayOrder = 0
                  Examples = [] }

            let expected: EntryResponse =
                { Id = actual.Id
                  VocabularyId = vocabularies.[0].Id
                  EntryText = "hello"
                  CreatedAt = actual.CreatedAt
                  UpdatedAt = None
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

            let systemCollection =
                Entities.makeCollection wordfolioUser "[System] Unsorted" None DateTimeOffset.UtcNow None true

            let defaultVocabulary =
                Entities.makeVocabulary systemCollection "[Default]" None DateTimeOffset.UtcNow None true

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
                        Source = DefinitionSourceDto.Manual
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
                  Source = DefinitionSourceDto.Manual
                  DisplayOrder = 0
                  Examples = [] }

            let expected: EntryResponse =
                { Id = actual.Id
                  VocabularyId = defaultVocabulary.Id
                  EntryText = "hello"
                  CreatedAt = actual.CreatedAt
                  UpdatedAt = None
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
                        Source = DefinitionSourceDto.Manual
                        Examples =
                          [ { ExampleText = "Hello, world!"
                              Source = ExampleSourceDto.Custom } ] } ]
                  Translations =
                    [ { TranslationText = "hola"
                        Source = TranslationSourceDto.Manual
                        Examples =
                          [ { ExampleText = "Hola, mundo!"
                              Source = ExampleSourceDto.Custom } ] } ]
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
                { Id = actual.Id
                  VocabularyId = vocabularies.[0].Id
                  EntryText = "hello"
                  CreatedAt = actual.CreatedAt
                  UpdatedAt = None
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
                        Source = DefinitionSourceDto.Manual
                        Examples = [] } ]
                  Translations = []
                  AllowDuplicate = None }

            let! response = client.PostAsJsonAsync(Urls.Drafts.Path, request)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
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
                        Source = DefinitionSourceDto.Manual
                        Examples = [] } ]
                  Translations = []
                  AllowDuplicate = None }

            let! response = client.PostAsJsonAsync(Urls.Drafts.Path, request)

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
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
                  Translations = []
                  AllowDuplicate = None }

            let! response = client.PostAsJsonAsync(Urls.Drafts.Path, request)

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }

    [<Fact>]
    member _.``POST /drafts with duplicate entry text fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(604, "user@example.com", "P@ssw0rd!")

            let systemCollection =
                Entities.makeCollection wordfolioUser "[System] Unsorted" None DateTimeOffset.UtcNow None true

            let defaultVocabulary =
                Entities.makeVocabulary systemCollection "[Default]" None DateTimeOffset.UtcNow None true

            let existingEntry =
                Entities.makeEntry defaultVocabulary "hello" DateTimeOffset.UtcNow None

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
                        Source = DefinitionSourceDto.Manual
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

            let systemCollection =
                Entities.makeCollection wordfolioUser "[System] Unsorted" None DateTimeOffset.UtcNow None true

            let defaultVocabulary =
                Entities.makeVocabulary systemCollection "[Default]" None DateTimeOffset.UtcNow None true

            let existingEntry =
                Entities.makeEntry defaultVocabulary "hello" DateTimeOffset.UtcNow None

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
                        Source = DefinitionSourceDto.Manual
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
                  Source = DefinitionSourceDto.Manual
                  DisplayOrder = 0
                  Examples = [] }

            let expected: EntryResponse =
                { Id = actual.Id
                  VocabularyId = defaultVocabulary.Id
                  EntryText = "hello"
                  CreatedAt = actual.CreatedAt
                  UpdatedAt = None
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

    [<Fact>]
    member _.``GET draft by id returns entry with hierarchy``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(700, "user@example.com", "P@ssw0rd!")

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

            let url = Urls.Drafts.draftById entry.Id

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
    member _.``GET draft by id returns 404 when entry does not exist``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(701, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url = Urls.Drafts.draftById 999999

            let! response = client.GetAsync(url)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``GET draft by id without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let url = Urls.Drafts.draftById 1

            let! response = client.GetAsync(url)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``PUT draft updates entry with new definitions and translations``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(702, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None false

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None DateTimeOffset.UtcNow None false

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

            let url = Urls.Drafts.draftById entry.Id

            let! response = client.PutAsJsonAsync(url, request)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let! actual = response.Content.ReadFromJsonAsync<EntryResponse>()

            let expectedDefinitionExample: ExampleResponse =
                { Id = actual.Definitions.[0].Examples.[0].Id
                  ExampleText = "Hello there!"
                  Source = ExampleSourceDto.Custom }

            let expectedDefinition: DefinitionResponse =
                { Id = actual.Definitions.[0].Id
                  DefinitionText = "a new greeting"
                  Source = DefinitionSourceDto.Manual
                  DisplayOrder = 0
                  Examples = [ expectedDefinitionExample ] }

            let expectedTranslationExample: ExampleResponse =
                { Id = actual.Translations.[0].Examples.[0].Id
                  ExampleText = "Hola ahi!"
                  Source = ExampleSourceDto.Custom }

            let expectedTranslation: TranslationResponse =
                { Id = actual.Translations.[0].Id
                  TranslationText = "hola actualizado"
                  Source = TranslationSourceDto.Manual
                  DisplayOrder = 0
                  Examples = [ expectedTranslationExample ] }

            let expected: EntryResponse =
                { Id = entry.Id
                  VocabularyId = vocabulary.Id
                  EntryText = "hello updated"
                  CreatedAt = actual.CreatedAt
                  UpdatedAt = actual.UpdatedAt
                  Definitions = [ expectedDefinition ]
                  Translations = [ expectedTranslation ] }

            Assert.Equal(expected, actual)

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
    member _.``PUT draft without authentication fails``() : Task =
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

            let url = Urls.Drafts.draftById 1

            let! response = client.PutAsJsonAsync(url, request)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``PUT draft with empty entry text fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(703, "user@example.com", "P@ssw0rd!")

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

            let request: UpdateEntryRequest =
                { EntryText = ""
                  Definitions =
                    [ { DefinitionText = "a greeting"
                        Source = DefinitionSourceDto.Manual
                        Examples = [] } ]
                  Translations = [] }

            let url = Urls.Drafts.draftById entry.Id

            let! response = client.PutAsJsonAsync(url, request)

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }

    [<Fact>]
    member _.``PUT draft with no definitions or translations fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(704, "user@example.com", "P@ssw0rd!")

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

            let request: UpdateEntryRequest =
                { EntryText = "hello"
                  Definitions = []
                  Translations = [] }

            let url = Urls.Drafts.draftById entry.Id

            let! response = client.PutAsJsonAsync(url, request)

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }

    [<Fact>]
    member _.``PUT draft for non-existent entry fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(705, "user@example.com", "P@ssw0rd!")

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

            let url = Urls.Drafts.draftById 999999

            let! response = client.PutAsJsonAsync(url, request)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``DELETE draft deletes entry successfully``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(706, "user@example.com", "P@ssw0rd!")

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

            let url = Urls.Drafts.draftById entry.Id

            let! response = client.DeleteAsync(url)

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode)

            let! actual = Seeder.getEntryByIdAsync entry.Id fixture.WordfolioSeeder
            let expected: Entry option = None

            Assert.Equal<Entry option>(expected, actual)
        }

    [<Fact>]
    member _.``DELETE draft without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! _, wordfolioUser = factory.CreateUserAsync(707, "user@example.com", "P@ssw0rd!")

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

            let url = Urls.Drafts.draftById entry.Id

            let! response = client.DeleteAsync(url)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
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
    member _.``POST move draft updates entry vocabulary for owned source and target``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(709, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None false

            let sourceVocabulary =
                Entities.makeVocabulary collection "Source" None DateTimeOffset.UtcNow None false

            let targetVocabulary =
                Entities.makeVocabulary collection "Target" None DateTimeOffset.UtcNow None false

            let entry =
                Entities.makeEntry sourceVocabulary "hello" DateTimeOffset.UtcNow None

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ sourceVocabulary; targetVocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: MoveEntryRequest =
                { VocabularyId = targetVocabulary.Id }

            let url = Urls.Drafts.moveDraftById entry.Id

            let! response = client.PostAsJsonAsync(url, request)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let! actual = response.Content.ReadFromJsonAsync<EntryResponse>()

            let expected: EntryResponse =
                { Id = entry.Id
                  VocabularyId = targetVocabulary.Id
                  EntryText = "hello"
                  CreatedAt = actual.CreatedAt
                  UpdatedAt = actual.UpdatedAt
                  Definitions = []
                  Translations = [] }

            Assert.Equal(expected, actual)

            let! movedEntry = Seeder.getEntryByIdAsync entry.Id fixture.WordfolioSeeder

            let expectedMovedEntry: Entry option =
                Some
                    { Id = entry.Id
                      VocabularyId = targetVocabulary.Id
                      EntryText = "hello"
                      CreatedAt = actual.CreatedAt
                      UpdatedAt = actual.UpdatedAt }

            Assert.Equal(expectedMovedEntry, movedEntry)
        }

    [<Fact>]
    member _.``POST move draft without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let request: MoveEntryRequest =
                { VocabularyId = 1 }

            let! response = client.PostAsJsonAsync(Urls.Drafts.moveDraftById 1, request)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``POST move draft for non-existent entry returns 404``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(710, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: MoveEntryRequest =
                { VocabularyId = 999 }

            let! response = client.PostAsJsonAsync(Urls.Drafts.moveDraftById 999999, request)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``POST move draft returns 404 when target vocabulary does not exist``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(711, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None false

            let sourceVocabulary =
                Entities.makeVocabulary collection "Source" None DateTimeOffset.UtcNow None false

            let entry =
                Entities.makeEntry sourceVocabulary "hello" DateTimeOffset.UtcNow None

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ sourceVocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: MoveEntryRequest =
                { VocabularyId = 999999 }

            let! response = client.PostAsJsonAsync(Urls.Drafts.moveDraftById entry.Id, request)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``PUT draft with same entry text succeeds``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(712, "user@example.com", "P@ssw0rd!")

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

            let request: UpdateEntryRequest =
                { EntryText = "hello"
                  Definitions =
                    [ { DefinitionText = "updated definition"
                        Source = DefinitionSourceDto.Manual
                        Examples = [] } ]
                  Translations = [] }

            let url = Urls.Drafts.draftById entry.Id

            let! response = client.PutAsJsonAsync(url, request)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let! actual = response.Content.ReadFromJsonAsync<EntryResponse>()

            let expectedDefinition: DefinitionResponse =
                { Id = actual.Definitions.[0].Id
                  DefinitionText = "updated definition"
                  Source = DefinitionSourceDto.Manual
                  DisplayOrder = 0
                  Examples = [] }

            let expected: EntryResponse =
                { Id = entry.Id
                  VocabularyId = vocabulary.Id
                  EntryText = "hello"
                  CreatedAt = actual.CreatedAt
                  UpdatedAt = actual.UpdatedAt
                  Definitions = [ expectedDefinition ]
                  Translations = [] }

            Assert.Equal(expected, actual)

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
    member _.``PUT draft for another user's entry fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! _, wordfolioUser1 = factory.CreateUserAsync(713, "user1@example.com", "P@ssw0rd!")
            let! identityUser2, wordfolioUser2 = factory.CreateUserAsync(714, "user2@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser1 "Test Collection" None DateTimeOffset.UtcNow None false

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None DateTimeOffset.UtcNow None false

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

            let url = Urls.Drafts.draftById entry.Id

            let! response = client.PutAsJsonAsync(url, request)

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

            let collection =
                Entities.makeCollection wordfolioUser1 "Test Collection" None DateTimeOffset.UtcNow None false

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None DateTimeOffset.UtcNow None false

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

            let url = Urls.Drafts.draftById entry.Id

            let! response = client.DeleteAsync(url)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``DELETE draft cascades to delete definitions, translations, and examples``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(717, "user@example.com", "P@ssw0rd!")

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

    [<Fact>]
    member _.``POST move draft returns 404 when target vocabulary belongs to another user``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser1, wordfolioUser1 = factory.CreateUserAsync(718, "user1@example.com", "P@ssw0rd!")
            let! _, wordfolioUser2 = factory.CreateUserAsync(719, "user2@example.com", "P@ssw0rd!")

            let collection1 =
                Entities.makeCollection wordfolioUser1 "Collection 1" None DateTimeOffset.UtcNow None false

            let sourceVocabulary =
                Entities.makeVocabulary collection1 "Source" None DateTimeOffset.UtcNow None false

            let entry =
                Entities.makeEntry sourceVocabulary "hello" DateTimeOffset.UtcNow None

            let collection2 =
                Entities.makeCollection wordfolioUser2 "Collection 2" None DateTimeOffset.UtcNow None false

            let foreignTargetVocabulary =
                Entities.makeVocabulary collection2 "Target" None DateTimeOffset.UtcNow None false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser1; wordfolioUser2 ]
                |> Seeder.addCollections [ collection1; collection2 ]
                |> Seeder.addVocabularies [ sourceVocabulary; foreignTargetVocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser1)

            let request: MoveEntryRequest =
                { VocabularyId = foreignTargetVocabulary.Id }

            let! response = client.PostAsJsonAsync(Urls.Drafts.moveDraftById entry.Id, request)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``POST move draft to default vocabulary succeeds``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(720, "user@example.com", "P@ssw0rd!")

            let systemCollection =
                Entities.makeCollection wordfolioUser "[System] Unsorted" None DateTimeOffset.UtcNow None true

            let defaultVocabulary =
                Entities.makeVocabulary systemCollection "[Default]" None DateTimeOffset.UtcNow None true

            let regularCollection =
                Entities.makeCollection wordfolioUser "Regular Collection" None DateTimeOffset.UtcNow None false

            let regularVocabulary =
                Entities.makeVocabulary regularCollection "Regular Vocabulary" None DateTimeOffset.UtcNow None false

            let entry =
                Entities.makeEntry regularVocabulary "hello" DateTimeOffset.UtcNow None

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ systemCollection; regularCollection ]
                |> Seeder.addVocabularies [ defaultVocabulary; regularVocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: MoveEntryRequest =
                { VocabularyId = defaultVocabulary.Id }

            let! response = client.PostAsJsonAsync(Urls.Drafts.moveDraftById entry.Id, request)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let! actual = response.Content.ReadFromJsonAsync<EntryResponse>()

            let expected: EntryResponse =
                { Id = entry.Id
                  VocabularyId = defaultVocabulary.Id
                  EntryText = "hello"
                  CreatedAt = actual.CreatedAt
                  UpdatedAt = actual.UpdatedAt
                  Definitions = []
                  Translations = [] }

            Assert.Equal(expected, actual)
        }
