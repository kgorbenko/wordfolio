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
let ``generatePrompt sorts options alphabetically``() =
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

    let expected: MultipleChoicePromptDto =
        { entryText = "cat"
          options =
            [| { id = "a"; text = "apple" }
               { id = "b"; text = "banana" }
               { id = "c"; text = "cherry" }
               { id = "d"; text = "date" } |]
          correctOptionId = "a" }

    Assert.Equivalent(expected, dto)

[<Fact>]
let ``generatePrompt derives correct option id from first translation``() =
    let entry =
        makeEntry
            "cat"
            [ makeTranslation 1 "cherry" ]
            [ makeDefinition 1 "apple"; makeDefinition 2 "banana"; makeDefinition 3 "date" ]

    let prompt =
        MultipleChoice.generatePrompt entry

    let dto =
        deserializePrompt prompt.PromptData

    let expected: MultipleChoicePromptDto =
        { entryText = "cat"
          options =
            [| { id = "a"; text = "apple" }
               { id = "b"; text = "banana" }
               { id = "c"; text = "cherry" }
               { id = "d"; text = "date" } |]
          correctOptionId = "c" }

    Assert.Equivalent(expected, dto)

[<Fact>]
let ``generatePrompt allows fewer than 4 options when not enough distractors``() =
    let entry =
        makeEntry "cat" [ makeTranslation 1 "apple" ] []

    let prompt =
        MultipleChoice.generatePrompt entry

    let dto =
        deserializePrompt prompt.PromptData

    let expected: MultipleChoicePromptDto =
        { entryText = "cat"
          options = [| { id = "a"; text = "apple" } |]
          correctOptionId = "a" }

    Assert.Equivalent(expected, dto)

[<Fact>]
let ``generatePrompt includes definition examples and translation examples as distractors``() =
    let definitionWithExample =
        { makeDefinition 1 "a domestic animal" with
            Examples =
                [ { Id = ExampleId 1
                    ExampleText = "in the wild"
                    Source = ExampleSource.Custom } ] }

    let translationWithExample =
        { makeTranslation 1 "apple" with
            Examples =
                [ { Id = ExampleId 2
                    ExampleText = "the apple tree"
                    Source = ExampleSource.Custom } ] }

    let entry =
        makeEntry "cat" [ translationWithExample ] [ definitionWithExample ]

    let prompt =
        MultipleChoice.generatePrompt entry

    let dto =
        deserializePrompt prompt.PromptData

    let expected: MultipleChoicePromptDto =
        { entryText = "cat"
          options =
            [| { id = "a"; text = "a domestic animal" }
               { id = "b"; text = "apple" }
               { id = "c"; text = "in the wild" }
               { id = "d"; text = "the apple tree" } |]
          correctOptionId = "b" }

    Assert.Equivalent(expected, dto)

[<Fact>]
let ``generatePrompt deduplicates distractor candidates``() =
    let entry =
        makeEntry
            "cat"
            [ makeTranslation 1 "apple" ]
            [ makeDefinition 1 "banana"
              makeDefinition 2 "banana"
              makeDefinition 3 "cherry" ]

    let prompt =
        MultipleChoice.generatePrompt entry

    let dto =
        deserializePrompt prompt.PromptData

    let expected: MultipleChoicePromptDto =
        { entryText = "cat"
          options =
            [| { id = "a"; text = "apple" }
               { id = "b"; text = "banana" }
               { id = "c"; text = "cherry" } |]
          correctOptionId = "a" }

    Assert.Equivalent(expected, dto)

[<Fact>]
let ``generatePrompt excludes distractors equal to correct answer text``() =
    let entry =
        makeEntry "cat" [ makeTranslation 1 "apple" ] [ makeDefinition 1 "APPLE"; makeDefinition 2 "banana" ]

    let prompt =
        MultipleChoice.generatePrompt entry

    let dto =
        deserializePrompt prompt.PromptData

    let expected: MultipleChoicePromptDto =
        { entryText = "cat"
          options = [| { id = "a"; text = "apple" }; { id = "b"; text = "banana" } |]
          correctOptionId = "a" }

    Assert.Equivalent(expected, dto)

[<Fact>]
let ``generatePrompt truncates to 3 distractors when more than 3 are available``() =
    let entry =
        makeEntry
            "cat"
            [ makeTranslation 1 "apple" ]
            [ makeDefinition 1 "banana"
              makeDefinition 2 "cherry"
              makeDefinition 3 "date"
              makeDefinition 4 "elderberry" ]

    let prompt =
        MultipleChoice.generatePrompt entry

    let dto =
        deserializePrompt prompt.PromptData

    let expected: MultipleChoicePromptDto =
        { entryText = "cat"
          options =
            [| { id = "a"; text = "apple" }
               { id = "b"; text = "banana" }
               { id = "c"; text = "cherry" }
               { id = "d"; text = "date" } |]
          correctOptionId = "a" }

    Assert.Equivalent(expected, dto)

[<Fact>]
let ``generatePrompt handles entry with no translations``() =
    let entry = makeEntry "cat" [] []

    let prompt =
        MultipleChoice.generatePrompt entry

    let dto =
        deserializePrompt prompt.PromptData

    let expected: MultipleChoicePromptDto =
        { entryText = "cat"
          options = [| { id = "a"; text = "" } |]
          correctOptionId = "a" }

    Assert.Equivalent(expected, dto)

[<Fact>]
let ``evaluate returns Ok true for correct answer``() =
    let json =
        """{"entryText":"cat","options":[{"id":"a","text":"apple"}],"correctOptionId":"a"}"""

    let result =
        MultipleChoice.evaluate 1s (PromptData json) (RawAnswer "a")

    Assert.Equal(Ok true, result)

[<Fact>]
let ``evaluate returns Ok false for wrong answer``() =
    let json =
        """{"entryText":"cat","options":[{"id":"a","text":"apple"},{"id":"b","text":"banana"}],"correctOptionId":"a"}"""

    let result =
        MultipleChoice.evaluate 1s (PromptData json) (RawAnswer "b")

    Assert.Equal(Ok false, result)

[<Fact>]
let ``evaluate is case-insensitive``() =
    let json =
        """{"entryText":"cat","options":[{"id":"a","text":"apple"}],"correctOptionId":"a"}"""

    let result =
        MultipleChoice.evaluate 1s (PromptData json) (RawAnswer "A")

    Assert.Equal(Ok true, result)

[<Fact>]
let ``evaluate trims whitespace from answer``() =
    let json =
        """{"entryText":"cat","options":[{"id":"a","text":"apple"}],"correctOptionId":"a"}"""

    let result =
        MultipleChoice.evaluate 1s (PromptData json) (RawAnswer "  a  ")

    Assert.Equal(Ok true, result)

[<Fact>]
let ``evaluate returns MalformedPromptData for invalid JSON``() =
    let result =
        MultipleChoice.evaluate 1s (PromptData "not json") (RawAnswer "a")

    Assert.Equal(Error EvaluateError.MalformedPromptData, result)

[<Fact>]
let ``evaluate returns MalformedPromptData when correctOptionId is null``() =
    let json =
        """{"entryText":"cat","options":[{"id":"a","text":"apple"}],"correctOptionId":null}"""

    let result =
        MultipleChoice.evaluate 1s (PromptData json) (RawAnswer "a")

    Assert.Equal(Error EvaluateError.MalformedPromptData, result)

[<Fact>]
let ``evaluate returns UnsupportedPromptSchemaVersion for unsupported schema version``() =
    let result =
        MultipleChoice.evaluate 2s (PromptData "{}") (RawAnswer "a")

    Assert.Equal(Error EvaluateError.UnsupportedPromptSchemaVersion, result)
