module Wordfolio.Api.Domain.Tests.Exercises.MultipleChoiceTests

open System
open System.Text.Json

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Exercises

[<CLIMutable>]
type OptionDto = { id: string; text: string }

[<CLIMutable>]
type MultipleChoicePromptDto =
    { entryText: string
      options: OptionDto array
      correctOptionId: string }

let timestamp =
    DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

let makeDefinition id text =
    { Id = DefinitionId id
      DefinitionText = text
      Source = DefinitionSource.Manual
      DisplayOrder = 0
      Examples = [] }

let makeTranslation id text =
    { Id = TranslationId id
      TranslationText = text
      Source = TranslationSource.Manual
      DisplayOrder = 0
      Examples = [] }

let makeEntry entryText translations definitions =
    { Id = EntryId 1
      VocabularyId = VocabularyId 1
      EntryText = entryText
      CreatedAt = timestamp
      UpdatedAt = timestamp
      Definitions = definitions
      Translations = translations }

let deserializePrompt(PromptData json) =
    JsonSerializer.Deserialize<MultipleChoicePromptDto>(json)

[<Fact>]
let ``generatePrompt returns schema version 1``() =
    let entry =
        makeEntry "cat" [ makeTranslation 1 "apple" ] []

    let prompt =
        MultipleChoice.generatePrompt entry

    Assert.Equal(1s, prompt.PromptSchemaVersion)

[<Fact>]
let ``generatePrompt sorts options and derives correct option from first translation``() =
    let entry =
        makeEntry
            "cat"
            [ makeTranslation 1 "apple" ]
            [ makeDefinition 1 "banana"
              makeDefinition 2 "cherry"
              makeDefinition 3 "date" ]

    let prompt =
        MultipleChoice.generatePrompt entry

    let dto =
        deserializePrompt prompt.PromptData

    Assert.Equal("cat", dto.entryText)
    Assert.Equal(4, dto.options.Length)

    Assert.Equal<string array>(
        [| "apple"; "banana"; "cherry"; "date" |],
        dto.options
        |> Array.map(fun o -> o.text)
    )

    Assert.Equal("a", dto.correctOptionId)

[<Fact>]
let ``generatePrompt allows fewer than 4 options when not enough distractors``() =
    let entry =
        makeEntry "cat" [ makeTranslation 1 "apple" ] []

    let prompt =
        MultipleChoice.generatePrompt entry

    let dto =
        deserializePrompt prompt.PromptData

    Assert.Equal(1, dto.options.Length)
    Assert.Equal("apple", dto.options[0].text)
    Assert.Equal("a", dto.correctOptionId)

[<Fact>]
let ``evaluate returns Ok true for correct answer``() =
    let entry =
        makeEntry "cat" [ makeTranslation 1 "apple" ] []

    let prompt =
        MultipleChoice.generatePrompt entry

    let dto =
        deserializePrompt prompt.PromptData

    let result =
        MultipleChoice.evaluate 1s prompt.PromptData (RawAnswer dto.correctOptionId)

    Assert.Equal(Ok true, result)

[<Fact>]
let ``evaluate returns Ok false for wrong answer``() =
    let entry =
        makeEntry "cat" [ makeTranslation 1 "apple" ] [ makeDefinition 1 "banana" ]

    let prompt =
        MultipleChoice.generatePrompt entry

    let dto =
        deserializePrompt prompt.PromptData

    let wrongOptionId =
        dto.options
        |> Array.find(fun o -> o.id <> dto.correctOptionId)
        |> (fun o -> o.id)

    let result =
        MultipleChoice.evaluate 1s prompt.PromptData (RawAnswer wrongOptionId)

    Assert.Equal(Ok false, result)

[<Fact>]
let ``evaluate is case-insensitive and trims whitespace``() =
    let entry =
        makeEntry "cat" [ makeTranslation 1 "apple" ] []

    let prompt =
        MultipleChoice.generatePrompt entry

    let dto =
        deserializePrompt prompt.PromptData

    let paddedAnswer =
        sprintf "  %s  " (dto.correctOptionId.ToUpper())

    let result =
        MultipleChoice.evaluate 1s prompt.PromptData (RawAnswer paddedAnswer)

    Assert.Equal(Ok true, result)

[<Fact>]
let ``evaluate returns MalformedPromptData for invalid JSON``() =
    let result =
        MultipleChoice.evaluate 1s (PromptData "not json") (RawAnswer "a")

    Assert.Equal(Error EvaluateError.MalformedPromptData, result)

[<Fact>]
let ``evaluate returns UnsupportedPromptSchemaVersion for unsupported schema version``() =
    let result =
        MultipleChoice.evaluate 2s (PromptData "{}") (RawAnswer "a")

    Assert.Equal(Error EvaluateError.UnsupportedPromptSchemaVersion, result)
