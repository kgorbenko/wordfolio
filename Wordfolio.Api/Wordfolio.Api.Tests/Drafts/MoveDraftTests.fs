namespace Wordfolio.Api.Tests.Drafts

open System
open System.Net
open System.Net.Http.Json
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Api
open Wordfolio.Api.Tests
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

module Urls = Wordfolio.Api.Urls

type MoveDraftTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

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

            let unaffectedEntry =
                Entities.makeEntry sourceVocabulary "stay" DateTimeOffset.UtcNow None

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ sourceVocabulary; targetVocabulary ]
                |> Seeder.addEntries [ entry; unaffectedEntry ]
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
    member _.``POST move draft without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! _, wordfolioUser = factory.CreateUserAsync(717, "user@example.com", "P@ssw0rd!")

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

            use client = factory.CreateClient()

            let request: MoveEntryRequest =
                { VocabularyId = targetVocabulary.Id }

            let! response = client.PostAsJsonAsync(Urls.Drafts.moveDraftById entry.Id, request)

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

            let requesterCollection =
                Entities.makeCollection wordfolioUser1 "Requester Collection" None DateTimeOffset.UtcNow None false

            let requesterVocabulary =
                Entities.makeVocabulary requesterCollection "Requester Vocabulary" None DateTimeOffset.UtcNow None false

            let requesterEntry =
                Entities.makeEntry requesterVocabulary "requester entry" DateTimeOffset.UtcNow None

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser1; wordfolioUser2 ]
                |> Seeder.addCollections [ collection1; collection2; requesterCollection ]
                |> Seeder.addVocabularies [ sourceVocabulary; foreignTargetVocabulary; requesterVocabulary ]
                |> Seeder.addEntries [ entry; requesterEntry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser1)

            let request: MoveEntryRequest =
                { VocabularyId = foreignTargetVocabulary.Id }

            let! response = client.PostAsJsonAsync(Urls.Drafts.moveDraftById entry.Id, request)

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
