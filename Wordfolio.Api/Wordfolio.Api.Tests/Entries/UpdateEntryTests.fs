namespace Wordfolio.Api.Tests.Entries

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

type UpdateEntryTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``PUT updates entry with new definitions and translations``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(311, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None now now false

            let entry =
                Entities.makeEntry vocabulary "hello" now now

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

            let unaffectedEntry =
                Entities.makeEntry vocabulary "stay unchanged" now now

            let unaffectedDefinition =
                Entities.makeDefinition
                    unaffectedEntry
                    "unaffected definition"
                    Wordfolio.Api.DataAccess.Definitions.DefinitionSource.Manual
                    0

            let unaffectedTranslation =
                Entities.makeTranslation
                    unaffectedEntry
                    "unaffected translation"
                    Wordfolio.Api.DataAccess.Translations.TranslationSource.Manual
                    0

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry; unaffectedEntry ]
                |> Seeder.addDefinitions [ oldDefinition; unaffectedDefinition ]
                |> Seeder.addTranslations [ oldTranslation; unaffectedTranslation ]
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
                Urls.Entries.entryById(
                    encoder.Encode collection.Id,
                    encoder.Encode vocabulary.Id,
                    encoder.Encode entry.Id
                )

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

            let updatedEntryInDatabase =
                dbEntries
                |> List.find(fun currentEntry -> currentEntry.Id = entry.Id)

            let unaffectedEntryInDatabase =
                dbEntries
                |> List.find(fun currentEntry -> currentEntry.Id = unaffectedEntry.Id)

            let expectedDbEntries =
                [ { updatedEntryInDatabase with
                      Id = entry.Id
                      EntryText = "hello updated"
                      VocabularyId = vocabulary.Id }
                  { unaffectedEntryInDatabase with
                      Id = unaffectedEntry.Id
                      EntryText = "stay unchanged"
                      VocabularyId = vocabulary.Id } ]
                |> List.sortBy(fun currentEntry -> currentEntry.Id)

            let actualDbEntries =
                dbEntries
                |> List.sortBy(fun currentEntry -> currentEntry.Id)

            Assert.Equal<Entry list>(expectedDbEntries, actualDbEntries)

            let! dbDefinitions = Seeder.getAllDefinitionsAsync fixture.WordfolioSeeder

            let updatedDefinitionInDatabase =
                dbDefinitions
                |> List.find(fun currentDefinition -> currentDefinition.EntryId = entry.Id)

            let unaffectedDefinitionInDatabase =
                dbDefinitions
                |> List.find(fun currentDefinition -> currentDefinition.Id = unaffectedDefinition.Id)

            let expectedDbDefinitions =
                [ { updatedDefinitionInDatabase with
                      EntryId = entry.Id
                      DefinitionText = "a new greeting"
                      Source = Wordfolio.Api.DataAccess.Definitions.DefinitionSource.Manual
                      DisplayOrder = 0 }
                  { unaffectedDefinitionInDatabase with
                      Id = unaffectedDefinition.Id
                      EntryId = unaffectedEntry.Id
                      DefinitionText = "unaffected definition"
                      Source = Wordfolio.Api.DataAccess.Definitions.DefinitionSource.Manual
                      DisplayOrder = 0 } ]
                |> List.sortBy(fun currentDefinition -> currentDefinition.Id)

            let actualDbDefinitions =
                dbDefinitions
                |> List.sortBy(fun currentDefinition -> currentDefinition.Id)

            Assert.Equal<Definition list>(expectedDbDefinitions, actualDbDefinitions)

            let! dbTranslations = Seeder.getAllTranslationsAsync fixture.WordfolioSeeder

            let updatedTranslationInDatabase =
                dbTranslations
                |> List.find(fun currentTranslation -> currentTranslation.EntryId = entry.Id)

            let unaffectedTranslationInDatabase =
                dbTranslations
                |> List.find(fun currentTranslation -> currentTranslation.Id = unaffectedTranslation.Id)

            let expectedDbTranslations =
                [ { updatedTranslationInDatabase with
                      EntryId = entry.Id
                      TranslationText = "hola actualizado"
                      Source = Wordfolio.Api.DataAccess.Translations.TranslationSource.Manual
                      DisplayOrder = 0 }
                  { unaffectedTranslationInDatabase with
                      Id = unaffectedTranslation.Id
                      EntryId = unaffectedEntry.Id
                      TranslationText = "unaffected translation"
                      Source = Wordfolio.Api.DataAccess.Translations.TranslationSource.Manual
                      DisplayOrder = 0 } ]
                |> List.sortBy(fun currentTranslation -> currentTranslation.Id)

            let actualDbTranslations =
                dbTranslations
                |> List.sortBy(fun currentTranslation -> currentTranslation.Id)

            Assert.Equal<Translation list>(expectedDbTranslations, actualDbTranslations)

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

            let encoder = factory.Encoder

            let! _, wordfolioUser = factory.CreateUserAsync(323, "auth@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Auth Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary collection "Auth Vocabulary" None now now false

            let entry =
                Entities.makeEntry vocabulary "protected entry" now now

            let definition =
                Entities.makeDefinition
                    entry
                    "protected definition"
                    Wordfolio.Api.DataAccess.Definitions.DefinitionSource.Manual
                    0

            let translation =
                Entities.makeTranslation
                    entry
                    "protected translation"
                    Wordfolio.Api.DataAccess.Translations.TranslationSource.Manual
                    0

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.addDefinitions [ definition ]
                |> Seeder.addTranslations [ translation ]
                |> Seeder.saveChangesAsync

            use client = factory.CreateClient()

            let request: UpdateEntryRequest =
                { EntryText = "hello"
                  Definitions =
                    [ { DefinitionText = "a greeting"
                        Source = DefinitionSource.Manual
                        Examples = [] } ]
                  Translations = [] }

            let url =
                Urls.Entries.entryById(
                    encoder.Encode collection.Id,
                    encoder.Encode vocabulary.Id,
                    encoder.Encode entry.Id
                )

            let! response = client.PutAsJsonAsync(url, request)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)

            let! dbEntries = Seeder.getAllEntriesAsync fixture.WordfolioSeeder

            let expectedDbEntries =
                [ { dbEntries.[0] with
                      EntryText = "protected entry"
                      VocabularyId = vocabulary.Id } ]

            Assert.Equal<Entry list>(expectedDbEntries, dbEntries)

            let! dbDefinitions = Seeder.getAllDefinitionsAsync fixture.WordfolioSeeder

            let expectedDbDefinitions =
                [ { dbDefinitions.[0] with
                      EntryId = entry.Id
                      DefinitionText = "protected definition"
                      Source = Wordfolio.Api.DataAccess.Definitions.DefinitionSource.Manual
                      DisplayOrder = 0 } ]

            Assert.Equal<Definition list>(expectedDbDefinitions, dbDefinitions)

            let! dbTranslations = Seeder.getAllTranslationsAsync fixture.WordfolioSeeder

            let expectedDbTranslations =
                [ { dbTranslations.[0] with
                      EntryId = entry.Id
                      TranslationText = "protected translation"
                      Source = Wordfolio.Api.DataAccess.Translations.TranslationSource.Manual
                      DisplayOrder = 0 } ]

            Assert.Equal<Translation list>(expectedDbTranslations, dbTranslations)

            let! dbExamples = Seeder.getAllExamplesAsync fixture.WordfolioSeeder
            Assert.Equal<Example list>([], dbExamples)
        }

    [<Fact>]
    member _.``PUT with empty entry text fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(312, "user@example.com", "P@ssw0rd!")

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
                    "original definition"
                    Wordfolio.Api.DataAccess.Definitions.DefinitionSource.Manual
                    0

            let translation =
                Entities.makeTranslation
                    entry
                    "original translation"
                    Wordfolio.Api.DataAccess.Translations.TranslationSource.Manual
                    0

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.addDefinitions [ definition ]
                |> Seeder.addTranslations [ translation ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: UpdateEntryRequest =
                { EntryText = ""
                  Definitions =
                    [ { DefinitionText = "a greeting"
                        Source = DefinitionSource.Manual
                        Examples = [] } ]
                  Translations = [] }

            let url =
                Urls.Entries.entryById(
                    encoder.Encode collection.Id,
                    encoder.Encode vocabulary.Id,
                    encoder.Encode entry.Id
                )

            let! response = client.PutAsJsonAsync(url, request)

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)

            let! dbEntries = Seeder.getAllEntriesAsync fixture.WordfolioSeeder

            let expectedDbEntries =
                [ { dbEntries.[0] with
                      EntryText = "hello"
                      VocabularyId = vocabulary.Id } ]

            Assert.Equal<Entry list>(expectedDbEntries, dbEntries)

            let! dbDefinitions = Seeder.getAllDefinitionsAsync fixture.WordfolioSeeder

            let expectedDbDefinitions =
                [ { dbDefinitions.[0] with
                      EntryId = entry.Id
                      DefinitionText = "original definition"
                      Source = Wordfolio.Api.DataAccess.Definitions.DefinitionSource.Manual
                      DisplayOrder = 0 } ]

            Assert.Equal<Definition list>(expectedDbDefinitions, dbDefinitions)

            let! dbTranslations = Seeder.getAllTranslationsAsync fixture.WordfolioSeeder

            let expectedDbTranslations =
                [ { dbTranslations.[0] with
                      EntryId = entry.Id
                      TranslationText = "original translation"
                      Source = Wordfolio.Api.DataAccess.Translations.TranslationSource.Manual
                      DisplayOrder = 0 } ]

            Assert.Equal<Translation list>(expectedDbTranslations, dbTranslations)

            let! dbExamples = Seeder.getAllExamplesAsync fixture.WordfolioSeeder
            Assert.Equal<Example list>([], dbExamples)
        }

    [<Fact>]
    member _.``PUT with no definitions or translations fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(313, "user@example.com", "P@ssw0rd!")

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
                    "existing definition"
                    Wordfolio.Api.DataAccess.Definitions.DefinitionSource.Manual
                    0

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.addDefinitions [ definition ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: UpdateEntryRequest =
                { EntryText = "hello"
                  Definitions = []
                  Translations = [] }

            let url =
                Urls.Entries.entryById(
                    encoder.Encode collection.Id,
                    encoder.Encode vocabulary.Id,
                    encoder.Encode entry.Id
                )

            let! response = client.PutAsJsonAsync(url, request)

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)

            let! dbEntries = Seeder.getAllEntriesAsync fixture.WordfolioSeeder

            let expectedDbEntries =
                [ { dbEntries.[0] with
                      EntryText = "hello"
                      VocabularyId = vocabulary.Id } ]

            Assert.Equal<Entry list>(expectedDbEntries, dbEntries)

            let! dbDefinitions = Seeder.getAllDefinitionsAsync fixture.WordfolioSeeder

            let expectedDbDefinitions =
                [ { dbDefinitions.[0] with
                      EntryId = entry.Id
                      DefinitionText = "existing definition"
                      Source = Wordfolio.Api.DataAccess.Definitions.DefinitionSource.Manual
                      DisplayOrder = 0 } ]

            Assert.Equal<Definition list>(expectedDbDefinitions, dbDefinitions)

            let! dbTranslations = Seeder.getAllTranslationsAsync fixture.WordfolioSeeder
            Assert.Equal<Translation list>([], dbTranslations)

            let! dbExamples = Seeder.getAllExamplesAsync fixture.WordfolioSeeder
            Assert.Equal<Example list>([], dbExamples)
        }

    [<Fact>]
    member _.``PUT for non-existent entry fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

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
                        Source = DefinitionSource.Manual
                        Examples = [] } ]
                  Translations = [] }

            let url =
                Urls.Entries.entryById(encoder.Encode 1, encoder.Encode 1, encoder.Encode 999999)

            let! response = client.PutAsJsonAsync(url, request)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``PUT with same entry text succeeds``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(316, "user@example.com", "P@ssw0rd!")

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
                Urls.Entries.entryById(
                    encoder.Encode collection.Id,
                    encoder.Encode vocabulary.Id,
                    encoder.Encode entry.Id
                )

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
    member _.``PUT for another user's entry fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser1, wordfolioUser1 = factory.CreateUserAsync(317, "user1@example.com", "P@ssw0rd!")
            let! identityUser2, wordfolioUser2 = factory.CreateUserAsync(318, "user2@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser1 "Test Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary collection "Test Vocabulary" None now now false

            let entry =
                Entities.makeEntry vocabulary "hello" now now

            let ownerDefinition =
                Entities.makeDefinition
                    entry
                    "owner definition"
                    Wordfolio.Api.DataAccess.Definitions.DefinitionSource.Manual
                    0

            let ownerTranslation =
                Entities.makeTranslation
                    entry
                    "owner translation"
                    Wordfolio.Api.DataAccess.Translations.TranslationSource.Manual
                    0

            let requesterCollection =
                Entities.makeCollection wordfolioUser2 "Requester Collection" None now now false

            let requesterVocabulary =
                Entities.makeVocabulary requesterCollection "Requester Vocabulary" None now now false

            let requesterEntry =
                Entities.makeEntry requesterVocabulary "requester entry" now now

            let requesterDefinition =
                Entities.makeDefinition
                    requesterEntry
                    "requester definition"
                    Wordfolio.Api.DataAccess.Definitions.DefinitionSource.Manual
                    0

            let requesterTranslation =
                Entities.makeTranslation
                    requesterEntry
                    "requester translation"
                    Wordfolio.Api.DataAccess.Translations.TranslationSource.Manual
                    0

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser1; wordfolioUser2 ]
                |> Seeder.addCollections [ collection; requesterCollection ]
                |> Seeder.addVocabularies [ vocabulary; requesterVocabulary ]
                |> Seeder.addEntries [ entry; requesterEntry ]
                |> Seeder.addDefinitions [ ownerDefinition; requesterDefinition ]
                |> Seeder.addTranslations [ ownerTranslation; requesterTranslation ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser2)

            let request: UpdateEntryRequest =
                { EntryText = "hello updated"
                  Definitions =
                    [ { DefinitionText = "a greeting"
                        Source = DefinitionSource.Manual
                        Examples = [] } ]
                  Translations = [] }

            let url =
                Urls.Entries.entryById(
                    encoder.Encode collection.Id,
                    encoder.Encode vocabulary.Id,
                    encoder.Encode entry.Id
                )

            let! response = client.PutAsJsonAsync(url, request)

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

            let! dbDefinitions = Seeder.getAllDefinitionsAsync fixture.WordfolioSeeder

            let ownerDefinitionInDatabase =
                dbDefinitions
                |> List.find(fun currentDefinition -> currentDefinition.Id = ownerDefinition.Id)

            let requesterDefinitionInDatabase =
                dbDefinitions
                |> List.find(fun currentDefinition -> currentDefinition.Id = requesterDefinition.Id)

            let expectedDbDefinitions =
                [ { ownerDefinitionInDatabase with
                      Id = ownerDefinition.Id
                      EntryId = entry.Id
                      DefinitionText = "owner definition"
                      Source = Wordfolio.Api.DataAccess.Definitions.DefinitionSource.Manual
                      DisplayOrder = 0 }
                  { requesterDefinitionInDatabase with
                      Id = requesterDefinition.Id
                      EntryId = requesterEntry.Id
                      DefinitionText = "requester definition"
                      Source = Wordfolio.Api.DataAccess.Definitions.DefinitionSource.Manual
                      DisplayOrder = 0 } ]
                |> List.sortBy(fun currentDefinition -> currentDefinition.Id)

            let actualDbDefinitions =
                dbDefinitions
                |> List.sortBy(fun currentDefinition -> currentDefinition.Id)

            Assert.Equal<Definition list>(expectedDbDefinitions, actualDbDefinitions)

            let! dbTranslations = Seeder.getAllTranslationsAsync fixture.WordfolioSeeder

            let ownerTranslationInDatabase =
                dbTranslations
                |> List.find(fun currentTranslation -> currentTranslation.Id = ownerTranslation.Id)

            let requesterTranslationInDatabase =
                dbTranslations
                |> List.find(fun currentTranslation -> currentTranslation.Id = requesterTranslation.Id)

            let expectedDbTranslations =
                [ { ownerTranslationInDatabase with
                      Id = ownerTranslation.Id
                      EntryId = entry.Id
                      TranslationText = "owner translation"
                      Source = Wordfolio.Api.DataAccess.Translations.TranslationSource.Manual
                      DisplayOrder = 0 }
                  { requesterTranslationInDatabase with
                      Id = requesterTranslation.Id
                      EntryId = requesterEntry.Id
                      TranslationText = "requester translation"
                      Source = Wordfolio.Api.DataAccess.Translations.TranslationSource.Manual
                      DisplayOrder = 0 } ]
                |> List.sortBy(fun currentTranslation -> currentTranslation.Id)

            let actualDbTranslations =
                dbTranslations
                |> List.sortBy(fun currentTranslation -> currentTranslation.Id)

            Assert.Equal<Translation list>(expectedDbTranslations, actualDbTranslations)

            let! dbExamples = Seeder.getAllExamplesAsync fixture.WordfolioSeeder
            Assert.Equal<Example list>([], dbExamples)
        }

    [<Fact>]
    member _.``PUT returns 404 when collection does not exist``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(509, "user@example.com", "P@ssw0rd!")

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
                    [ { DefinitionText = "a greeting"
                        Source = DefinitionSource.Manual
                        Examples = [] } ]
                  Translations = [] }

            let! response =
                client.PutAsJsonAsync(
                    Urls.Entries.entryById(encoder.Encode 999999, encoder.Encode vocabulary.Id, encoder.Encode entry.Id),
                    request
                )

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``PUT returns 404 when entry belongs to different vocabulary``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(513, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None now now false

            let vocabularyA =
                Entities.makeVocabulary collection "Vocabulary A" None now now false

            let vocabularyB =
                Entities.makeVocabulary collection "Vocabulary B" None now now false

            let entry =
                Entities.makeEntry vocabularyA "hello" now now

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabularyA; vocabularyB ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: UpdateEntryRequest =
                { EntryText = "hello"
                  Definitions =
                    [ { DefinitionText = "a greeting"
                        Source = DefinitionSource.Manual
                        Examples = [] } ]
                  Translations = [] }

            let! response =
                client.PutAsJsonAsync(
                    Urls.Entries.entryById(
                        encoder.Encode collection.Id,
                        encoder.Encode vocabularyB.Id,
                        encoder.Encode entry.Id
                    ),
                    request
                )

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``PUT returns 404 when vocabulary belongs to different collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(520, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collectionA =
                Entities.makeCollection wordfolioUser "Collection A" None now now false

            let collectionB =
                Entities.makeCollection wordfolioUser "Collection B" None now now false

            let vocabulary =
                Entities.makeVocabulary collectionB "Test Vocabulary" None now now false

            let entry =
                Entities.makeEntry vocabulary "hello" now now

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collectionA; collectionB ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: UpdateEntryRequest =
                { EntryText = "hello"
                  Definitions =
                    [ { DefinitionText = "a greeting"
                        Source = DefinitionSource.Manual
                        Examples = [] } ]
                  Translations = [] }

            let! response =
                client.PutAsJsonAsync(
                    Urls.Entries.entryById(
                        encoder.Encode collectionA.Id,
                        encoder.Encode vocabulary.Id,
                        encoder.Encode entry.Id
                    ),
                    request
                )

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }
