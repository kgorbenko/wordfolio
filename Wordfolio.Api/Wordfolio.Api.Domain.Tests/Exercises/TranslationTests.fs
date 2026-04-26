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

    let expected: TranslationPromptDto =
        { entryText = "cat"
          acceptedTranslations = [| "hello"; "bonjour" |] }

    Assert.Equivalent(expected, dto)

[<Fact>]
let ``evaluate returns Ok true for matching translation``() =
    let json =
        """{"entryText":"cat","acceptedTranslations":["hello"]}"""

    let result =
        Translation.evaluate 1s (PromptData json) (RawAnswer "hello")

    Assert.Equal(Ok true, result)

[<Fact>]
let ``evaluate is case-insensitive``() =
    let json =
        """{"entryText":"cat","acceptedTranslations":["Hello"]}"""

    let result =
        Translation.evaluate 1s (PromptData json) (RawAnswer "hello")

    Assert.Equal(Ok true, result)

[<Fact>]
let ``evaluate trims whitespace from answer``() =
    let json =
        """{"entryText":"cat","acceptedTranslations":["hello"]}"""

    let result =
        Translation.evaluate 1s (PromptData json) (RawAnswer "  hello  ")

    Assert.Equal(Ok true, result)

[<Fact>]
let ``evaluate accepts any translation in the accepted translations list``() =
    let json =
        """{"entryText":"cat","acceptedTranslations":["hello","bonjour"]}"""

    let result =
        Translation.evaluate 1s (PromptData json) (RawAnswer "bonjour")

    Assert.Equal(Ok true, result)

[<Fact>]
let ``evaluate returns Ok false for wrong answer``() =
    let json =
        """{"entryText":"cat","acceptedTranslations":["hello"]}"""

    let result =
        Translation.evaluate 1s (PromptData json) (RawAnswer "wrong")

    Assert.Equal(Ok false, result)

[<Fact>]
let ``evaluate returns Ok false when acceptedTranslations is empty``() =
    let json =
        """{"entryText":"cat","acceptedTranslations":[]}"""

    let result =
        Translation.evaluate 1s (PromptData json) (RawAnswer "hello")

    Assert.Equal(Ok false, result)

[<Fact>]
let ``evaluate returns MalformedPromptData for invalid JSON``() =
    let result =
        Translation.evaluate 1s (PromptData "not json") (RawAnswer "hello")

    Assert.Equal(Error EvaluateError.MalformedPromptData, result)

[<Fact>]
let ``evaluate returns MalformedPromptData when acceptedTranslations is null``() =
    let json = """{"entryText":"cat"}"""

    let result =
        Translation.evaluate 1s (PromptData json) (RawAnswer "hello")

    Assert.Equal(Error EvaluateError.MalformedPromptData, result)

[<Fact>]
let ``evaluate returns UnsupportedPromptSchemaVersion for unsupported schema version``() =
    let result =
        Translation.evaluate 2s (PromptData "{}") (RawAnswer "hello")

    Assert.Equal(Error EvaluateError.UnsupportedPromptSchemaVersion, result)
