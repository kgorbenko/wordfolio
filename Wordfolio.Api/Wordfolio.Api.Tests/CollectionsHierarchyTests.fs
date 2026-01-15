namespace Wordfolio.Api.Tests

open System
open System.Net
open System.Net.Http.Json
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Handlers.CollectionsHierarchy
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

module Urls = Wordfolio.Api.Urls

type CollectionsHierarchyTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``GET returns collections with vocabularies and entry counts``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(300, "user@example.com", "P@ssw0rd!")

            let collection1 =
                Entities.makeCollection
                    wordfolioUser
                    "Collection 1"
                    (Some "Description 1")
                    DateTimeOffset.UtcNow
                    None
                    false

            let collection2 =
                Entities.makeCollection wordfolioUser "Collection 2" None DateTimeOffset.UtcNow None false

            let vocabulary1 =
                Entities.makeVocabulary
                    collection1
                    "Vocabulary 1"
                    (Some "Vocab description")
                    DateTimeOffset.UtcNow
                    None
                    false

            let vocabulary2 =
                Entities.makeVocabulary collection1 "Vocabulary 2" None DateTimeOffset.UtcNow None false

            let entry1 =
                Entities.makeEntry vocabulary1 "entry1" DateTimeOffset.UtcNow None

            let entry2 =
                Entities.makeEntry vocabulary1 "entry2" DateTimeOffset.UtcNow None

            let entry3 =
                Entities.makeEntry vocabulary2 "entry3" DateTimeOffset.UtcNow None

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection1; collection2 ]
                |> Seeder.addVocabularies [ vocabulary1; vocabulary2 ]
                |> Seeder.addEntries [ entry1; entry2; entry3 ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.GetAsync(Urls.CollectionsHierarchy.Path)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<CollectionsHierarchyResponse>()

            let actualCollection1 =
                actual.Collections
                |> List.find(fun c -> c.Id = collection1.Id)

            let actualCollection2 =
                actual.Collections
                |> List.find(fun c -> c.Id = collection2.Id)

            let expected: CollectionsHierarchyResponse =
                { Collections =
                    [ { Id = collection1.Id
                        Name = "Collection 1"
                        Description = Some "Description 1"
                        CreatedAt = actualCollection1.CreatedAt
                        UpdatedAt = None
                        Vocabularies =
                          [ { Id = vocabulary1.Id
                              Name = "Vocabulary 1"
                              Description = Some "Vocab description"
                              CreatedAt = actualCollection1.Vocabularies[0].CreatedAt
                              UpdatedAt = None
                              EntryCount = 2 }
                            { Id = vocabulary2.Id
                              Name = "Vocabulary 2"
                              Description = None
                              CreatedAt = actualCollection1.Vocabularies[1].CreatedAt
                              UpdatedAt = None
                              EntryCount = 1 } ] }
                      { Id = collection2.Id
                        Name = "Collection 2"
                        Description = None
                        CreatedAt = actualCollection2.CreatedAt
                        UpdatedAt = None
                        Vocabularies = [] } ]
                  DefaultVocabulary = None }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``GET excludes system collections``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(301, "user@example.com", "P@ssw0rd!")

            let regularCollection =
                Entities.makeCollection wordfolioUser "Regular Collection" None DateTimeOffset.UtcNow None false

            let systemCollection =
                Entities.makeCollection wordfolioUser "System Collection" None DateTimeOffset.UtcNow None true

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ regularCollection; systemCollection ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.GetAsync(Urls.CollectionsHierarchy.Path)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<CollectionsHierarchyResponse>()

            let actualCollection =
                Assert.Single(actual.Collections)

            let expected: CollectionsHierarchyResponse =
                { Collections =
                    [ { Id = regularCollection.Id
                        Name = "Regular Collection"
                        Description = None
                        CreatedAt = actualCollection.CreatedAt
                        UpdatedAt = None
                        Vocabularies = [] } ]
                  DefaultVocabulary = None }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``GET excludes default vocabularies``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(302, "user@example.com", "P@ssw0rd!")

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None DateTimeOffset.UtcNow None false

            let regularVocabulary =
                Entities.makeVocabulary collection "Regular Vocabulary" None DateTimeOffset.UtcNow None false

            let defaultVocabulary =
                Entities.makeVocabulary collection "Default Vocabulary" None DateTimeOffset.UtcNow None true

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ regularVocabulary; defaultVocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.GetAsync(Urls.CollectionsHierarchy.Path)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<CollectionsHierarchyResponse>()

            let actualCollection =
                Assert.Single(actual.Collections)

            let expected: CollectionsHierarchyResponse =
                { Collections =
                    [ { Id = collection.Id
                        Name = "Test Collection"
                        Description = None
                        CreatedAt = actualCollection.CreatedAt
                        UpdatedAt = None
                        Vocabularies =
                          [ { Id = regularVocabulary.Id
                              Name = "Regular Vocabulary"
                              Description = None
                              CreatedAt = actualCollection.Vocabularies[0].CreatedAt
                              UpdatedAt = None
                              EntryCount = 0 } ] } ]
                  DefaultVocabulary = None }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``GET returns empty list when no collections``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(303, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.GetAsync(Urls.CollectionsHierarchy.Path)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<CollectionsHierarchyResponse>()

            let expected: CollectionsHierarchyResponse =
                { Collections = []
                  DefaultVocabulary = None }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``GET returns default vocabulary when it has entries``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(304, "user@example.com", "P@ssw0rd!")

            let systemCollection =
                Entities.makeCollection wordfolioUser "Unsorted" None DateTimeOffset.UtcNow None true

            let defaultVocabulary =
                Entities.makeVocabulary
                    systemCollection
                    "My Words"
                    (Some "Default vocab")
                    DateTimeOffset.UtcNow
                    None
                    true

            let entry1 =
                Entities.makeEntry defaultVocabulary "word1" DateTimeOffset.UtcNow None

            let entry2 =
                Entities.makeEntry defaultVocabulary "word2" DateTimeOffset.UtcNow None

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ systemCollection ]
                |> Seeder.addVocabularies [ defaultVocabulary ]
                |> Seeder.addEntries [ entry1; entry2 ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.GetAsync(Urls.CollectionsHierarchy.Path)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<CollectionsHierarchyResponse>()

            let expected: CollectionsHierarchyResponse =
                { Collections = []
                  DefaultVocabulary =
                    Some
                        { Id = defaultVocabulary.Id
                          Name = "My Words"
                          Description = Some "Default vocab"
                          CreatedAt = actual.DefaultVocabulary.Value.CreatedAt
                          UpdatedAt = None
                          EntryCount = 2 } }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``GET does not return default vocabulary when it has no entries``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(305, "user@example.com", "P@ssw0rd!")

            let systemCollection =
                Entities.makeCollection wordfolioUser "Unsorted" None DateTimeOffset.UtcNow None true

            let defaultVocabulary =
                Entities.makeVocabulary systemCollection "My Words" None DateTimeOffset.UtcNow None true

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ systemCollection ]
                |> Seeder.addVocabularies [ defaultVocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.GetAsync(Urls.CollectionsHierarchy.Path)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<CollectionsHierarchyResponse>()

            let expected: CollectionsHierarchyResponse =
                { Collections = []
                  DefaultVocabulary = None }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``GET without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let! response = client.GetAsync(Urls.CollectionsHierarchy.Path)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }
