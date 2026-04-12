namespace Wordfolio.Api.Tests.Entries

open System
open System.Net
open System.Net.Http
open System.Net.Http.Json
open System.Text
open System.Threading.Tasks
open Microsoft.Extensions.DependencyInjection

open Xunit

open Wordfolio.Api.Api.Entries.Types
open Wordfolio.Api.Api.Types
open Wordfolio.Api.Infrastructure.ResourceIdEncoder
open Wordfolio.Api.Tests
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

module Urls = Wordfolio.Api.Urls

type MoveEntryTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``POST move updates entry vocabulary for owned source and target``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(500, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None now now false

            let sourceVocabulary =
                Entities.makeVocabulary collection "Source" None now now false

            let targetVocabulary =
                Entities.makeVocabulary collection "Target" None now now false

            let entry =
                Entities.makeEntry sourceVocabulary "hello" now now

            let unaffectedEntry =
                Entities.makeEntry sourceVocabulary "stay" now now

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ sourceVocabulary; targetVocabulary ]
                |> Seeder.addEntries [ entry; unaffectedEntry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: MoveEntryRequest =
                { VocabularyId = Some(encoder.Encode targetVocabulary.Id) }

            let url =
                Urls.Entries.moveEntryById(
                    encoder.Encode collection.Id,
                    encoder.Encode sourceVocabulary.Id,
                    encoder.Encode entry.Id
                )

            let! response = client.PostAsJsonAsync(url, request)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let! actual = response.Content.ReadFromJsonAsync<EntryResponse>()

            Assert.True(actual.UpdatedAt >= entry.CreatedAt)

            let expected: EntryResponse =
                { Id = encoder.Encode entry.Id
                  VocabularyId = encoder.Encode targetVocabulary.Id
                  EntryText = "hello"
                  CreatedAt = entry.CreatedAt
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
                      CreatedAt = entry.CreatedAt
                      UpdatedAt = actual.UpdatedAt }

            Assert.Equal(expectedMovedEntry, movedEntry)

            let! dbEntries = Seeder.getAllEntriesAsync fixture.WordfolioSeeder

            let expectedDbEntries =
                [ { Id = unaffectedEntry.Id
                    VocabularyId = sourceVocabulary.Id
                    EntryText = "stay"
                    CreatedAt = unaffectedEntry.CreatedAt
                    UpdatedAt = unaffectedEntry.UpdatedAt }
                  { Id = entry.Id
                    VocabularyId = targetVocabulary.Id
                    EntryText = "hello"
                    CreatedAt = entry.CreatedAt
                    UpdatedAt = actual.UpdatedAt } ]
                |> List.sortBy(fun currentEntry -> currentEntry.Id)

            let actualDbEntries =
                dbEntries
                |> List.sortBy(fun currentEntry -> currentEntry.Id)

            Assert.Equal<Entry list>(expectedDbEntries, actualDbEntries)
        }

    [<Fact>]
    member _.``POST move without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! _, wordfolioUser = factory.CreateUserAsync(526, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None now now false

            let sourceVocabulary =
                Entities.makeVocabulary collection "Source" None now now false

            let targetVocabulary =
                Entities.makeVocabulary collection "Target" None now now false

            let entry =
                Entities.makeEntry sourceVocabulary "hello" now now

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ sourceVocabulary; targetVocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.saveChangesAsync

            use client = factory.CreateClient()

            let request: MoveEntryRequest =
                { VocabularyId = Some(encoder.Encode targetVocabulary.Id) }

            let! response =
                client.PostAsJsonAsync(
                    Urls.Entries.moveEntryById(
                        encoder.Encode collection.Id,
                        encoder.Encode sourceVocabulary.Id,
                        encoder.Encode entry.Id
                    ),
                    request
                )

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)

            let! dbEntries = Seeder.getAllEntriesAsync fixture.WordfolioSeeder

            let expectedDbEntries =
                [ { dbEntries.[0] with
                      Id = entry.Id
                      VocabularyId = sourceVocabulary.Id
                      EntryText = "hello" } ]

            Assert.Equal<Entry list>(expectedDbEntries, dbEntries)
        }

    [<Fact>]
    member _.``POST move for non-existent entry returns 404``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(501, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: MoveEntryRequest =
                { VocabularyId = Some(encoder.Encode 999) }

            let! response =
                client.PostAsJsonAsync(
                    Urls.Entries.moveEntryById(encoder.Encode 1, encoder.Encode 1, encoder.Encode 999999),
                    request
                )

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``POST move returns 404 when target vocabulary does not exist``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(502, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None now now false

            let sourceVocabulary =
                Entities.makeVocabulary collection "Source" None now now false

            let entry =
                Entities.makeEntry sourceVocabulary "hello" now now

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ sourceVocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: MoveEntryRequest =
                { VocabularyId = Some(encoder.Encode 999999) }

            let! response =
                client.PostAsJsonAsync(
                    Urls.Entries.moveEntryById(
                        encoder.Encode collection.Id,
                        encoder.Encode sourceVocabulary.Id,
                        encoder.Encode entry.Id
                    ),
                    request
                )

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``POST move returns 404 when target vocabulary belongs to another user``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser1, wordfolioUser1 = factory.CreateUserAsync(503, "user1@example.com", "P@ssw0rd!")
            let! _, wordfolioUser2 = factory.CreateUserAsync(504, "user2@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection1 =
                Entities.makeCollection wordfolioUser1 "Collection 1" None now now false

            let sourceVocabulary =
                Entities.makeVocabulary collection1 "Source" None now now false

            let entry =
                Entities.makeEntry sourceVocabulary "hello" now now

            let collection2 =
                Entities.makeCollection wordfolioUser2 "Collection 2" None now now false

            let foreignTargetVocabulary =
                Entities.makeVocabulary collection2 "Target" None now now false

            let requesterCollection =
                Entities.makeCollection wordfolioUser1 "Requester Collection" None now now false

            let requesterVocabulary =
                Entities.makeVocabulary requesterCollection "Requester Vocabulary" None now now false

            let requesterEntry =
                Entities.makeEntry requesterVocabulary "requester entry" now now

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser1; wordfolioUser2 ]
                |> Seeder.addCollections [ collection1; collection2; requesterCollection ]
                |> Seeder.addVocabularies [ sourceVocabulary; foreignTargetVocabulary; requesterVocabulary ]
                |> Seeder.addEntries [ entry; requesterEntry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser1)

            let request: MoveEntryRequest =
                { VocabularyId = Some(encoder.Encode foreignTargetVocabulary.Id) }

            let! response =
                client.PostAsJsonAsync(
                    Urls.Entries.moveEntryById(
                        encoder.Encode collection1.Id,
                        encoder.Encode sourceVocabulary.Id,
                        encoder.Encode entry.Id
                    ),
                    request
                )

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
                      VocabularyId = sourceVocabulary.Id
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
    member _.``POST move to default vocabulary succeeds``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(505, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let systemCollection =
                Entities.makeCollection wordfolioUser "[System] Unsorted" None now now true

            let defaultVocabulary =
                Entities.makeVocabulary systemCollection "[Default]" None now now true

            let regularCollection =
                Entities.makeCollection wordfolioUser "Regular Collection" None now now false

            let regularVocabulary =
                Entities.makeVocabulary regularCollection "Regular Vocabulary" None now now false

            let entry =
                Entities.makeEntry regularVocabulary "hello" now now

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ systemCollection; regularCollection ]
                |> Seeder.addVocabularies [ defaultVocabulary; regularVocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: MoveEntryRequest =
                { VocabularyId = Some(encoder.Encode defaultVocabulary.Id) }

            let! response =
                client.PostAsJsonAsync(
                    Urls.Entries.moveEntryById(
                        encoder.Encode regularCollection.Id,
                        encoder.Encode regularVocabulary.Id,
                        encoder.Encode entry.Id
                    ),
                    request
                )

            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let! actual = response.Content.ReadFromJsonAsync<EntryResponse>()

            Assert.True(actual.UpdatedAt >= entry.CreatedAt)

            let expected: EntryResponse =
                { Id = encoder.Encode entry.Id
                  VocabularyId = encoder.Encode defaultVocabulary.Id
                  EntryText = "hello"
                  CreatedAt = entry.CreatedAt
                  UpdatedAt = actual.UpdatedAt
                  Definitions = []
                  Translations = [] }

            Assert.Equal(expected, actual)

            let! dbCollections = Seeder.getAllCollectionsAsync fixture.WordfolioSeeder
            let! dbVocabularies = Seeder.getAllVocabulariesAsync fixture.WordfolioSeeder

            let systemCollectionInDatabase =
                dbCollections
                |> List.find(fun collection -> collection.Id = systemCollection.Id)

            let regularCollectionInDatabase =
                dbCollections
                |> List.find(fun collection -> collection.Id = regularCollection.Id)

            let defaultVocabularyInDatabase =
                dbVocabularies
                |> List.find(fun vocabulary -> vocabulary.Id = defaultVocabulary.Id)

            let regularVocabularyInDatabase =
                dbVocabularies
                |> List.find(fun vocabulary -> vocabulary.Id = regularVocabulary.Id)

            let expectedDbCollections =
                [ { Id = systemCollection.Id
                    UserId = wordfolioUser.Id
                    Name = "[System] Unsorted"
                    Description = None
                    CreatedAt = systemCollectionInDatabase.CreatedAt
                    UpdatedAt = systemCollectionInDatabase.UpdatedAt
                    IsSystem = true }
                  { Id = regularCollection.Id
                    UserId = wordfolioUser.Id
                    Name = "Regular Collection"
                    Description = None
                    CreatedAt = regularCollectionInDatabase.CreatedAt
                    UpdatedAt = regularCollectionInDatabase.UpdatedAt
                    IsSystem = false } ]
                |> List.sortBy(fun collection -> collection.Id)

            Assert.Equal<Wordfolio.Collection list>(
                expectedDbCollections,
                dbCollections
                |> List.sortBy(fun collection -> collection.Id)
            )

            let expectedDbVocabularies =
                [ { Id = defaultVocabulary.Id
                    CollectionId = systemCollection.Id
                    Name = "[Default]"
                    Description = None
                    CreatedAt = defaultVocabularyInDatabase.CreatedAt
                    UpdatedAt = defaultVocabularyInDatabase.UpdatedAt
                    IsDefault = true }
                  { Id = regularVocabulary.Id
                    CollectionId = regularCollection.Id
                    Name = "Regular Vocabulary"
                    Description = None
                    CreatedAt = regularVocabularyInDatabase.CreatedAt
                    UpdatedAt = regularVocabularyInDatabase.UpdatedAt
                    IsDefault = false } ]
                |> List.sortBy(fun vocabulary -> vocabulary.Id)

            Assert.Equal<Wordfolio.Vocabulary list>(
                expectedDbVocabularies,
                dbVocabularies
                |> List.sortBy(fun vocabulary -> vocabulary.Id)
            )
        }

    [<Fact>]
    member _.``POST move to default vocabulary succeeds when request omits vocabulary id and default vocabulary exists``
        ()
        : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(506, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let systemCollection =
                Entities.makeCollection wordfolioUser "[System] Unsorted" None now now true

            let defaultVocabulary =
                Entities.makeVocabulary systemCollection "[Default]" None now now true

            let regularCollection =
                Entities.makeCollection wordfolioUser "Regular Collection" None now now false

            let regularVocabulary =
                Entities.makeVocabulary regularCollection "Regular Vocabulary" None now now false

            let entry =
                Entities.makeEntry regularVocabulary "hello" now now

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ systemCollection; regularCollection ]
                |> Seeder.addVocabularies [ defaultVocabulary; regularVocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response =
                client.PostAsync(
                    Urls.Entries.moveEntryById(
                        encoder.Encode regularCollection.Id,
                        encoder.Encode regularVocabulary.Id,
                        encoder.Encode entry.Id
                    ),
                    new StringContent("{}", Encoding.UTF8, "application/json")
                )

            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let! actual = response.Content.ReadFromJsonAsync<EntryResponse>()

            Assert.True(actual.UpdatedAt >= entry.CreatedAt)

            let expected: EntryResponse =
                { Id = encoder.Encode entry.Id
                  VocabularyId = encoder.Encode defaultVocabulary.Id
                  EntryText = "hello"
                  CreatedAt = entry.CreatedAt
                  UpdatedAt = actual.UpdatedAt
                  Definitions = []
                  Translations = [] }

            Assert.Equal(expected, actual)

            let! dbCollections = Seeder.getAllCollectionsAsync fixture.WordfolioSeeder
            let! dbVocabularies = Seeder.getAllVocabulariesAsync fixture.WordfolioSeeder
            let! dbEntries = Seeder.getAllEntriesAsync fixture.WordfolioSeeder

            let systemCollectionInDatabase =
                dbCollections
                |> List.find(fun collection -> collection.Id = systemCollection.Id)

            let regularCollectionInDatabase =
                dbCollections
                |> List.find(fun collection -> collection.Id = regularCollection.Id)

            let defaultVocabularyInDatabase =
                dbVocabularies
                |> List.find(fun vocabulary -> vocabulary.Id = defaultVocabulary.Id)

            let regularVocabularyInDatabase =
                dbVocabularies
                |> List.find(fun vocabulary -> vocabulary.Id = regularVocabulary.Id)

            let expectedDbCollections =
                [ { Id = systemCollection.Id
                    UserId = wordfolioUser.Id
                    Name = "[System] Unsorted"
                    Description = None
                    CreatedAt = systemCollectionInDatabase.CreatedAt
                    UpdatedAt = systemCollectionInDatabase.UpdatedAt
                    IsSystem = true }
                  { Id = regularCollection.Id
                    UserId = wordfolioUser.Id
                    Name = "Regular Collection"
                    Description = None
                    CreatedAt = regularCollectionInDatabase.CreatedAt
                    UpdatedAt = regularCollectionInDatabase.UpdatedAt
                    IsSystem = false } ]
                |> List.sortBy(fun collection -> collection.Id)

            Assert.Equal<Wordfolio.Collection list>(
                expectedDbCollections,
                dbCollections
                |> List.sortBy(fun collection -> collection.Id)
            )

            let expectedDbVocabularies =
                [ { Id = defaultVocabulary.Id
                    CollectionId = systemCollection.Id
                    Name = "[Default]"
                    Description = None
                    CreatedAt = defaultVocabularyInDatabase.CreatedAt
                    UpdatedAt = defaultVocabularyInDatabase.UpdatedAt
                    IsDefault = true }
                  { Id = regularVocabulary.Id
                    CollectionId = regularCollection.Id
                    Name = "Regular Vocabulary"
                    Description = None
                    CreatedAt = regularVocabularyInDatabase.CreatedAt
                    UpdatedAt = regularVocabularyInDatabase.UpdatedAt
                    IsDefault = false } ]
                |> List.sortBy(fun vocabulary -> vocabulary.Id)

            Assert.Equal<Wordfolio.Vocabulary list>(
                expectedDbVocabularies,
                dbVocabularies
                |> List.sortBy(fun vocabulary -> vocabulary.Id)
            )

            let entryInDatabase =
                dbEntries
                |> List.find(fun currentEntry -> currentEntry.Id = entry.Id)

            let expectedDbEntries =
                [ { entryInDatabase with
                      Id = entry.Id
                      VocabularyId = defaultVocabulary.Id
                      EntryText = "hello" } ]

            Assert.Equal<Entry list>(expectedDbEntries, dbEntries)
        }

    [<Fact>]
    member _.``POST move to default vocabulary creates default collection and vocabulary when neither exists``
        ()
        : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(507, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let regularCollection =
                Entities.makeCollection wordfolioUser "Regular Collection" None now now false

            let regularVocabulary =
                Entities.makeVocabulary regularCollection "Regular Vocabulary" None now now false

            let entry =
                Entities.makeEntry regularVocabulary "hello" now now

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ regularCollection ]
                |> Seeder.addVocabularies [ regularVocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response =
                client.PostAsync(
                    Urls.Entries.moveEntryById(
                        encoder.Encode regularCollection.Id,
                        encoder.Encode regularVocabulary.Id,
                        encoder.Encode entry.Id
                    ),
                    new StringContent("{}", Encoding.UTF8, "application/json")
                )

            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let! actual = response.Content.ReadFromJsonAsync<EntryResponse>()
            let! dbCollections = Seeder.getAllCollectionsAsync fixture.WordfolioSeeder
            let! dbVocabularies = Seeder.getAllVocabulariesAsync fixture.WordfolioSeeder
            let! dbEntries = Seeder.getAllEntriesAsync fixture.WordfolioSeeder

            Assert.Equal(2, dbCollections.Length)
            Assert.Equal(2, dbVocabularies.Length)

            let createdSystemCollection =
                dbCollections
                |> List.find(fun collection -> collection.Id <> regularCollection.Id)

            let createdDefaultVocabulary =
                dbVocabularies
                |> List.find(fun vocabulary -> vocabulary.Id <> regularVocabulary.Id)

            let expectedSystemCollection: Wordfolio.Collection =
                { createdSystemCollection with
                    UserId = wordfolioUser.Id
                    Name = "[System] Unsorted"
                    Description = None
                    IsSystem = true }

            let expectedDefaultVocabulary: Wordfolio.Vocabulary =
                { createdDefaultVocabulary with
                    CollectionId = createdSystemCollection.Id
                    Name = "[Default]"
                    Description = None
                    IsDefault = true }

            Assert.Equal(expectedSystemCollection, createdSystemCollection)
            Assert.Equal(expectedDefaultVocabulary, createdDefaultVocabulary)

            Assert.True(actual.UpdatedAt >= entry.CreatedAt)

            let expectedResponse: EntryResponse =
                { Id = encoder.Encode entry.Id
                  VocabularyId = encoder.Encode createdDefaultVocabulary.Id
                  EntryText = "hello"
                  CreatedAt = entry.CreatedAt
                  UpdatedAt = actual.UpdatedAt
                  Definitions = []
                  Translations = [] }

            Assert.Equal(expectedResponse, actual)

            let entryInDatabase =
                dbEntries
                |> List.find(fun currentEntry -> currentEntry.Id = entry.Id)

            let expectedDbEntries =
                [ { entryInDatabase with
                      Id = entry.Id
                      VocabularyId = createdDefaultVocabulary.Id
                      EntryText = "hello" } ]

            Assert.Equal<Entry list>(expectedDbEntries, dbEntries)
        }

    [<Fact>]
    member _.``POST move to default vocabulary creates default vocabulary in existing system collection when vocabulary is missing``
        ()
        : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(508, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let systemCollection =
                Entities.makeCollection wordfolioUser "[System] Unsorted" None now now true

            let regularCollection =
                Entities.makeCollection wordfolioUser "Regular Collection" None now now false

            let regularVocabulary =
                Entities.makeVocabulary regularCollection "Regular Vocabulary" None now now false

            let entry =
                Entities.makeEntry regularVocabulary "hello" now now

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ systemCollection; regularCollection ]
                |> Seeder.addVocabularies [ regularVocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response =
                client.PostAsync(
                    Urls.Entries.moveEntryById(
                        encoder.Encode regularCollection.Id,
                        encoder.Encode regularVocabulary.Id,
                        encoder.Encode entry.Id
                    ),
                    new StringContent("{}", Encoding.UTF8, "application/json")
                )

            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let! actual = response.Content.ReadFromJsonAsync<EntryResponse>()
            let! dbCollections = Seeder.getAllCollectionsAsync fixture.WordfolioSeeder
            let! dbVocabularies = Seeder.getAllVocabulariesAsync fixture.WordfolioSeeder
            let! dbEntries = Seeder.getAllEntriesAsync fixture.WordfolioSeeder

            let systemCollectionInDatabase =
                dbCollections
                |> List.find(fun collection -> collection.Id = systemCollection.Id)

            let regularCollectionInDatabase =
                dbCollections
                |> List.find(fun collection -> collection.Id = regularCollection.Id)

            let expectedDbCollections =
                [ { Id = systemCollection.Id
                    UserId = wordfolioUser.Id
                    Name = "[System] Unsorted"
                    Description = None
                    CreatedAt = systemCollectionInDatabase.CreatedAt
                    UpdatedAt = systemCollectionInDatabase.UpdatedAt
                    IsSystem = true }
                  { Id = regularCollection.Id
                    UserId = wordfolioUser.Id
                    Name = "Regular Collection"
                    Description = None
                    CreatedAt = regularCollectionInDatabase.CreatedAt
                    UpdatedAt = regularCollectionInDatabase.UpdatedAt
                    IsSystem = false } ]
                |> List.sortBy(fun collection -> collection.Id)

            Assert.Equal<Wordfolio.Collection list>(
                expectedDbCollections,
                dbCollections
                |> List.sortBy(fun collection -> collection.Id)
            )

            Assert.Equal(2, dbVocabularies.Length)

            let createdDefaultVocabulary =
                dbVocabularies
                |> List.find(fun vocabulary -> vocabulary.Id <> regularVocabulary.Id)

            let expectedDefaultVocabulary: Wordfolio.Vocabulary =
                { Id = createdDefaultVocabulary.Id
                  CollectionId = systemCollection.Id
                  Name = "[Default]"
                  Description = None
                  CreatedAt = createdDefaultVocabulary.CreatedAt
                  UpdatedAt = createdDefaultVocabulary.UpdatedAt
                  IsDefault = true }

            Assert.Equal(expectedDefaultVocabulary, createdDefaultVocabulary)

            Assert.True(actual.UpdatedAt >= entry.CreatedAt)

            let expectedResponse: EntryResponse =
                { Id = encoder.Encode entry.Id
                  VocabularyId = encoder.Encode createdDefaultVocabulary.Id
                  EntryText = "hello"
                  CreatedAt = entry.CreatedAt
                  UpdatedAt = actual.UpdatedAt
                  Definitions = []
                  Translations = [] }

            Assert.Equal(expectedResponse, actual)

            let entryInDatabase =
                dbEntries
                |> List.find(fun currentEntry -> currentEntry.Id = entry.Id)

            let expectedDbEntries =
                [ { entryInDatabase with
                      Id = entry.Id
                      VocabularyId = createdDefaultVocabulary.Id
                      EntryText = "hello" } ]

            Assert.Equal<Entry list>(expectedDbEntries, dbEntries)
        }

    [<Fact>]
    member _.``POST move returns 404 when collection does not exist``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(511, "user@example.com", "P@ssw0rd!")

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

            let request: MoveEntryRequest =
                { VocabularyId = Some(encoder.Encode vocabulary.Id) }

            let! response =
                client.PostAsJsonAsync(
                    Urls.Entries.moveEntryById(
                        encoder.Encode 999999,
                        encoder.Encode vocabulary.Id,
                        encoder.Encode entry.Id
                    ),
                    request
                )

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``POST move returns 404 when entry belongs to different vocabulary``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(515, "user@example.com", "P@ssw0rd!")

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

            let request: MoveEntryRequest =
                { VocabularyId = Some(encoder.Encode vocabularyA.Id) }

            let! response =
                client.PostAsJsonAsync(
                    Urls.Entries.moveEntryById(
                        encoder.Encode collection.Id,
                        encoder.Encode vocabularyB.Id,
                        encoder.Encode entry.Id
                    ),
                    request
                )

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``POST move returns 404 when vocabulary belongs to different collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(522, "user@example.com", "P@ssw0rd!")

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

            let request: MoveEntryRequest =
                { VocabularyId = Some(encoder.Encode vocabulary.Id) }

            let! response =
                client.PostAsJsonAsync(
                    Urls.Entries.moveEntryById(
                        encoder.Encode collectionA.Id,
                        encoder.Encode vocabulary.Id,
                        encoder.Encode entry.Id
                    ),
                    request
                )

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }
