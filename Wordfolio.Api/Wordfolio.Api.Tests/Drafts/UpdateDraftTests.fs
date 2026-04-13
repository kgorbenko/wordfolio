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

type UpdateDraftTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``PUT draft updates entry with new definitions and translations``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(702, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None now now false

            let entry =
                Entities.makeEntry vocabulary "hello" now now

            let untouchedEntry =
                Entities.makeEntry vocabulary "untouched" now now

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

            let untouchedDefinition =
                Entities.makeDefinition
                    untouchedEntry
                    "untouched definition"
                    Wordfolio.Api.DataAccess.Definitions.DefinitionSource.Manual
                    0

            let untouchedTranslation =
                Entities.makeTranslation
                    untouchedEntry
                    "untouched translation"
                    Wordfolio.Api.DataAccess.Translations.TranslationSource.Manual
                    0

            let untouchedDefinitionExample =
                Entities.makeExampleForDefinition
                    untouchedDefinition
                    "Untouched definition example"
                    Wordfolio.Api.DataAccess.Examples.ExampleSource.Custom

            let untouchedTranslationExample =
                Entities.makeExampleForTranslation
                    untouchedTranslation
                    "Untouched translation example"
                    Wordfolio.Api.DataAccess.Examples.ExampleSource.Custom

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry; untouchedEntry ]
                |> Seeder.addDefinitions [ oldDefinition; untouchedDefinition ]
                |> Seeder.addTranslations [ oldTranslation; untouchedTranslation ]
                |> Seeder.addExamples [ untouchedDefinitionExample; untouchedTranslationExample ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: UpdateEntryRequest =
                { EntryText = "hello updated"
                  Definitions =
                    [ { DefinitionText = "a new greeting"
                        Source = DefinitionSource.Manual
                        Examples =
                          [ { ExampleText = "Hello there!"
                              Source = ExampleSource.Custom } ] } ]
                  Translations =
                    [ { TranslationText = "hola actualizado"
                        Source = TranslationSource.Manual
                        Examples =
                          [ { ExampleText = "Hola ahi!"
                              Source = ExampleSource.Custom } ] } ] }

            let url =
                Urls.Drafts.draftById(encoder.Encode entry.Id)

            let! response = client.PutAsJsonAsync(url, request)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let! actual = response.Content.ReadFromJsonAsync<EntryResponse>()

            let expectedDefinitionExample: ExampleResponse =
                { Id = actual.Definitions.[0].Examples.[0].Id
                  ExampleText = "Hello there!"
                  Source = ExampleSource.Custom }

            let expectedDefinition: DefinitionResponse =
                { Id = actual.Definitions.[0].Id
                  DefinitionText = "a new greeting"
                  Source = DefinitionSource.Manual
                  DisplayOrder = 0
                  Examples = [ expectedDefinitionExample ] }

            let expectedTranslationExample: ExampleResponse =
                { Id = actual.Translations.[0].Examples.[0].Id
                  ExampleText = "Hola ahi!"
                  Source = ExampleSource.Custom }

            let expectedTranslation: TranslationResponse =
                { Id = actual.Translations.[0].Id
                  TranslationText = "hola actualizado"
                  Source = TranslationSource.Manual
                  DisplayOrder = 0
                  Examples = [ expectedTranslationExample ] }

            Assert.True(actual.UpdatedAt >= entry.CreatedAt)

            let expected: EntryResponse =
                { Id = encoder.Encode entry.Id
                  VocabularyId = encoder.Encode vocabulary.Id
                  EntryText = "hello updated"
                  CreatedAt = entry.CreatedAt
                  UpdatedAt = actual.UpdatedAt
                  Definitions = [ expectedDefinition ]
                  Translations = [ expectedTranslation ] }

            Assert.Equal(expected, actual)

            let! dbEntries = Seeder.getAllEntriesAsync fixture.WordfolioSeeder

            let dbEntriesSorted =
                dbEntries
                |> List.sortBy(fun dbEntry -> dbEntry.EntryText)

            let expectedDbEntries =
                [ { dbEntriesSorted.[0] with
                      EntryText = "hello updated"
                      VocabularyId = vocabulary.Id }
                  { dbEntriesSorted.[1] with
                      EntryText = "untouched"
                      VocabularyId = vocabulary.Id } ]

            Assert.Equal<Entry list>(expectedDbEntries, dbEntriesSorted)

            let! dbDefinitions = Seeder.getAllDefinitionsAsync fixture.WordfolioSeeder

            let dbDefinitionsSorted =
                dbDefinitions
                |> List.sortBy(fun dbDefinition -> dbDefinition.DefinitionText)

            let expectedDbDefinitions =
                [ { dbDefinitionsSorted.[0] with
                      DefinitionText = "a new greeting"
                      Source = Wordfolio.Api.DataAccess.Definitions.DefinitionSource.Manual
                      DisplayOrder = 0 }
                  { dbDefinitionsSorted.[1] with
                      DefinitionText = "untouched definition"
                      Source = Wordfolio.Api.DataAccess.Definitions.DefinitionSource.Manual
                      DisplayOrder = 0 } ]

            Assert.Equal<Definition list>(expectedDbDefinitions, dbDefinitionsSorted)

            let! dbTranslations = Seeder.getAllTranslationsAsync fixture.WordfolioSeeder

            let dbTranslationsSorted =
                dbTranslations
                |> List.sortBy(fun dbTranslation -> dbTranslation.TranslationText)

            let expectedDbTranslations =
                [ { dbTranslationsSorted.[0] with
                      TranslationText = "hola actualizado"
                      Source = Wordfolio.Api.DataAccess.Translations.TranslationSource.Manual
                      DisplayOrder = 0 }
                  { dbTranslationsSorted.[1] with
                      TranslationText = "untouched translation"
                      Source = Wordfolio.Api.DataAccess.Translations.TranslationSource.Manual
                      DisplayOrder = 0 } ]

            Assert.Equal<Translation list>(expectedDbTranslations, dbTranslationsSorted)

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
                      Source = Wordfolio.Api.DataAccess.Examples.ExampleSource.Custom }
                  { dbExamplesSorted.[2] with
                      ExampleText = "Untouched definition example"
                      Source = Wordfolio.Api.DataAccess.Examples.ExampleSource.Custom }
                  { dbExamplesSorted.[3] with
                      ExampleText = "Untouched translation example"
                      Source = Wordfolio.Api.DataAccess.Examples.ExampleSource.Custom } ]

            Assert.Equal<Example list>(expectedDbExamples, dbExamplesSorted)
        }

    [<Fact>]
    member _.``PUT draft without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! _, wordfolioUser = factory.CreateUserAsync(715, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None now now false

            let entry =
                Entities.makeEntry vocabulary "hello" now now

            let untouchedEntry =
                Entities.makeEntry vocabulary "untouched" now now

            let definition =
                Entities.makeDefinition
                    entry
                    "original definition"
                    Wordfolio.Api.DataAccess.Definitions.DefinitionSource.Manual
                    0

            let untouchedDefinition =
                Entities.makeDefinition
                    untouchedEntry
                    "untouched definition"
                    Wordfolio.Api.DataAccess.Definitions.DefinitionSource.Manual
                    0

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry; untouchedEntry ]
                |> Seeder.addDefinitions [ definition; untouchedDefinition ]
                |> Seeder.saveChangesAsync

            let! dbEntriesBefore = Seeder.getAllEntriesAsync fixture.WordfolioSeeder
            let! dbDefinitionsBefore = Seeder.getAllDefinitionsAsync fixture.WordfolioSeeder
            let! dbTranslationsBefore = Seeder.getAllTranslationsAsync fixture.WordfolioSeeder
            let! dbExamplesBefore = Seeder.getAllExamplesAsync fixture.WordfolioSeeder

            use client = factory.CreateClient()

            let request: UpdateEntryRequest =
                { EntryText = "hello"
                  Definitions =
                    [ { DefinitionText = "a greeting"
                        Source = DefinitionSource.Manual
                        Examples = [] } ]
                  Translations = [] }

            let url =
                Urls.Drafts.draftById(encoder.Encode entry.Id)

            let! response = client.PutAsJsonAsync(url, request)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)

            let! dbEntriesAfter = Seeder.getAllEntriesAsync fixture.WordfolioSeeder
            let! dbDefinitionsAfter = Seeder.getAllDefinitionsAsync fixture.WordfolioSeeder
            let! dbTranslationsAfter = Seeder.getAllTranslationsAsync fixture.WordfolioSeeder
            let! dbExamplesAfter = Seeder.getAllExamplesAsync fixture.WordfolioSeeder

            Assert.Equal<Entry list>(dbEntriesBefore, dbEntriesAfter)
            Assert.Equal<Definition list>(dbDefinitionsBefore, dbDefinitionsAfter)
            Assert.Equal<Translation list>(dbTranslationsBefore, dbTranslationsAfter)
            Assert.Equal<Example list>(dbExamplesBefore, dbExamplesAfter)
        }

    [<Fact>]
    member _.``PUT draft with empty entry text fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(703, "user@example.com", "P@ssw0rd!")

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

            let! dbEntriesBefore = Seeder.getAllEntriesAsync fixture.WordfolioSeeder
            let! dbDefinitionsBefore = Seeder.getAllDefinitionsAsync fixture.WordfolioSeeder
            let! dbTranslationsBefore = Seeder.getAllTranslationsAsync fixture.WordfolioSeeder
            let! dbExamplesBefore = Seeder.getAllExamplesAsync fixture.WordfolioSeeder

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: UpdateEntryRequest =
                { EntryText = ""
                  Definitions =
                    [ { DefinitionText = "a greeting"
                        Source = DefinitionSource.Manual
                        Examples = [] } ]
                  Translations = [] }

            let url =
                Urls.Drafts.draftById(encoder.Encode entry.Id)

            let! response = client.PutAsJsonAsync(url, request)

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)

            let! dbEntriesAfter = Seeder.getAllEntriesAsync fixture.WordfolioSeeder
            let! dbDefinitionsAfter = Seeder.getAllDefinitionsAsync fixture.WordfolioSeeder
            let! dbTranslationsAfter = Seeder.getAllTranslationsAsync fixture.WordfolioSeeder
            let! dbExamplesAfter = Seeder.getAllExamplesAsync fixture.WordfolioSeeder

            Assert.Equal<Entry list>(dbEntriesBefore, dbEntriesAfter)
            Assert.Equal<Definition list>(dbDefinitionsBefore, dbDefinitionsAfter)
            Assert.Equal<Translation list>(dbTranslationsBefore, dbTranslationsAfter)
            Assert.Equal<Example list>(dbExamplesBefore, dbExamplesAfter)
        }

    [<Fact>]
    member _.``PUT draft with no definitions or translations fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(704, "user@example.com", "P@ssw0rd!")

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

            let! dbEntriesBefore = Seeder.getAllEntriesAsync fixture.WordfolioSeeder
            let! dbDefinitionsBefore = Seeder.getAllDefinitionsAsync fixture.WordfolioSeeder
            let! dbTranslationsBefore = Seeder.getAllTranslationsAsync fixture.WordfolioSeeder
            let! dbExamplesBefore = Seeder.getAllExamplesAsync fixture.WordfolioSeeder

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: UpdateEntryRequest =
                { EntryText = "hello"
                  Definitions = []
                  Translations = [] }

            let url =
                Urls.Drafts.draftById(encoder.Encode entry.Id)

            let! response = client.PutAsJsonAsync(url, request)

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)

            let! dbEntriesAfter = Seeder.getAllEntriesAsync fixture.WordfolioSeeder
            let! dbDefinitionsAfter = Seeder.getAllDefinitionsAsync fixture.WordfolioSeeder
            let! dbTranslationsAfter = Seeder.getAllTranslationsAsync fixture.WordfolioSeeder
            let! dbExamplesAfter = Seeder.getAllExamplesAsync fixture.WordfolioSeeder

            Assert.Equal<Entry list>(dbEntriesBefore, dbEntriesAfter)
            Assert.Equal<Definition list>(dbDefinitionsBefore, dbDefinitionsAfter)
            Assert.Equal<Translation list>(dbTranslationsBefore, dbTranslationsAfter)
            Assert.Equal<Example list>(dbExamplesBefore, dbExamplesAfter)
        }

    [<Fact>]
    member _.``PUT draft for non-existent entry fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

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
                        Source = DefinitionSource.Manual
                        Examples = [] } ]
                  Translations = [] }

            let url =
                Urls.Drafts.draftById(encoder.Encode 999999)

            let! response = client.PutAsJsonAsync(url, request)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``PUT draft with same entry text succeeds``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(712, "user@example.com", "P@ssw0rd!")

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

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: UpdateEntryRequest =
                { EntryText = "hello"
                  Definitions =
                    [ { DefinitionText = "updated definition"
                        Source = DefinitionSource.Manual
                        Examples = [] } ]
                  Translations = [] }

            let url =
                Urls.Drafts.draftById(encoder.Encode entry.Id)

            let! response = client.PutAsJsonAsync(url, request)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let! actual = response.Content.ReadFromJsonAsync<EntryResponse>()

            let expectedDefinition: DefinitionResponse =
                { Id = actual.Definitions.[0].Id
                  DefinitionText = "updated definition"
                  Source = DefinitionSource.Manual
                  DisplayOrder = 0
                  Examples = [] }

            Assert.True(actual.UpdatedAt >= entry.CreatedAt)

            let expected: EntryResponse =
                { Id = encoder.Encode entry.Id
                  VocabularyId = encoder.Encode vocabulary.Id
                  EntryText = "hello"
                  CreatedAt = entry.CreatedAt
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

            let encoder = factory.Encoder

            let! _, wordfolioUser1 = factory.CreateUserAsync(713, "user1@example.com", "P@ssw0rd!")
            let! identityUser2, wordfolioUser2 = factory.CreateUserAsync(714, "user2@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser1 "Test Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None now now false

            let entry =
                Entities.makeEntry vocabulary "hello" now now

            let definition =
                Entities.makeDefinition
                    entry
                    "owner definition"
                    Wordfolio.Api.DataAccess.Definitions.DefinitionSource.Manual
                    0

            let translation =
                Entities.makeTranslation
                    entry
                    "owner translation"
                    Wordfolio.Api.DataAccess.Translations.TranslationSource.Manual
                    0

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser1; wordfolioUser2 ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.addDefinitions [ definition ]
                |> Seeder.addTranslations [ translation ]
                |> Seeder.saveChangesAsync

            let! dbEntriesBefore = Seeder.getAllEntriesAsync fixture.WordfolioSeeder
            let! dbDefinitionsBefore = Seeder.getAllDefinitionsAsync fixture.WordfolioSeeder
            let! dbTranslationsBefore = Seeder.getAllTranslationsAsync fixture.WordfolioSeeder
            let! dbExamplesBefore = Seeder.getAllExamplesAsync fixture.WordfolioSeeder

            use! client = factory.CreateAuthenticatedClientAsync(identityUser2)

            let request: UpdateEntryRequest =
                { EntryText = "hello updated"
                  Definitions =
                    [ { DefinitionText = "a greeting"
                        Source = DefinitionSource.Manual
                        Examples = [] } ]
                  Translations = [] }

            let url =
                Urls.Drafts.draftById(encoder.Encode entry.Id)

            let! response = client.PutAsJsonAsync(url, request)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)

            let! dbEntriesAfter = Seeder.getAllEntriesAsync fixture.WordfolioSeeder
            let! dbDefinitionsAfter = Seeder.getAllDefinitionsAsync fixture.WordfolioSeeder
            let! dbTranslationsAfter = Seeder.getAllTranslationsAsync fixture.WordfolioSeeder
            let! dbExamplesAfter = Seeder.getAllExamplesAsync fixture.WordfolioSeeder

            Assert.Equal<Entry list>(dbEntriesBefore, dbEntriesAfter)
            Assert.Equal<Definition list>(dbDefinitionsBefore, dbDefinitionsAfter)
            Assert.Equal<Translation list>(dbTranslationsBefore, dbTranslationsAfter)
            Assert.Equal<Example list>(dbExamplesBefore, dbExamplesAfter)
        }
