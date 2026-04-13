namespace Wordfolio.Api.Tests.CollectionsHierarchy

open System
open System.Net
open System.Net.Http.Json
open System.Threading.Tasks
open Microsoft.Extensions.DependencyInjection

open Xunit

open Wordfolio.Api.Api.CollectionsHierarchy.Types
open Wordfolio.Api.Infrastructure.ResourceIdEncoder
open Wordfolio.Api.Tests
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

module Urls = Wordfolio.Api.Urls

type GetCollectionsHierarchyTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``GET returns collections with vocabularies and entry counts``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(300, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection1 =
                Entities.makeCollection wordfolioUser "Collection 1" (Some "Description 1") now now false

            let collection2 =
                Entities.makeCollection wordfolioUser "Collection 2" None now now false

            let vocabulary1 =
                Entities.makeVocabulary collection1 "Vocabulary 1" (Some "Vocab description") now now false

            let vocabulary2 =
                Entities.makeVocabulary collection1 "Vocabulary 2" None now now false

            let entry1 =
                Entities.makeEntry vocabulary1 "entry1" now now

            let entry2 =
                Entities.makeEntry vocabulary1 "entry2" now now

            let entry3 =
                Entities.makeEntry vocabulary2 "entry3" now now

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

            let! actual = response.Content.ReadFromJsonAsync<CollectionsHierarchyResultResponse>()

            let actualCollection1 =
                actual.Collections
                |> List.find(fun c -> c.Id = encoder.Encode collection1.Id)

            let actualCollection2 =
                actual.Collections
                |> List.find(fun c -> c.Id = encoder.Encode collection2.Id)

            let expected: CollectionsHierarchyResultResponse =
                { Collections =
                    [ { Id = encoder.Encode collection1.Id
                        Name = "Collection 1"
                        Description = Some "Description 1"
                        CreatedAt = actualCollection1.CreatedAt
                        UpdatedAt = actualCollection1.CreatedAt
                        Vocabularies =
                          [ { Id = encoder.Encode vocabulary1.Id
                              Name = "Vocabulary 1"
                              Description = Some "Vocab description"
                              CreatedAt = actualCollection1.Vocabularies[0].CreatedAt
                              UpdatedAt = actualCollection1.Vocabularies[0].CreatedAt
                              EntryCount = 2 }
                            { Id = encoder.Encode vocabulary2.Id
                              Name = "Vocabulary 2"
                              Description = None
                              CreatedAt = actualCollection1.Vocabularies[1].CreatedAt
                              UpdatedAt = actualCollection1.Vocabularies[1].CreatedAt
                              EntryCount = 1 } ] }
                      { Id = encoder.Encode collection2.Id
                        Name = "Collection 2"
                        Description = None
                        CreatedAt = actualCollection2.CreatedAt
                        UpdatedAt = actualCollection2.CreatedAt
                        Vocabularies = [] } ]
                  DefaultVocabulary = None }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``GET returns only authenticated user's collections hierarchy``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! requesterIdentityUser, requesterWordfolioUser =
                factory.CreateUserAsync(306, "requester@example.com", "P@ssw0rd!")

            let! _, otherWordfolioUser = factory.CreateUserAsync(307, "other@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let requesterCollection =

                Entities.makeCollection
                    requesterWordfolioUser
                    "Requester Collection"
                    (Some "Requester Description")
                    now
                    now
                    false

            let requesterVocabulary =

                Entities.makeVocabulary
                    requesterCollection
                    "Requester Vocabulary"
                    (Some "Requester Vocab Description")
                    now
                    now
                    false

            let requesterEntry =
                Entities.makeEntry requesterVocabulary "requester-entry" now now

            let otherCollection =

                Entities.makeCollection otherWordfolioUser "Other Collection" (Some "Other Description") now now false

            let otherVocabulary =

                Entities.makeVocabulary
                    otherCollection
                    "Other Vocabulary"
                    (Some "Other Vocab Description")
                    now
                    now
                    false

            let otherEntry =
                Entities.makeEntry otherVocabulary "other-entry" now now

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ requesterWordfolioUser; otherWordfolioUser ]
                |> Seeder.addCollections [ requesterCollection; otherCollection ]
                |> Seeder.addVocabularies [ requesterVocabulary; otherVocabulary ]
                |> Seeder.addEntries [ requesterEntry; otherEntry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(requesterIdentityUser)

            let! response = client.GetAsync(Urls.CollectionsHierarchy.Path)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<CollectionsHierarchyResultResponse>()

            let actualCollection =
                Assert.Single(actual.Collections)

            let actualVocabulary =
                Assert.Single(actualCollection.Vocabularies)

            let expected: CollectionsHierarchyResultResponse =
                { Collections =
                    [ { Id = encoder.Encode requesterCollection.Id
                        Name = "Requester Collection"
                        Description = Some "Requester Description"
                        CreatedAt = actualCollection.CreatedAt
                        UpdatedAt = actualCollection.CreatedAt
                        Vocabularies =
                          [ { Id = encoder.Encode requesterVocabulary.Id
                              Name = "Requester Vocabulary"
                              Description = Some "Requester Vocab Description"
                              CreatedAt = actualVocabulary.CreatedAt
                              UpdatedAt = actualVocabulary.CreatedAt
                              EntryCount = 1 } ] } ]
                  DefaultVocabulary = None }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``GET excludes system collections``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(301, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let regularCollection =
                Entities.makeCollection wordfolioUser "Regular Collection" None now now false

            let systemCollection =
                Entities.makeCollection wordfolioUser "System Collection" None now now true

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ regularCollection; systemCollection ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.GetAsync(Urls.CollectionsHierarchy.Path)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<CollectionsHierarchyResultResponse>()

            let actualCollection =
                Assert.Single(actual.Collections)

            let expected: CollectionsHierarchyResultResponse =
                { Collections =
                    [ { Id = encoder.Encode regularCollection.Id
                        Name = "Regular Collection"
                        Description = None
                        CreatedAt = actualCollection.CreatedAt
                        UpdatedAt = actualCollection.CreatedAt
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

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(302, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Test Collection" None now now false

            let regularVocabulary =
                Entities.makeVocabulary collection "Regular Vocabulary" None now now false

            let defaultVocabulary =
                Entities.makeVocabulary collection "Default Vocabulary" None now now true

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

            let! actual = response.Content.ReadFromJsonAsync<CollectionsHierarchyResultResponse>()

            let actualCollection =
                Assert.Single(actual.Collections)

            let expected: CollectionsHierarchyResultResponse =
                { Collections =
                    [ { Id = encoder.Encode collection.Id
                        Name = "Test Collection"
                        Description = None
                        CreatedAt = actualCollection.CreatedAt
                        UpdatedAt = actualCollection.CreatedAt
                        Vocabularies =
                          [ { Id = encoder.Encode regularVocabulary.Id
                              Name = "Regular Vocabulary"
                              Description = None
                              CreatedAt = actualCollection.Vocabularies[0].CreatedAt
                              UpdatedAt = actualCollection.Vocabularies[0].CreatedAt
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

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(303, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.GetAsync(Urls.CollectionsHierarchy.Path)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")

            let! actual = response.Content.ReadFromJsonAsync<CollectionsHierarchyResultResponse>()

            let expected: CollectionsHierarchyResultResponse =
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

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(304, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let systemCollection =
                Entities.makeCollection wordfolioUser "Unsorted" None now now true

            let defaultVocabulary =
                Entities.makeVocabulary systemCollection "My Words" (Some "Default vocab") now now true

            let entry1 =
                Entities.makeEntry defaultVocabulary "word1" now now

            let entry2 =
                Entities.makeEntry defaultVocabulary "word2" now now

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

            let! actual = response.Content.ReadFromJsonAsync<CollectionsHierarchyResultResponse>()

            let expected: CollectionsHierarchyResultResponse =
                { Collections = []
                  DefaultVocabulary =
                    Some
                        { Id = encoder.Encode defaultVocabulary.Id
                          Name = "My Words"
                          Description = Some "Default vocab"
                          CreatedAt = defaultVocabulary.CreatedAt
                          UpdatedAt = defaultVocabulary.UpdatedAt
                          EntryCount = 2 } }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``GET does not return default vocabulary when it has no entries``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(305, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let systemCollection =
                Entities.makeCollection wordfolioUser "Unsorted" None now now true

            let defaultVocabulary =
                Entities.makeVocabulary systemCollection "My Words" None now now true

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

            let! actual = response.Content.ReadFromJsonAsync<CollectionsHierarchyResultResponse>()

            let expected: CollectionsHierarchyResultResponse =
                { Collections = []
                  DefaultVocabulary = None }

            Assert.Equal(expected, actual)
        }

    [<Fact>]
    member _.``GET /collections-hierarchy without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            use client = factory.CreateClient()

            let! response = client.GetAsync(Urls.CollectionsHierarchy.Path)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }
