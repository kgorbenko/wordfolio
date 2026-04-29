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

module GetSessionUrls = Wordfolio.Api.Urls.Exercises

type GetSessionByIdTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``GET session returns session bundle without attempts``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(921, "user@example.com", "P@ssw0rd!")

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

            let url =
                GetSessionUrls.sessionById(encoder.Encode session.Id)

            let! response = client.GetAsync(url)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let! actual = response.Content.ReadFromJsonAsync<SessionBundleResponse>()

            let expected: SessionBundleResponse =
                { SessionId = encoder.Encode session.Id
                  ExerciseType = ExerciseTypeDto.Translation
                  Entries =
                    [ { EntryId = encoder.Encode entry.Id
                        DisplayOrder = 0
                        PromptData = actual.Entries[0].PromptData
                        Attempt = None } ] }

            Assert.Equal(expected, actual)
            Assert.Equal(System.Text.Json.JsonValueKind.Object, actual.Entries[0].PromptData.ValueKind)
        }

    [<Fact>]
    member _.``GET session returns session bundle with attempt``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(922, "user@example.com", "P@ssw0rd!")

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

            let attempt =
                Entities.makeExerciseAttempt wordfolioUser (Some session) entry 1s promptData 1s "hola" true attemptedAt

            do!
                fixture.WordfolioSeeder
                |> Seeder.addExerciseAttempts [ attempt ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                GetSessionUrls.sessionById(encoder.Encode session.Id)

            let! response = client.GetAsync(url)
            let! body = response.Content.ReadAsStringAsync()

            Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}. Body: {body}")
            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let! actual = response.Content.ReadFromJsonAsync<SessionBundleResponse>()

            let expected: SessionBundleResponse =
                { SessionId = encoder.Encode session.Id
                  ExerciseType = ExerciseTypeDto.Translation
                  Entries =
                    [ { EntryId = encoder.Encode entry.Id
                        DisplayOrder = 0
                        PromptData = actual.Entries[0].PromptData
                        Attempt =
                          Some
                              { RawAnswer = "hola"
                                IsCorrect = true
                                AttemptedAt = attemptedAt } } ] }

            Assert.Equal(expected, actual)
            Assert.Equal(System.Text.Json.JsonValueKind.Object, actual.Entries[0].PromptData.ValueKind)
        }

    [<Fact>]
    member _.``GET session returns 401 when unauthenticated``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            use client = factory.CreateClient()

            let url =
                GetSessionUrls.sessionById(encoder.Encode 1)

            let! response = client.GetAsync(url)

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }

    [<Fact>]
    member _.``GET session returns 404 when session id is malformed``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let! identityUser, wordfolioUser = factory.CreateUserAsync(924, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                GetSessionUrls.sessionById "invalidSessionId999"

            let! response = client.GetAsync(url)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``GET session returns 404 when session does not exist``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! identityUser, wordfolioUser = factory.CreateUserAsync(925, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let url =
                GetSessionUrls.sessionById(encoder.Encode 999999)

            let! response = client.GetAsync(url)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }

    [<Fact>]
    member _.``GET session returns 404 when session belongs to another user``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            use factory =
                new WebApplicationFactory(fixture)

            let encoder = factory.Encoder

            let! _, ownerWordfolioUser = factory.CreateUserAsync(926, "owner@example.com", "P@ssw0rd!")

            let! requesterIdentityUser, requesterWordfolioUser =
                factory.CreateUserAsync(927, "requester@example.com", "P@ssw0rd!")

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

            let url =
                GetSessionUrls.sessionById(encoder.Encode ownerSession.Id)

            let! response = client.GetAsync(url)

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
        }
