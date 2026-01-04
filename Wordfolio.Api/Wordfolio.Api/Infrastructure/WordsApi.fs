module Wordfolio.Api.Infrastructure.WordsApi

open System
open System.Net.Http
open System.Text.Json
open System.Threading
open System.Threading.Tasks

[<CLIMutable>]
type WordsApiConfiguration =
    { BaseUrl: string
      Host: string
      ApiKey: string }

type DefinitionResultDto =
    { Definition: string
      PartOfSpeech: string
      Examples: string[] }

type ResponseDto =
    { Word: string
      Results: DefinitionResultDto[] }

type DefinitionResult =
    { Definition: string
      PartOfSpeech: string option
      Examples: string list }

type LookupResult =
    { Word: string
      Definitions: DefinitionResult list }

type WordsApiError =
    | NotFound
    | ApiError of string
    | NetworkError of string

type WordsApiClient(httpClient: HttpClient, configuration: WordsApiConfiguration) =

    let toDefinitionResult(dto: DefinitionResultDto) : DefinitionResult =
        { Definition = dto.Definition
          PartOfSpeech = Option.ofObj dto.PartOfSpeech
          Examples =
            if isNull dto.Examples then
                []
            else
                dto.Examples |> Array.toList }

    let toResult(dto: ResponseDto) : LookupResult =
        { Word = dto.Word
          Definitions =
            if isNull dto.Results then
                []
            else
                dto.Results
                |> Array.toList
                |> List.map toDefinitionResult }

    member _.LookupWordAsync
        (
            word: string,
            cancellationToken: CancellationToken
        ) : Task<Result<LookupResult, WordsApiError>> =
        task {
            try
                let url =
                    $"{configuration.BaseUrl}/{Uri.EscapeDataString(word)}"

                use request =
                    new HttpRequestMessage(HttpMethod.Get, url)

                request.Headers.Add("x-rapidapi-host", configuration.Host)
                request.Headers.Add("x-rapidapi-key", configuration.ApiKey)

                let! response = httpClient.SendAsync(request, cancellationToken)

                if response.StatusCode = System.Net.HttpStatusCode.NotFound then
                    return Error NotFound
                elif not response.IsSuccessStatusCode then
                    let! errorContent = response.Content.ReadAsStringAsync(cancellationToken)
                    return Error(ApiError $"HTTP {int response.StatusCode}: {errorContent}")
                else
                    let! content = response.Content.ReadAsStringAsync(cancellationToken)

                    let options =
                        JsonSerializerOptions(PropertyNameCaseInsensitive = true)

                    let dto =
                        JsonSerializer.Deserialize<ResponseDto>(content, options)

                    return Ok(toResult dto)
            with
            | :? HttpRequestException as ex -> return Error(NetworkError ex.Message)
            | :? TaskCanceledException as ex -> return Error(NetworkError $"Request timed out: {ex.Message}")
            | :? JsonException as ex -> return Error(ApiError $"Invalid JSON response: {ex.Message}")
        }
