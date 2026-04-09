namespace Wordfolio.Api.Tests.Entries

open System
open System.Net
open System.Net.Http
open System.Net.Http.Json
open System.Text
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Api.Entries.Types
open Wordfolio.Api.Api.Types
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

            let! identityUser, wordfolioUser = factory.CreateUserAsync(500, "user@example.com", "P@ssw0rd!")

            let collection =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeCollection wordfolioUser "Test Collection" None createdAt createdAt false

            let sourceVocabulary =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeVocabulary collection "Source" None createdAt createdAt false

            let targetVocabulary =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeVocabulary collection "Target" None createdAt createdAt false

            let entry =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeEntry sourceVocabulary "hello" createdAt createdAt

            let unaffectedEntry =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeEntry sourceVocabulary "stay" createdAt createdAt

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ sourceVocabulary; targetVocabulary ]
                |> Seeder.addEntries [ entry; unaffectedEntry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: MoveEntryRequest =
                { VocabularyId = Some targetVocabulary.Id }

            let url =
                Urls.Entries.moveEntryById(collection.Id, sourceVocabulary.Id, entry.Id)

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

            let! dbEntries = Seeder.getAllEntriesAsync fixture.WordfolioSeeder

            let unaffectedEntryInDatabase =
                dbEntries
                |> List.find(fun currentEntry -> currentEntry.Id = unaffectedEntry.Id)

            let expectedDbEntries =
                [ { unaffectedEntryInDatabase with
                      Id = unaffectedEntry.Id
                      VocabularyId = sourceVocabulary.Id
                      EntryText = "stay" }
                  { unaffectedEntryInDatabase with
                      Id = entry.Id
                      VocabularyId = targetVocabulary.Id
                      EntryText = "hello"
                      CreatedAt = actual.CreatedAt
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

            let! _, wordfolioUser = factory.CreateUserAsync(526, "user@example.com", "P@ssw0rd!")

            let collection =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeCollection wordfolioUser "Test Collection" None createdAt createdAt false

            let sourceVocabulary =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeVocabulary collection "Source" None createdAt createdAt false

            let targetVocabulary =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeVocabulary collection "Target" None createdAt createdAt false

            let entry =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeEntry sourceVocabulary "hello" createdAt createdAt

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ sourceVocabulary; targetVocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.saveChangesAsync

            use client = factory.CreateClient()

            let request: MoveEntryRequest =
                { VocabularyId = Some targetVocabulary.Id }

            let! response =
                client.PostAsJsonAsync(
                    Urls.Entries.moveEntryById(collection.Id, sourceVocabulary.Id, entry.Id),
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

            let! identityUser, wordfolioUser = factory.CreateUserAsync(501, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: MoveEntryRequest =
                { VocabularyId = Some 999 }

            let! response = client.PostAsJsonAsync(Urls.Entries.moveEntryById(1, 1, 999999), request)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``POST move returns 404 when target vocabulary does not exist``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(502, "user@example.com", "P@ssw0rd!")

            let collection =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeCollection wordfolioUser "Test Collection" None createdAt createdAt false

            let sourceVocabulary =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeVocabulary collection "Source" None createdAt createdAt false

            let entry =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeEntry sourceVocabulary "hello" createdAt createdAt

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ sourceVocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: MoveEntryRequest =
                { VocabularyId = Some 999999 }

            let! response =
                client.PostAsJsonAsync(
                    Urls.Entries.moveEntryById(collection.Id, sourceVocabulary.Id, entry.Id),
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

            let! identityUser1, wordfolioUser1 = factory.CreateUserAsync(503, "user1@example.com", "P@ssw0rd!")
            let! _, wordfolioUser2 = factory.CreateUserAsync(504, "user2@example.com", "P@ssw0rd!")

            let collection1 =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeCollection wordfolioUser1 "Collection 1" None createdAt createdAt false

            let sourceVocabulary =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeVocabulary collection1 "Source" None createdAt createdAt false

            let entry =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeEntry sourceVocabulary "hello" createdAt createdAt

            let collection2 =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeCollection wordfolioUser2 "Collection 2" None createdAt createdAt false

            let foreignTargetVocabulary =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeVocabulary collection2 "Target" None createdAt createdAt false

            let requesterCollection =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeCollection wordfolioUser1 "Requester Collection" None createdAt createdAt false

            let requesterVocabulary =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeVocabulary requesterCollection "Requester Vocabulary" None createdAt createdAt false

            let requesterEntry =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeEntry requesterVocabulary "requester entry" createdAt createdAt

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser1; wordfolioUser2 ]
                |> Seeder.addCollections [ collection1; collection2; requesterCollection ]
                |> Seeder.addVocabularies [ sourceVocabulary; foreignTargetVocabulary; requesterVocabulary ]
                |> Seeder.addEntries [ entry; requesterEntry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser1)

            let request: MoveEntryRequest =
                { VocabularyId = Some foreignTargetVocabulary.Id }

            let! response =
                client.PostAsJsonAsync(
                    Urls.Entries.moveEntryById(collection1.Id, sourceVocabulary.Id, entry.Id),
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

            let! identityUser, wordfolioUser = factory.CreateUserAsync(505, "user@example.com", "P@ssw0rd!")

            let systemCollection =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeCollection wordfolioUser "[System] Unsorted" None createdAt createdAt true

            let defaultVocabulary =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeVocabulary systemCollection "[Default]" None createdAt createdAt true

            let regularCollection =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeCollection wordfolioUser "Regular Collection" None createdAt createdAt false

            let regularVocabulary =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeVocabulary regularCollection "Regular Vocabulary" None createdAt createdAt false

            let entry =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeEntry regularVocabulary "hello" createdAt createdAt

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ systemCollection; regularCollection ]
                |> Seeder.addVocabularies [ defaultVocabulary; regularVocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: MoveEntryRequest =
                { VocabularyId = Some defaultVocabulary.Id }

            let! response =
                client.PostAsJsonAsync(
                    Urls.Entries.moveEntryById(regularCollection.Id, regularVocabulary.Id, entry.Id),
                    request
                )

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

            let! identityUser, wordfolioUser = factory.CreateUserAsync(506, "user@example.com", "P@ssw0rd!")

            let systemCollection =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeCollection wordfolioUser "[System] Unsorted" None createdAt createdAt true

            let defaultVocabulary =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeVocabulary systemCollection "[Default]" None createdAt createdAt true

            let regularCollection =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeCollection wordfolioUser "Regular Collection" None createdAt createdAt false

            let regularVocabulary =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeVocabulary regularCollection "Regular Vocabulary" None createdAt createdAt false

            let entry =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeEntry regularVocabulary "hello" createdAt createdAt

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
                    Urls.Entries.moveEntryById(regularCollection.Id, regularVocabulary.Id, entry.Id),
                    new StringContent("{}", Encoding.UTF8, "application/json")
                )

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

            let! identityUser, wordfolioUser = factory.CreateUserAsync(507, "user@example.com", "P@ssw0rd!")

            let regularCollection =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeCollection wordfolioUser "Regular Collection" None createdAt createdAt false

            let regularVocabulary =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeVocabulary regularCollection "Regular Vocabulary" None createdAt createdAt false

            let entry =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeEntry regularVocabulary "hello" createdAt createdAt

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
                    Urls.Entries.moveEntryById(regularCollection.Id, regularVocabulary.Id, entry.Id),
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

            let expectedResponse: EntryResponse =
                { Id = entry.Id
                  VocabularyId = createdDefaultVocabulary.Id
                  EntryText = "hello"
                  CreatedAt = actual.CreatedAt
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

            let! identityUser, wordfolioUser = factory.CreateUserAsync(508, "user@example.com", "P@ssw0rd!")

            let systemCollection =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeCollection wordfolioUser "[System] Unsorted" None createdAt createdAt true

            let regularCollection =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeCollection wordfolioUser "Regular Collection" None createdAt createdAt false

            let regularVocabulary =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeVocabulary regularCollection "Regular Vocabulary" None createdAt createdAt false

            let entry =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeEntry regularVocabulary "hello" createdAt createdAt

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
                    Urls.Entries.moveEntryById(regularCollection.Id, regularVocabulary.Id, entry.Id),
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
                  UpdatedAt = createdDefaultVocabulary.CreatedAt
                  IsDefault = true }

            Assert.Equal(expectedDefaultVocabulary, createdDefaultVocabulary)

            let expectedResponse: EntryResponse =
                { Id = entry.Id
                  VocabularyId = createdDefaultVocabulary.Id
                  EntryText = "hello"
                  CreatedAt = actual.CreatedAt
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

            let! identityUser, wordfolioUser = factory.CreateUserAsync(511, "user@example.com", "P@ssw0rd!")

            let collection =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeCollection wordfolioUser "Test Collection" None createdAt createdAt false

            let vocabulary =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeVocabulary collection "Test Vocabulary" None createdAt createdAt false

            let entry =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeEntry vocabulary "hello" createdAt createdAt

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: MoveEntryRequest =
                { VocabularyId = Some vocabulary.Id }

            let! response = client.PostAsJsonAsync(Urls.Entries.moveEntryById(999999, vocabulary.Id, entry.Id), request)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``POST move returns 404 when entry belongs to different vocabulary``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(515, "user@example.com", "P@ssw0rd!")

            let collection =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeCollection wordfolioUser "Test Collection" None createdAt createdAt false

            let vocabularyA =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeVocabulary collection "Vocabulary A" None createdAt createdAt false

            let vocabularyB =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeVocabulary collection "Vocabulary B" None createdAt createdAt false

            let entry =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeEntry vocabularyA "hello" createdAt createdAt

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabularyA; vocabularyB ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: MoveEntryRequest =
                { VocabularyId = Some vocabularyA.Id }

            let! response =
                client.PostAsJsonAsync(Urls.Entries.moveEntryById(collection.Id, vocabularyB.Id, entry.Id), request)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``POST move returns 404 when vocabulary belongs to different collection``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(522, "user@example.com", "P@ssw0rd!")

            let collectionA =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeCollection wordfolioUser "Collection A" None createdAt createdAt false

            let collectionB =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeCollection wordfolioUser "Collection B" None createdAt createdAt false

            let vocabulary =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeVocabulary collectionB "Test Vocabulary" None createdAt createdAt false

            let entry =
                let createdAt = DateTimeOffset.UtcNow
                Entities.makeEntry vocabulary "hello" createdAt createdAt

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collectionA; collectionB ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: MoveEntryRequest =
                { VocabularyId = Some vocabulary.Id }

            let! response =
                client.PostAsJsonAsync(Urls.Entries.moveEntryById(collectionA.Id, vocabulary.Id, entry.Id), request)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }
