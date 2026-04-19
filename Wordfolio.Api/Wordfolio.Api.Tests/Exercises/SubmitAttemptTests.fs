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

module SubmitUrls = Wordfolio.Api.Urls.Exercises

type SubmitAttemptTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``POST submit inserts attempt with correct evaluation``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(931, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None now now false

            let entry =
                Entities.makeEntry vocabulary "hello" now now

            let session =
                Entities.makeExerciseSession wordfolioUser 1s now

            let promptData =
                """{"entryText":"hello","acceptedTranslations":["hola"]}"""

            let sessionEntry =
                Entities.makeExerciseSessionEntry session entry 0 promptData 1s

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.addExerciseSessions [ session ]
                |> Seeder.addExerciseSessionEntries [ sessionEntry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let encodedSessionId =
                encoder.Encode session.Id

            let encodedEntryId = encoder.Encode entry.Id

            let attemptUrl =
                $"{SubmitUrls.sessionById encodedSessionId}/entries/{encodedEntryId}/attempts"

            let request: SubmitAttemptRequest =
                { RawAnswer = "hola" }

            let! response = client.PostAsJsonAsync(attemptUrl, request)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.Created, response.StatusCode)

            let! actual = response.Content.ReadFromJsonAsync<SubmitAttemptResponse>()
            Assert.True(actual.IsCorrect)

            let! attempts =
                fixture.WordfolioSeeder
                |> Seeder.getExerciseAttemptsBySessionIdAsync session.Id

            Assert.Equal(1, attempts.Length)
            Assert.Equal("hola", attempts[0].RawAnswer)
            Assert.True(attempts[0].IsCorrect)
        }

    [<Fact>]
    member _.``POST submit inserts attempt with incorrect evaluation``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(932, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None now now false

            let entry =
                Entities.makeEntry vocabulary "hello" now now

            let session =
                Entities.makeExerciseSession wordfolioUser 1s now

            let promptData =
                """{"entryText":"hello","acceptedTranslations":["hola"]}"""

            let sessionEntry =
                Entities.makeExerciseSessionEntry session entry 0 promptData 1s

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.addExerciseSessions [ session ]
                |> Seeder.addExerciseSessionEntries [ sessionEntry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let encodedSessionId =
                encoder.Encode session.Id

            let encodedEntryId = encoder.Encode entry.Id

            let attemptUrl =
                $"{SubmitUrls.sessionById encodedSessionId}/entries/{encodedEntryId}/attempts"

            let request: SubmitAttemptRequest =
                { RawAnswer = "wrong-answer" }

            let! response = client.PostAsJsonAsync(attemptUrl, request)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.Created, response.StatusCode)

            let! actual = response.Content.ReadFromJsonAsync<SubmitAttemptResponse>()
            Assert.False(actual.IsCorrect)

            let! attempts =
                fixture.WordfolioSeeder
                |> Seeder.getExerciseAttemptsBySessionIdAsync session.Id

            Assert.Equal(1, attempts.Length)
            Assert.Equal("wrong-answer", attempts[0].RawAnswer)
            Assert.False(attempts[0].IsCorrect)
        }

    [<Fact>]
    member _.``POST submit returns 200 for idempotent replay``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(933, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None now now false

            let entry =
                Entities.makeEntry vocabulary "hello" now now

            let session =
                Entities.makeExerciseSession wordfolioUser 1s now

            let promptData =
                """{"entryText":"hello","acceptedTranslations":["hola"]}"""

            let sessionEntry =
                Entities.makeExerciseSessionEntry session entry 0 promptData 1s

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.addExerciseSessions [ session ]
                |> Seeder.addExerciseSessionEntries [ sessionEntry ]
                |> Seeder.saveChangesAsync

            let attemptedAt =
                DateTimeOffset(2024, 1, 1, 13, 0, 0, TimeSpan.Zero)

            let existingAttempt =
                Entities.makeExerciseAttempt wordfolioUser (Some session) entry 1s promptData 1s "hola" true attemptedAt

            do!
                fixture.WordfolioSeeder
                |> Seeder.addExerciseAttempts [ existingAttempt ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let encodedSessionId =
                encoder.Encode session.Id

            let encodedEntryId = encoder.Encode entry.Id

            let attemptUrl =
                $"{SubmitUrls.sessionById encodedSessionId}/entries/{encodedEntryId}/attempts"

            let request: SubmitAttemptRequest =
                { RawAnswer = "hola" }

            let! response = client.PostAsJsonAsync(attemptUrl, request)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let! actual = response.Content.ReadFromJsonAsync<SubmitAttemptResponse>()
            Assert.True(actual.IsCorrect)

            let! attempts =
                fixture.WordfolioSeeder
                |> Seeder.getExerciseAttemptsBySessionIdAsync session.Id

            Assert.Equal(1, attempts.Length)
        }

    [<Fact>]
    member _.``POST submit returns 409 for conflicting replay``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(934, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None now now false

            let entry =
                Entities.makeEntry vocabulary "hello" now now

            let session =
                Entities.makeExerciseSession wordfolioUser 1s now

            let promptData =
                """{"entryText":"hello","acceptedTranslations":["hola"]}"""

            let sessionEntry =
                Entities.makeExerciseSessionEntry session entry 0 promptData 1s

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.addExerciseSessions [ session ]
                |> Seeder.addExerciseSessionEntries [ sessionEntry ]
                |> Seeder.saveChangesAsync

            let attemptedAt =
                DateTimeOffset(2024, 1, 1, 13, 0, 0, TimeSpan.Zero)

            let existingAttempt =
                Entities.makeExerciseAttempt wordfolioUser (Some session) entry 1s promptData 1s "hola" true attemptedAt

            do!
                fixture.WordfolioSeeder
                |> Seeder.addExerciseAttempts [ existingAttempt ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let encodedSessionId =
                encoder.Encode session.Id

            let encodedEntryId = encoder.Encode entry.Id

            let attemptUrl =
                $"{SubmitUrls.sessionById encodedSessionId}/entries/{encodedEntryId}/attempts"

            let request: SubmitAttemptRequest =
                { RawAnswer = "different-answer" }

            let! response = client.PostAsJsonAsync(attemptUrl, request)

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode)

            let! attempts =
                fixture.WordfolioSeeder
                |> Seeder.getExerciseAttemptsBySessionIdAsync session.Id

            Assert.Equal(1, attempts.Length)
            Assert.Equal("hola", attempts[0].RawAnswer)
        }

    [<Fact>]
    member _.``POST submit returns 401 when unauthenticated``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            use client = factory.CreateClient()

            let encodedSessionId = encoder.Encode 1
            let encodedEntryId = encoder.Encode 1

            let attemptUrl =
                $"{SubmitUrls.sessionById encodedSessionId}/entries/{encodedEntryId}/attempts"

            let request: SubmitAttemptRequest =
                { RawAnswer = "answer" }

            let! response = client.PostAsJsonAsync(attemptUrl, request)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``POST submit returns 404 when session id is malformed``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(936, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let encodedEntryId = encoder.Encode 1

            let malformedSessionPath =
                SubmitUrls.sessionById "invalidSessionId999"

            let attemptUrl =
                $"{malformedSessionPath}/entries/{encodedEntryId}/attempts"

            let request: SubmitAttemptRequest =
                { RawAnswer = "answer" }

            let! response = client.PostAsJsonAsync(attemptUrl, request)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``POST submit returns 404 when entry id is malformed``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(937, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let encodedSessionId = encoder.Encode 1

            let attemptUrl =
                $"{SubmitUrls.sessionById encodedSessionId}/entries/invalidEntryId999/attempts"

            let request: SubmitAttemptRequest =
                { RawAnswer = "answer" }

            let! response = client.PostAsJsonAsync(attemptUrl, request)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``POST submit returns 404 when session does not exist``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(938, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let encodedSessionId = encoder.Encode 999999
            let encodedEntryId = encoder.Encode 999999

            let attemptUrl =
                $"{SubmitUrls.sessionById encodedSessionId}/entries/{encodedEntryId}/attempts"

            let request: SubmitAttemptRequest =
                { RawAnswer = "answer" }

            let! response = client.PostAsJsonAsync(attemptUrl, request)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``POST submit returns 404 when session belongs to another user``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! _, ownerWordfolioUser = factory.CreateUserAsync(939, "owner@example.com", "P@ssw0rd!")

            let! requesterIdentityUser, requesterWordfolioUser =
                factory.CreateUserAsync(940, "requester@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let ownerCollection =
                Entities.makeCollection ownerWordfolioUser "Collection" None now now false

            let ownerVocabulary =
                Entities.makeVocabulary ownerCollection "Vocabulary" None now now false

            let ownerEntry =
                Entities.makeEntry ownerVocabulary "hello" now now

            let ownerSession =
                Entities.makeExerciseSession ownerWordfolioUser 1s now

            let promptData =
                """{"entryText":"hello","acceptedTranslations":["hola"]}"""

            let ownerSessionEntry =
                Entities.makeExerciseSessionEntry ownerSession ownerEntry 0 promptData 1s

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ ownerWordfolioUser; requesterWordfolioUser ]
                |> Seeder.addCollections [ ownerCollection ]
                |> Seeder.addVocabularies [ ownerVocabulary ]
                |> Seeder.addEntries [ ownerEntry ]
                |> Seeder.addExerciseSessions [ ownerSession ]
                |> Seeder.addExerciseSessionEntries [ ownerSessionEntry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(requesterIdentityUser)

            let encodedSessionId =
                encoder.Encode ownerSession.Id

            let encodedEntryId =
                encoder.Encode ownerEntry.Id

            let attemptUrl =
                $"{SubmitUrls.sessionById encodedSessionId}/entries/{encodedEntryId}/attempts"

            let request: SubmitAttemptRequest =
                { RawAnswer = "answer" }

            let! response = client.PostAsJsonAsync(attemptUrl, request)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``POST submit returns 404 when entry is not in session``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(941, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None now now false

            let entryInSession =
                Entities.makeEntry vocabulary "hello" now now

            let entryNotInSession =
                Entities.makeEntry vocabulary "world" now now

            let session =
                Entities.makeExerciseSession wordfolioUser 1s now

            let promptData =
                """{"entryText":"hello","acceptedTranslations":["hola"]}"""

            let sessionEntry =
                Entities.makeExerciseSessionEntry session entryInSession 0 promptData 1s

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entryInSession; entryNotInSession ]
                |> Seeder.addExerciseSessions [ session ]
                |> Seeder.addExerciseSessionEntries [ sessionEntry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let encodedSessionId =
                encoder.Encode session.Id

            let encodedEntryId =
                encoder.Encode entryNotInSession.Id

            let attemptUrl =
                $"{SubmitUrls.sessionById encodedSessionId}/entries/{encodedEntryId}/attempts"

            let request: SubmitAttemptRequest =
                { RawAnswer = "answer" }

            let! response = client.PostAsJsonAsync(attemptUrl, request)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``POST submit returns 500 when prompt schema version is unsupported``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(942, "user@example.com", "P@ssw0rd!")

            let now =
                DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero)

            let collection =
                Entities.makeCollection wordfolioUser "Collection" None now now false

            let vocabulary =
                Entities.makeVocabulary collection "Vocabulary" None now now false

            let entry =
                Entities.makeEntry vocabulary "hello" now now

            let session =
                Entities.makeExerciseSession wordfolioUser 1s now

            let promptData =
                """{"entryText":"hello","acceptedTranslations":["hola"]}"""

            let sessionEntry =
                Entities.makeExerciseSessionEntry session entry 0 promptData 99s

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.addCollections [ collection ]
                |> Seeder.addVocabularies [ vocabulary ]
                |> Seeder.addEntries [ entry ]
                |> Seeder.addExerciseSessions [ session ]
                |> Seeder.addExerciseSessionEntries [ sessionEntry ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let encodedSessionId =
                encoder.Encode session.Id

            let encodedEntryId = encoder.Encode entry.Id

            let attemptUrl =
                $"{SubmitUrls.sessionById encodedSessionId}/entries/{encodedEntryId}/attempts"

            let request: SubmitAttemptRequest =
                { RawAnswer = "hola" }

            let! response = client.PostAsJsonAsync(attemptUrl, request)

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode)
        }
