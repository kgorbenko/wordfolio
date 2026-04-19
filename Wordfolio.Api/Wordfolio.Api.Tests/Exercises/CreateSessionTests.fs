namespace Wordfolio.Api.Tests.Exercises

open System
open System.Net
open System.Net.Http.Json
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Api.Exercises.Types
open Wordfolio.Api.Tests
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.Wordfolio

module CreateSessionUrls = Wordfolio.Api.Urls.Exercises

type CreateSessionTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``POST creates session with vocabulary selector``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(901, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None now now false

            let entry =
                Entities.makeEntry vocabulary "hello" now now

            let translation =
                Entities.makeTranslation entry "hola" Wordfolio.Api.DataAccess.Translations.TranslationSource.Manual 0

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.addTranslations [ translation ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: CreateSessionRequest =
                { ExerciseType = ExerciseTypeDto.Translation
                  Selector =
                    { Type = "vocabulary"
                      VocabularyId = Some(encoder.Encode vocabulary.Id)
                      CollectionId = None
                      EntryIds = None
                      Count = None
                      Scope = None } }

            let! response = client.PostAsJsonAsync(CreateSessionUrls.Path, request)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.Created, response.StatusCode)

            let! actual = response.Content.ReadFromJsonAsync<SessionBundleResponse>()

            Assert.False(String.IsNullOrEmpty actual.SessionId)
            Assert.Equal(ExerciseTypeDto.Translation, actual.ExerciseType)
            Assert.Equal(1, actual.Entries.Length)
            Assert.Equal(System.Text.Json.JsonValueKind.Object, actual.Entries[0].PromptData.ValueKind)
        }

    [<Fact>]
    member _.``POST creates session with collection selector``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(902, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None now now false

            let entry =
                Entities.makeEntry vocabulary "hello" now now

            let translation =
                Entities.makeTranslation entry "hola" Wordfolio.Api.DataAccess.Translations.TranslationSource.Manual 0

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.addTranslations [ translation ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: CreateSessionRequest =
                { ExerciseType = ExerciseTypeDto.Translation
                  Selector =
                    { Type = "collection"
                      VocabularyId = None
                      CollectionId = Some(encoder.Encode collection.Id)
                      EntryIds = None
                      Count = None
                      Scope = None } }

            let! response = client.PostAsJsonAsync(CreateSessionUrls.Path, request)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.Created, response.StatusCode)

            let! actual = response.Content.ReadFromJsonAsync<SessionBundleResponse>()

            Assert.Equal(ExerciseTypeDto.Translation, actual.ExerciseType)
            Assert.Equal(1, actual.Entries.Length)
        }

    [<Fact>]
    member _.``POST creates session with explicitEntries selector``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(903, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None now now false

            let entry =
                Entities.makeEntry vocabulary "hello" now now

            let translation =
                Entities.makeTranslation entry "hola" Wordfolio.Api.DataAccess.Translations.TranslationSource.Manual 0

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.addTranslations [ translation ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: CreateSessionRequest =
                { ExerciseType = ExerciseTypeDto.Translation
                  Selector =
                    { Type = "explicitEntries"
                      VocabularyId = None
                      CollectionId = None
                      EntryIds = Some [ encoder.Encode entry.Id ]
                      Count = None
                      Scope = None } }

            let! response = client.PostAsJsonAsync(CreateSessionUrls.Path, request)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.Created, response.StatusCode)

            let! actual = response.Content.ReadFromJsonAsync<SessionBundleResponse>()

            Assert.Equal(ExerciseTypeDto.Translation, actual.ExerciseType)
            Assert.Equal(1, actual.Entries.Length)
            Assert.Equal(encoder.Encode entry.Id, actual.Entries[0].EntryId)
        }

    [<Fact>]
    member _.``POST creates session with worstKnown selector``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(904, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None now now false

            let entry =
                Entities.makeEntry vocabulary "hello" now now

            let translation =
                Entities.makeTranslation entry "hola" Wordfolio.Api.DataAccess.Translations.TranslationSource.Manual 0

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.addTranslations [ translation ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: CreateSessionRequest =
                { ExerciseType = ExerciseTypeDto.Translation
                  Selector =
                    { Type = "worstKnown"
                      VocabularyId = None
                      CollectionId = None
                      EntryIds = None
                      Count = Some 1
                      Scope = None } }

            let! response = client.PostAsJsonAsync(CreateSessionUrls.Path, request)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.Created, response.StatusCode)

            let! actual = response.Content.ReadFromJsonAsync<SessionBundleResponse>()

            Assert.Equal(ExerciseTypeDto.Translation, actual.ExerciseType)
            Assert.Equal(1, actual.Entries.Length)
        }

    [<Fact>]
    member _.``POST returns 401 when unauthenticated``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            use client = factory.CreateClient()

            let request: CreateSessionRequest =
                { ExerciseType = ExerciseTypeDto.Translation
                  Selector =
                    { Type = "vocabulary"
                      VocabularyId = Some "some-id"
                      CollectionId = None
                      EntryIds = None
                      Count = None
                      Scope = None } }

            let! response = client.PostAsJsonAsync(CreateSessionUrls.Path, request)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``POST returns 400 when selector body has unknown type``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(906, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: CreateSessionRequest =
                { ExerciseType = ExerciseTypeDto.Translation
                  Selector =
                    { Type = "unknownSelectorType"
                      VocabularyId = None
                      CollectionId = None
                      EntryIds = None
                      Count = None
                      Scope = None } }

            let! response = client.PostAsJsonAsync(CreateSessionUrls.Path, request)

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }

    [<Fact>]
    member _.``POST returns 400 when vocabulary selector has invalid sqid``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(907, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: CreateSessionRequest =
                { ExerciseType = ExerciseTypeDto.Translation
                  Selector =
                    { Type = "vocabulary"
                      VocabularyId = Some "invalidSessionId999"
                      CollectionId = None
                      EntryIds = None
                      Count = None
                      Scope = None } }

            let! response = client.PostAsJsonAsync(CreateSessionUrls.Path, request)

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }

    [<Fact>]
    member _.``POST returns 400 when explicitEntries count exceeds limit``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(908, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None now now false

            let entries =
                [ 1..11 ]
                |> List.map(fun i -> Entities.makeEntry vocabulary $"word{i}" now now)

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries entries
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: CreateSessionRequest =
                { ExerciseType = ExerciseTypeDto.Translation
                  Selector =
                    { Type = "explicitEntries"
                      VocabularyId = None
                      CollectionId = None
                      EntryIds =
                        Some(
                            entries
                            |> List.map(fun e -> encoder.Encode e.Id)
                        )
                      Count = None
                      Scope = None } }

            let! response = client.PostAsJsonAsync(CreateSessionUrls.Path, request)

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }

    [<Fact>]
    member _.``POST returns 400 when worstKnown count exceeds limit``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(909, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: CreateSessionRequest =
                { ExerciseType = ExerciseTypeDto.Translation
                  Selector =
                    { Type = "worstKnown"
                      VocabularyId = None
                      CollectionId = None
                      EntryIds = None
                      Count = Some 11
                      Scope = None } }

            let! response = client.PostAsJsonAsync(CreateSessionUrls.Path, request)

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }

    [<Fact>]
    member _.``POST returns 400 when no entries can be resolved``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(910, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None now now false

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let request: CreateSessionRequest =
                { ExerciseType = ExerciseTypeDto.Translation
                  Selector =
                    { Type = "vocabulary"
                      VocabularyId = Some(encoder.Encode vocabulary.Id)
                      CollectionId = None
                      EntryIds = None
                      Count = None
                      Scope = None } }

            let! response = client.PostAsJsonAsync(CreateSessionUrls.Path, request)

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }

    [<Fact>]
    member _.``POST returns 403 when vocabulary is owned by another user``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! _, ownerWordfolioUser = factory.CreateUserAsync(911, "owner@example.com", "P@ssw0rd!")

            let! requesterIdentityUser, requesterWordfolioUser =
                factory.CreateUserAsync(912, "requester@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let ownerCollection =
                Entities.makeCollection ownerWordfolioUser "Collection" None now now false

            let ownerVocabulary =
                Entities.makeVocabulary ownerCollection "Vocabulary" None now now false

            let ownerEntry =
                Entities.makeEntry ownerVocabulary "hello" now now

            let ownerTranslation =
                Entities.makeTranslation
                    ownerEntry
                    "hola"
                    Wordfolio.Api.DataAccess.Translations.TranslationSource.Manual
                    0

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ ownerWordfolioUser; requesterWordfolioUser ]
                |> Seeder.addCollections [ ownerCollection ]
                |> Seeder.addVocabularies [ ownerVocabulary ]
                |> Seeder.addEntries [ ownerEntry ]
                |> Seeder.addTranslations [ ownerTranslation ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(requesterIdentityUser)

            let request: CreateSessionRequest =
                { ExerciseType = ExerciseTypeDto.Translation
                  Selector =
                    { Type = "vocabulary"
                      VocabularyId = Some(encoder.Encode ownerVocabulary.Id)
                      CollectionId = None
                      EntryIds = None
                      Count = None
                      Scope = None } }

            let! response = client.PostAsJsonAsync(CreateSessionUrls.Path, request)

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode)

            let! sessions =
                fixture.WordfolioSeeder
                |> Seeder.getAllExerciseSessionsAsync

            Assert.Empty(sessions)
        }

    [<Fact>]
    member _.``POST returns 403 when explicit entry is owned by another user``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! _, ownerWordfolioUser = factory.CreateUserAsync(913, "owner@example.com", "P@ssw0rd!")

            let! requesterIdentityUser, requesterWordfolioUser =
                factory.CreateUserAsync(914, "requester@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let ownerCollection =
                Entities.makeCollection ownerWordfolioUser "Collection" None now now false

            let ownerVocabulary =
                Entities.makeVocabulary ownerCollection "Vocabulary" None now now false

            let ownerEntry =
                Entities.makeEntry ownerVocabulary "hello" now now

            let ownerTranslation =
                Entities.makeTranslation
                    ownerEntry
                    "hola"
                    Wordfolio.Api.DataAccess.Translations.TranslationSource.Manual
                    0

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ ownerWordfolioUser; requesterWordfolioUser ]
                |> Seeder.addCollections [ ownerCollection ]
                |> Seeder.addVocabularies [ ownerVocabulary ]
                |> Seeder.addEntries [ ownerEntry ]
                |> Seeder.addTranslations [ ownerTranslation ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(requesterIdentityUser)

            let request: CreateSessionRequest =
                { ExerciseType = ExerciseTypeDto.Translation
                  Selector =
                    { Type = "explicitEntries"
                      VocabularyId = None
                      CollectionId = None
                      EntryIds = Some [ encoder.Encode ownerEntry.Id ]
                      Count = None
                      Scope = None } }

            let! response = client.PostAsJsonAsync(CreateSessionUrls.Path, request)

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode)

            let! sessions =
                fixture.WordfolioSeeder
                |> Seeder.getAllExerciseSessionsAsync

            Assert.Empty(sessions)
        }
