namespace Wordfolio.Api.Tests

open System
open System.Net
open System.Net.ServerSentEvents
open System.Text
open System.Threading
open System.Threading.Tasks

open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.DependencyInjection.Extensions

open Xunit

open Wordfolio.Api.Infrastructure.ChatClient
open Wordfolio.Api.Tests.Utils
open Wordfolio.Api.Tests.Utils.FakeChatClient
open Wordfolio.Api.Tests.Utils.Wordfolio

module Urls = Wordfolio.Api.Urls

module private SseReader =
    let readEventsAsync(response: Net.Http.HttpResponseMessage) : Task<SseItem<string> list> =
        task {
            use! stream = response.Content.ReadAsStreamAsync()

            let parser =
                SseParser.Create(stream, (fun _ (data: ReadOnlySpan<byte>) -> Encoding.UTF8.GetString(data)))

            let events = ResizeArray<SseItem<string>>()

            let enumerator =
                parser.EnumerateAsync().GetAsyncEnumerator(CancellationToken.None)

            let mutable hasNext = true

            while hasNext do
                let! next = enumerator.MoveNextAsync()
                hasNext <- next

                if hasNext then
                    events.Add(enumerator.Current)

            return events |> List.ofSeq
        }

type DictionaryTests(fixture: WordfolioIdentityTestFixture) =
    interface IClassFixture<WordfolioIdentityTestFixture>

    [<Fact>]
    member _.``GET lookup returns SSE stream with text and result events``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            let fakeResponse =
                [ "[verb] To move quickly.\n"
                  "\"He *runs* every morning.\"\n\n"
                  "---JSON---\n"
                  "{\"definitions\":[{\"definition\":\"To move quickly\",\"partOfSpeech\":\"verb\"}]}" ]

            let fakeChatClient =
                FakeChatClient(fakeResponse) :> IChatClient

            use factory =
                new WebApplicationFactory(
                    fixture,
                    fun services ->
                        services.RemoveAll(typeof<IChatClient>)
                        |> ignore

                        services.AddSingleton<IChatClient>(fakeChatClient)
                        |> ignore
                )

            let! identityUser, wordfolioUser = factory.CreateUserAsync(300, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.GetAsync($"{Urls.Dictionary.Path}{Urls.Dictionary.Lookup}?text=run")

            Assert.Equal(HttpStatusCode.OK, response.StatusCode)

            let! events = SseReader.readEventsAsync response

            let expectedEvents =
                [ SseItem<string>("[verb] To move quickly.\n", eventType = "text")
                  SseItem<string>("\"He *runs* every morning.\"\n\n", eventType = "text")
                  SseItem<string>(
                      "{\"definitions\":[{\"definition\":\"To move quickly\",\"partOfSpeech\":\"verb\"}]}",
                      eventType = "result"
                  ) ]

            Assert.Equal<SseItem<string> list>(expectedEvents, events)
        }

    [<Fact>]
    member _.``GET lookup with empty text returns BadRequest``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            let fakeChatClient =
                FakeChatClient([]) :> IChatClient

            use factory =
                new WebApplicationFactory(
                    fixture,
                    fun services ->
                        services.RemoveAll(typeof<IChatClient>)
                        |> ignore

                        services.AddSingleton<IChatClient>(fakeChatClient)
                        |> ignore
                )

            let! identityUser, wordfolioUser = factory.CreateUserAsync(301, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.GetAsync($"{Urls.Dictionary.Path}{Urls.Dictionary.Lookup}?text=")

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }

    [<Fact>]
    member _.``GET lookup with whitespace text returns BadRequest``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            let fakeChatClient =
                FakeChatClient([]) :> IChatClient

            use factory =
                new WebApplicationFactory(
                    fixture,
                    fun services ->
                        services.RemoveAll(typeof<IChatClient>)
                        |> ignore

                        services.AddSingleton<IChatClient>(fakeChatClient)
                        |> ignore
                )

            let! identityUser, wordfolioUser = factory.CreateUserAsync(302, "user@example.com", "P@ssw0rd!")

            do!
                fixture.WordfolioSeeder
                |> Seeder.addUsers [ wordfolioUser ]
                |> Seeder.saveChangesAsync

            use! client = factory.CreateAuthenticatedClientAsync(identityUser)

            let! response = client.GetAsync($"{Urls.Dictionary.Path}{Urls.Dictionary.Lookup}?text=   ")

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        }

    [<Fact>]
    member _.``GET lookup without authentication fails``() : Task =
        task {
            do! fixture.ResetDatabaseAsync()

            let fakeChatClient =
                FakeChatClient([]) :> IChatClient

            use factory =
                new WebApplicationFactory(
                    fixture,
                    fun services ->
                        services.RemoveAll(typeof<IChatClient>)
                        |> ignore

                        services.AddSingleton<IChatClient>(fakeChatClient)
                        |> ignore
                )

            use client = factory.CreateClient()

            let! response = client.GetAsync($"{Urls.Dictionary.Path}{Urls.Dictionary.Lookup}?text=run")

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode)
        }
