module Wordfolio.Api.Domain.Tests.Exercises.TranslationTests

open System
open System.Text.Json

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Exercises

[<CLIMutable>]
type TranslationPromptDto =
    { entryText: string
      acceptedTranslations: string array }

let timestamp =
    DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

let makeTranslation id text =
    { Id = TranslationId id
      TranslationText = text
      Source = TranslationSource.Manual
      DisplayOrder = 0
      Examples = [] }

let makeEntry entryText translations =
    { Id = EntryId 1
      VocabularyId = VocabularyId 1
      EntryText = entryText
      CreatedAt = timestamp
      UpdatedAt = timestamp
      Definitions = []
      Translations = translations }

let deserializePrompt(PromptData json) =
    JsonSerializer.Deserialize<TranslationPromptDto>(json)

[<Fact>]
let ``generatePrompt includes all accepted translations and schema version 1``() =
    let entry =
        makeEntry "cat" [ makeTranslation 1 "hello"; makeTranslation 2 "bonjour" ]

    let prompt =
        Translation.generatePrompt entry

    Assert.Equal(1s, prompt.PromptSchemaVersion)

    let dto =
        deserializePrompt prompt.PromptData

    Assert.Equal("cat", dto.entryText)
    Assert.Equal<string array>([| "hello"; "bonjour" |], dto.acceptedTranslations)

[<Fact>]
let ``evaluate accepts matching translation with trim and case-insensitive comparison``() =
    let entry =
        makeEntry "cat" [ makeTranslation 1 "Hello" ]

    let prompt =
        Translation.generatePrompt entry

    let result =
        Translation.evaluate 1s prompt.PromptData (RawAnswer "  hello  ")

    Assert.Equal(Ok true, result)

[<Fact>]
let ``evaluate returns Ok false for wrong answer``() =
    let entry =
        makeEntry "cat" [ makeTranslation 1 "hello" ]

    let prompt =
        Translation.generatePrompt entry

    let result =
        Translation.evaluate 1s prompt.PromptData (RawAnswer "wrong")

    Assert.Equal(Ok false, result)

[<Fact>]
let ``evaluate returns MalformedPromptData for invalid JSON``() =
    let result =
        Translation.evaluate 1s (PromptData "not json") (RawAnswer "hello")

    Assert.Equal(Error EvaluateError.MalformedPromptData, result)

[<Fact>]
let ``evaluate returns UnsupportedPromptSchemaVersion for unsupported schema version``() =
    let result =
        Translation.evaluate 2s (PromptData "{}") (RawAnswer "hello")

    Assert.Equal(Error EvaluateError.UnsupportedPromptSchemaVersion, result)
